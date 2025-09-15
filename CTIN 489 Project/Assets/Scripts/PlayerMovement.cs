using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;      // Speed of player movement
    public float jumpForce = 12f;     // Force applied when jumping
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    public Color cleanWindow;
    private Vector3 respawnPoint;

    public WaterManager waterMan;
    public PlayerAudio playerAudio;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        respawnPoint = transform.position;
    }

    void Update()
    {
        // Horizontal movement
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Jumping
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = respawnPoint;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            Destroy(collision.transform.parent.gameObject);
            waterMan.addWater();
            playerAudio.PlayPickupWater();
        }

        if (collision.gameObject.CompareTag("Window") && waterMan.canClean())
        {
            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.color = cleanWindow; 
            }

            waterMan.clearWater();
            playerAudio.PlayCleanWindow();
            respawnPoint = transform.position;
            jumpForce += 1f;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
