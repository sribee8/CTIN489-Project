using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float moveDistance = 3f;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private int direction = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        startPos = rb.position;
    }

    void FixedUpdate()
    {
        Vector2 newPos = rb.position + Vector2.right * direction * speed * Time.fixedDeltaTime;
        Vector2 velocity = (newPos - rb.position) / Time.fixedDeltaTime;
        rb.linearVelocity = velocity;

        // Reverse direction
        if (Mathf.Abs(newPos.x - startPos.x) >= moveDistance)
        {
            direction *= -1;
        }
    }
}
