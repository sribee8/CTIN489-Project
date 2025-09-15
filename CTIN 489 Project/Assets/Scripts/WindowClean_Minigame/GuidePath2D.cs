using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(SpriteRenderer))]
public class GuidePath2D : MonoBehaviour
{
    [Header("Texture")]
    public int texSize = 512;
    public Color guideColor = new Color(0f, 1f, 0.8f, 0.85f);
    public int bandRadiusPx = 18;

    [Header("Serpentine layout")]
    [Range(0.02f, 0.25f)] public float margin01 = 0.08f;
    [Range(2, 20)] public int columns = 6;

    Texture2D tex;
    SpriteRenderer sr;
    int pathPixels;

    void OnEnable()   { Build(); }
    void Start()      { Build(); }
    void OnValidate() { Build(); }

    [ContextMenu("Rebuild Guide")]
    public void Build()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!sr) return;

        if (tex == null || tex.width != texSize)
            tex = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false);

        var clear = new Color32[texSize * texSize];
        for (int i = 0; i < clear.Length; i++) clear[i] = new Color32(0, 0, 0, 0);
        tex.SetPixels32(clear);

        var pts = MakeSerpentinePoints();
        for (int i = 0; i < pts.Count - 1; i++)
            DrawSegment(pts[i], pts[i + 1]);

        tex.Apply();

        sr.color = Color.white;
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, texSize, texSize),
                                  new Vector2(0.5f, 0.5f), texSize);

        var fit = GetComponent<FitSpriteToCamera2D>();
        if (fit) fit.FitNow();

        CountPathPixels();
    }

    List<Vector2> MakeSerpentinePoints()
    {
        float left = margin01;
        float right = 1f - margin01;
        float top = 1f - margin01;
        float bottom = margin01;

        var list = new List<Vector2>();

        for (int i = 0; i < columns; i++)
        {
            float t = (columns == 1) ? 0f : i / (float)(columns - 1);
            float x = Mathf.Lerp(left, right, t);
            bool goDown = (i % 2 == 0);

            list.Add(new Vector2(x, goDown ? top : bottom));
            list.Add(new Vector2(x, goDown ? bottom : top));

            if (i < columns - 1)
            {
                float nextT = (i + 1) / (float)(columns - 1);
                float nextX = Mathf.Lerp(left, right, nextT);
                float yTurn = goDown ? bottom : top;
                list.Add(new Vector2(nextX, yTurn));
            }
        }
        return list;
    }

    void DrawSegment(Vector2 a01, Vector2 b01)
    {
        Vector2 a = new Vector2(Mathf.Clamp01(a01.x) * texSize, Mathf.Clamp01(a01.y) * texSize);
        Vector2 b = new Vector2(Mathf.Clamp01(b01.x) * texSize, Mathf.Clamp01(b01.y) * texSize);

        int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b)));
        for (int s = 0; s <= steps; s++)
        {
            float t = s / (float)steps;
            Vector2 p = Vector2.Lerp(a, b, t);
            StampDisc(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), bandRadiusPx);
        }
    }

    void StampDisc(int cx, int cy, int r)
    {
        int r2 = r * r;
        int minY = Mathf.Max(0, cy - r), maxY = Mathf.Min(texSize - 1, cy + r);
        int minX = Mathf.Max(0, cx - r), maxX = Mathf.Min(texSize - 1, cx + r);

        for (int y = minY; y <= maxY; y++)
        {
            int dy = y - cy;
            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - cx;
                if (dx * dx + dy * dy > r2) continue;

                Color bg = tex.GetPixel(x, y);
                float a0 = bg.a, a1 = guideColor.a;
                float outA = a0 + a1 * (1f - a0);
                Color outC = (bg * a0 + guideColor * a1 * (1f - a0)) / Mathf.Max(outA, 0.0001f);
                outC.a = outA;
                tex.SetPixel(x, y, outC);
            }
        }
    }

    void CountPathPixels()
    {
        pathPixels = 0;
        var px = tex.GetPixels32();
        for (int i = 0; i < px.Length; i++) if (px[i].a > 0) pathPixels++;
        if (pathPixels < 1) pathPixels = 1;
    }

    public bool IsOnPathPixel(int px, int py)
    {
        if (px < 0 || px >= texSize || py < 0 || py >= texSize) return false;
        return tex.GetPixel(px, py).a > 0f;
    }

    public int PathPixels => pathPixels;

    public Texture2D Texture => tex;

    public Color32[] GetPixels32()
    {
        if (tex == null) return null;
        return tex.GetPixels32();
    }
}
