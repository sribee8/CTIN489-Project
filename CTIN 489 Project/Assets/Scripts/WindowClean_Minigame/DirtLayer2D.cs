using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class DirtLayer2D : MonoBehaviour
{
    [Header("Brush world units")]
    public float brushWidthWorld = 1.5f;
    public float brushHeightWorld = 0.22f;
    public float featherPixels = 6f;
    public float eraseStrength = 1f;

    [Header("Texture")]
    public int texSize = 512;

    [Header("Guide and goop")]
    public GuidePath2D guide;                 // assign GuidePath2D here
    public SpriteRenderer goopSR;             // leave empty to use own SpriteRenderer
    public Color goopColor = new Color(0.80f, 0.95f, 1f, 1f);
    [Range(0f, 0.2f)] public float clearAlphaThreshold = 0.02f;

    Texture2D dirtTex;
    SpriteRenderer sr;

    int cleanedPath;
    int offPathCleaned;
    int pathTotal;
    int strokes;

    bool dragging;
    Vector2 lastUV;
    bool hasLastUV;

    bool blockUntilMouseUp;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!goopSR) goopSR = sr;
    }

    void Start() { BuildFresh(goopColor); }

    void BuildFresh(Color fill)
    {
        if (dirtTex == null || dirtTex.width != texSize)
            dirtTex = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false);

        var buf = new Color32[texSize * texSize];

        if (guide != null && guide.Texture != null)
        {
            var guidePx = guide.GetPixels32();
            var c32 = (Color32)fill; c32.a = 255;

            int n = Mathf.Min(buf.Length, guidePx.Length);
            for (int i = 0; i < n; i++)
            {
                if (guidePx[i].a > 0)
                    buf[i] = c32;
                else
                    buf[i] = new Color32(0, 0, 0, 0);
            }
        }
        else
        {
            var c32 = (Color32)fill; c32.a = 255;
            for (int i = 0; i < buf.Length; i++) buf[i] = c32;
        }

        dirtTex.SetPixels32(buf);
        dirtTex.Apply();

        goopSR.color = Color.white;
        goopSR.sprite = Sprite.Create(dirtTex, new Rect(0, 0, texSize, texSize),
                                      new Vector2(0.5f, 0.5f), texSize);

        var fit = GetComponent<FitSpriteToCamera2D>();
        if (fit) fit.FitNow();

        strokes = 0;
        cleanedPath = 0;
        offPathCleaned = 0;
        pathTotal = (guide != null) ? Mathf.Max(1, guide.PathPixels) : texSize * texSize;

        dragging = false;
        hasLastUV = false;

        blockUntilMouseUp = true;
        StartCoroutine(WaitForMouseUpToUnblock());
    }

    System.Collections.IEnumerator WaitForMouseUpToUnblock()
    {
        while (Input.GetMouseButton(0)) yield return null;
        blockUntilMouseUp = false;
    }

    public bool InputBlocked => blockUntilMouseUp;

    // called by the squeegee
    public void BeginDrag()
    {
        if (blockUntilMouseUp) return;
        dragging = true;
        strokes++;
        hasLastUV = false;
    }

    public void EndDrag()   { dragging = false; hasLastUV = false; }

    public void DragAtWorld(Vector3 worldPos)
    {
        if (!dragging) return;

        Vector3 local = transform.InverseTransformPoint(worldPos);
        var uv = new Vector2(local.x + 0.5f, local.y + 0.5f);
        PaintAlongPath(uv);
    }

    public void BeginStroke() => BeginDrag();
    public void EndStroke()   => EndDrag();
    public void EraseAtWorld(Vector2 worldPos) => DragAtWorld(worldPos);

    void PaintAlongPath(Vector2 uv)
    {
        if (!hasLastUV)
        {
            StampAtUV(uv);
            lastUV = uv;
            hasLastUV = true;
            return;
        }

        Vector2 delta = uv - lastUV;
        float dist = delta.magnitude;
        int steps = Mathf.CeilToInt(dist * texSize * 1.2f);
        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 p = Vector2.Lerp(lastUV, uv, t);
            StampAtUV(p);
        }
        lastUV = uv;
    }

    void StampAtUV(Vector2 uv)
    {
        Vector2 worldSize = sr.bounds.size;
        float pxPerWorldX = texSize / Mathf.Max(worldSize.x, 0.0001f);
        float pxPerWorldY = texSize / Mathf.Max(worldSize.y, 0.0001f);

        int halfW = Mathf.Max(1, Mathf.RoundToInt(0.5f * brushWidthWorld * pxPerWorldX));
        int halfH = Mathf.Max(1, Mathf.RoundToInt(0.5f * brushHeightWorld * pxPerWorldY));

        int cx = Mathf.Clamp(Mathf.RoundToInt(uv.x * (texSize - 1)), 0, texSize - 1);
        int cy = Mathf.Clamp(Mathf.RoundToInt(uv.y * (texSize - 1)), 0, texSize - 1);

        int soft = Mathf.RoundToInt(featherPixels);
        int minY = Mathf.Clamp(cy - halfH - soft, 0, texSize - 1);
        int maxY = Mathf.Clamp(cy + halfH + soft, 0, texSize - 1);
        int minX = Mathf.Clamp(cx - halfW - soft, 0, texSize - 1);
        int maxX = Mathf.Clamp(cx + halfW + soft, 0, texSize - 1);

        for (int y = minY; y <= maxY; y++)
        {
            int dy = Mathf.Abs(y - cy);
            for (int x = minX; x <= maxX; x++)
            {
                int dx = Mathf.Abs(x - cx);

                float edgeDist = Mathf.Max(dx - halfW, dy - halfH);
                float erase = 0f;

                if (edgeDist <= 0f) erase = eraseStrength;
                else if (edgeDist <= soft)
                {
                    float t = Mathf.Clamp01(edgeDist / Mathf.Max(1f, soft));
                    t = t * t * (3f - 2f * t);
                    erase = eraseStrength * (1f - t);
                }
                else continue;

                Color c = dirtTex.GetPixel(x, y);
                if (c.a <= 0f) continue;

                float oldA = c.a;
                float newA = Mathf.Max(0f, oldA - erase);
                if (newA < oldA)
                {
                    bool crossed = (oldA > clearAlphaThreshold && newA <= clearAlphaThreshold);
                    bool onPath = (guide != null) ? guide.IsOnPathPixel(x, y) : true;

                    if (crossed)
                    {
                        if (onPath) cleanedPath++;
                        else        offPathCleaned++;
                    }

                    c.a = newA;
                    c.r = goopColor.r; c.g = goopColor.g; c.b = goopColor.b;
                    dirtTex.SetPixel(x, y, c);
                }
            }
        }
        dirtTex.Apply(false);
    }

    public float CleanPercent()
    {
        if (pathTotal <= 0) return 0f;
        return 100f * Mathf.Clamp01((float)cleanedPath / pathTotal);
    }
    public int RemainingPixels() => Mathf.Max(0, pathTotal - cleanedPath);
    public int OffPathCleaned() => offPathCleaned;
    public int StrokeCount() => strokes;

    public void ResetPane()
    {
        dragging = false;
        hasLastUV = false;
        BuildFresh(goopColor);
    }
}
