using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;      // Speed of player movement
    public float jumpForce = 12f;     // Force applied when jumping
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private Vector3 respawnPoint;

    public WaterManager waterMan;
    public PlayerAudio playerAudio;
    public GameObject cleanWindowText;

    private Window currWindow;
    private bool nearWindow;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        respawnPoint = transform.position;
        cleanWindowText.SetActive(false);
        nearWindow = false;
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

        // Respawning
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = respawnPoint;
        }

        // Check if fallen
        if (transform.position.y <= -5) transform.position = respawnPoint;

        if (Input.GetKeyDown(KeyCode.E) && nearWindow)
        {
            CleanWindow();
            cleanWindowText.SetActive(false);
        }
    }

    void CleanWindow()
    {
        waterMan.clearWater();
        playerAudio.PlayCleanWindow();
        respawnPoint = transform.position;
        jumpForce += 1f;
        currWindow.LoadWindowCleaning();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Ground should have an upward normal
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // picking up water
        if (collision.gameObject.CompareTag("Water"))
        {
            Destroy(collision.transform.parent.gameObject);
            waterMan.addWater();
            playerAudio.PlayPickupWater();
        }

        // cleaning window **if** the window is uncleaned
        if (collision.gameObject.CompareTag("Window") && waterMan.canClean() && !collision.gameObject.GetComponent<Window>().isClean())
        {
            cleanWindowText.SetActive(true);
            currWindow = collision.gameObject.GetComponent<Window>();
            nearWindow = true;

        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Window"))
        {
            if (cleanWindowText != null)
                cleanWindowText.SetActive(false);
            nearWindow = false;
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
