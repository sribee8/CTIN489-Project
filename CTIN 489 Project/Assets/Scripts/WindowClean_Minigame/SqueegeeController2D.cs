using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class SqueegeeController2D : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public DirtLayer2D dirt;
    public BoxCollider2D paneCollider;

    [Header("Motion")]
    public float followLerp = 14f;

    [Header("Brush world units")]
    public float brushWidthWorld = 1.5f;
    public float brushHeightWorld = 0.22f;

    [Header("Visuals")]
    public bool matchBrushSize = true;

    Vector3 vel;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!cam) cam = Camera.main;
    }

    void Start()
    {
        if (dirt)
        {
            dirt.brushWidthWorld  = brushWidthWorld;
            dirt.brushHeightWorld = brushHeightWorld;
        }

        if (matchBrushSize && sr && sr.sprite != null && cam)
        {
            var cur = sr.bounds.size;
            if (cur.x > 0f && cur.y > 0f)
            {
                float sx = brushWidthWorld  / cur.x;
                float sy = brushHeightWorld / cur.y;
                var ls = transform.localScale;
                transform.localScale = new Vector3(ls.x * sx, ls.y * sy, ls.z);
            }
        }
    }

    void LateUpdate()
    {
        if (!cam || !dirt || !paneCollider) return;

        var mp = Input.mousePosition;
        mp.z = Mathf.Abs(cam.transform.position.z);
        var target = cam.ScreenToWorldPoint(mp);
        transform.position = Vector3.SmoothDamp(
            transform.position, target, ref vel, 1f / Mathf.Max(1f, followLerp));

        bool inside = paneCollider.OverlapPoint(transform.position);
        bool overUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();

        if (Input.GetMouseButtonDown(0) && inside && !overUI && !dirt.InputBlocked)
            dirt.BeginDrag();

        if (Input.GetMouseButton(0))
        {
            if (inside)
            {
                Vector2 p = paneCollider.ClosestPoint(transform.position);
                dirt.DragAtWorld(p);
            }
            else
            {
                dirt.EndDrag();
            }
        }

        if (Input.GetMouseButtonUp(0))
            dirt.EndDrag();
    }

    void OnDisable() { if (dirt) dirt.EndDrag(); }
}
