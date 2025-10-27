using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using USCG.Core.Telemetry;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float climbSpeed = 4f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float horizontal;
    private float vertical;
    private bool isGrounded;
    private bool isClimbing;
    private Vector3 respawnPoint;

    [Header("References")]
    public WaterManager waterMan;
    public PlayerAudio playerAudio;
    public GameObject cleanWindowText;

    private Window currWindow;
    private bool nearWindow;
    public int numCleaned = 0;

    // Timing and telemetry
    private float windowStartTime;
    private List<float> windowTimes = new();
    private MetricId windowTimeMetric;
    private MetricId respawnMetric;

    private static PlayerMovement instance;

    void Awake()
    {
        instance = this;

    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        respawnPoint = transform.position;
        cleanWindowText.SetActive(false);
        nearWindow = false;
        isClimbing = false;

        // Start timer when level begins
        windowStartTime = Time.time;

        // Create telemetry metrics
        windowTimeMetric = TelemetryManager.instance.CreateSampledMetric<float>("WindowCompletionTime");
        respawnMetric = TelemetryManager.instance.CreateAccumulatedMetric("RespawnCount");

        // Listen for new scenes (resets timing per scene)
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Flip sprite based on horizontal movement
        if (horizontal > 0.01f)
            spriteRenderer.flipX = false;  // facing right
        else if (horizontal < -0.01f)
            spriteRenderer.flipX = true;   // facing left

        // Jumping
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Respawning
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = respawnPoint;
            TelemetryManager.instance.AccumulateMetric(respawnMetric, 1);
        }

        // Check if fallen
        if (transform.position.y <= -5)
        {
            transform.position = respawnPoint;
            TelemetryManager.instance.AccumulateMetric(respawnMetric, 1);
        }

        // Cleaning window
        if (Input.GetKeyDown(KeyCode.E) && nearWindow)
        {
            CleanWindow();
            cleanWindowText.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, vertical * climbSpeed);
        }
        else
        {
            rb.gravityScale = 1f;
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        }
    }

    void CleanWindow()
    {
        waterMan.clearWater();
        playerAudio.PlayCleanWindow();
        respawnPoint = transform.position;
        currWindow.LoadWindowCleaning();
        numCleaned++;

        // Calculate time to reach this window
        float windowTime = Time.time - windowStartTime;
        windowTimes.Add(windowTime);
        TelemetryManager.instance.AddMetricSample(windowTimeMetric, windowTime);

        Debug.Log($"Window cleaned in {windowTime:F2} seconds.");

        // Reset timer for next window
        windowStartTime = Time.time;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Wait to see if any other ground contact remains
            StartCoroutine(ResetGroundedAfterFrame());
        }
    }

    void CheckGroundCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Ground normal points up (avoid slopes or walls)
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    return;
                }
            }
        }
    }

    // Small delay to avoid flicker on exit
    System.Collections.IEnumerator ResetGroundedAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        isGrounded = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            Destroy(collision.transform.parent.gameObject);
            waterMan.addWater();
            playerAudio.PlayPickupWater();
        }

        if (collision.gameObject.CompareTag("Window") && waterMan.canClean() && !collision.gameObject.GetComponent<Window>().isClean())
        {
            cleanWindowText.SetActive(true);
            currWindow = collision.gameObject.GetComponent<Window>();
            nearWindow = true;
        }

        if (collision.gameObject.CompareTag("Ladder"))
        {
            isClimbing = true;
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

        if (collision.gameObject.CompareTag("Ladder"))
        {
            isClimbing = false;
        }
    }

    // Optional helper to view results
    public void PrintAllWindowTimes()
    {
        for (int i = 0; i < windowTimes.Count; i++)
        {
            Debug.Log($"Window {i + 1}: {windowTimes[i]:F2} seconds");
        }
    }
}
