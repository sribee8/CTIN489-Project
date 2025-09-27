using UnityEngine;
using UnityEngine.UI;

public class GraffitiEraserUI : MonoBehaviour
{
    [Header("Graffiti Setup")]
    public Texture2D graffitiTexture;    // Original PNG (Read/Write Enabled)
    private Texture2D workingTexture;    // runtime copy
    private Image graffitiImage;         // UI Image this is attached to

    [Header("Squeegee Setup")]
    public RectTransform squeegee;       // squeegee GameObject (RectTransform)
    public Vector2 headOffset = Vector2.zero; // offset from squeegee pivot to the head (in UI units)

    [Header("Head Size (UI units)")]
    public float headWidthUI = 100f;
    public float headHeightUI = 20f;

    [Header("Completion")]
    [Range(0f, 1f)]
    public float completionThreshold = 0.95f;
    public Window windowToNotify;
    private bool completed = false;

    // Tracking previous position for interpolation
    private Vector2 lastPosTex;
    private bool hadLastPos = false;

    void Start()
    {
        graffitiImage = GetComponent<Image>();

        // copy texture so we don't modify the original asset
        int downscaleFactor = 2; // 2 = half resolution, 4 = quarter resolution
        int width = graffitiTexture.width / downscaleFactor;
        int height = graffitiTexture.height / downscaleFactor;

        workingTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // copy original texture into the smaller texture
        Color[] pixels = graffitiTexture.GetPixels();
        Color[] downscaledPixels = new Color[width * height];

        // simple nearest-neighbor downscale
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int origX = x * downscaleFactor;
                int origY = y * downscaleFactor;
                downscaledPixels[y * width + x] = pixels[origY * graffitiTexture.width + origX];
            }
        }

        workingTexture.SetPixels(downscaledPixels);
        workingTexture.Apply();

        graffitiImage.sprite = Sprite.Create(
            workingTexture,
            new Rect(0, 0, workingTexture.width, workingTexture.height),
            new Vector2(0.5f, 0.5f)
        );
        graffitiImage.preserveAspect = graffitiImage.preserveAspect;
    }

    void Update()
    {
        // Move squeegee graphic to mouse
        squeegee.position = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            EraseAt();
        }
        else
        {
            // Reset lastPos so next drag doesn't draw a huge line
            hadLastPos = false;
        }
    }

    void EraseAt()
    {
        Canvas canvas = graffitiImage.canvas;
        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        Vector3 headScreenPos = squeegee.position + (Vector3)headOffset;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            graffitiImage.rectTransform,
            headScreenPos,
            cam,
            out Vector2 localPos)
        ) return;

        Rect imgRect = graffitiImage.rectTransform.rect;
        Sprite sprite = graffitiImage.sprite;
        if (sprite == null) return;

        Rect spriteTexRect = sprite.textureRect;
        float spritePixelW = spriteTexRect.width;
        float spritePixelH = spriteTexRect.height;

        // Drawn area in UI units (accounts for preserveAspect)
        float rectW = imgRect.width;
        float rectH = imgRect.height;
        float spriteAspect = spritePixelW / spritePixelH;
        float rectAspect = rectW / rectH;

        float drawW, drawH;
        if (graffitiImage.preserveAspect)
        {
            if (spriteAspect > rectAspect)
            {
                drawW = rectW;
                drawH = rectW / spriteAspect;
            }
            else
            {
                drawH = rectH;
                drawW = rectH * spriteAspect;
            }
        }
        else
        {
            drawW = rectW;
            drawH = rectH;
        }

        float xInDraw = localPos.x + drawW * 0.5f;
        float yInDraw = localPos.y + drawH * 0.5f;

        if (xInDraw < 0 || xInDraw > drawW || yInDraw < 0 || yInDraw > drawH) return;

        // normalized UV inside the drawn sprite
        float u = xInDraw / drawW;
        float v = yInDraw / drawH;

        float texXf = spriteTexRect.x + u * spritePixelW;
        float texYf = spriteTexRect.y + v * spritePixelH;

        int texCx = Mathf.RoundToInt(texXf);
        int texCy = Mathf.RoundToInt(texYf);

        // convert head size to texture pixels
        float headW_px = (headWidthUI / drawW) * spritePixelW;
        float headH_px = (headHeightUI / drawH) * spritePixelH;
        int halfW = Mathf.Max(1, Mathf.RoundToInt(headW_px * 0.5f));
        int halfH = Mathf.Max(1, Mathf.RoundToInt(headH_px * 0.5f));

        // interpolate along line from last position
        Vector2 currentPosTex = new Vector2(texCx, texCy);
        if (hadLastPos)
        {
            DrawLine(lastPosTex, currentPosTex, halfW, halfH);
        }
        else
        {
            EraseRectangle(texCx, texCy, halfW, halfH);
        }
        lastPosTex = currentPosTex;
        hadLastPos = true;

        workingTexture.Apply();

        // check completion
        if (!completed && windowToNotify != null)
        {
            if (GetClearedFraction() >= completionThreshold)
            {
                completed = true;
                windowToNotify.CompleteMinigame();
            }
        }
    }

    void EraseRectangle(int centerX, int centerY, int halfW, int halfH)
    {
        int texW = workingTexture.width;
        int texH = workingTexture.height;

        for (int x = -halfW; x <= halfW; x++)
        {
            for (int y = -halfH; y <= halfH; y++)
            {
                float nx = (float)x / halfW;
                float ny = (float)y / halfH;
                if (nx * nx + ny * ny > 1f) continue; // circular brush

                int px = centerX + x;
                int py = centerY + y;
                if (px < 0 || px >= texW || py < 0 || py >= texH) continue;

                Color c = workingTexture.GetPixel(px, py);
                c.a = 0f;
                workingTexture.SetPixel(px, py, c);
            }
        }
    }

    void DrawLine(Vector2 start, Vector2 end, int halfW, int halfH)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(start, end, t);
            EraseRectangle(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), halfW, halfH);
        }
    }

    float GetClearedFraction()
    {
        Color[] pixels = workingTexture.GetPixels();
        int total = pixels.Length;
        int cleared = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a <= 0.01f) cleared++;
        }

        return (float)cleared / total;
    }
}
