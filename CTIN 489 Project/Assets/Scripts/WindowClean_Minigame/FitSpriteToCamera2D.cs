using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class FitSpriteToCamera2D : MonoBehaviour
{
    public Camera cam;

    void Reset() { cam = Camera.main; }
    void OnEnable() { FitNow(); }
    void OnValidate() { FitNow(); }

    public void FitNow()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (!sr || sr.sprite == null) return;
        if (!cam) cam = Camera.main;
        if (!cam || !cam.orthographic) return;

        var oldScale = transform.localScale;
        transform.localScale = Vector3.one;

        var size = sr.bounds.size;
        if (size.x <= 0 || size.y <= 0)
        {
            transform.localScale = oldScale;
            return;
        }

        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * cam.aspect;

        transform.localScale = new Vector3(worldW / size.x, worldH / size.y, 1f);
    }
}
