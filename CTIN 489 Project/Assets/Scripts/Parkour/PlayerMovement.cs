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
    private BubbleWindow bubbleWindow;
    private bool nearWindow;
    public int numCleaned = 0;

    // Timing and telemetry
    private float windowStartTime;
    private List<float> windowTimes = new();
    private MetricId windowTimeMetric;
    private MetricId respawnMetric;

    private static PlayerMovement instance;
    public bool isLevel2 = true;

    // NEW: Moving platform support
    private Rigidbody2D currentPlatform;

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

        windowStartTime = Time.time;

        windowTimeMetric = TelemetryManager.instance.CreateSampledMetric<float>("WindowCompletionTime");
        respawnMetric = TelemetryManager.instance.CreateAccumulatedMetric("RespawnCount");
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Flip sprite
        if (horizontal > 0.01f)
            spriteRenderer.flipX = false;
        else if (horizontal < -0.01f)
            spriteRenderer.flipX = true;

        // Jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Respawn
        if (Input.GetKeyDown(KeyCode.R) || transform.position.y <= -5)
        {
            transform.position = respawnPoint;
            TelemetryManager.instance.AccumulateMetric(respawnMetric, 1);
        }

        // Cleaning window
        if (Input.GetKeyDown(KeyCode.E) && nearWindow)
        {
            cleanWindowText.SetActive(false);
            if (!isLevel2) CleanWindow();
            else CleanBubbleWindow();
        }
    }

    void FixedUpdate()
    {
        Vector2 platformVelocity = currentPlatform != null ? currentPlatform.linearVelocity : Vector2.zero;

        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, vertical * climbSpeed) + platformVelocity;
        }
        else
        {
            rb.gravityScale = 1f;
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y) + platformVelocity;
        }
    }

    void CleanWindow()
    {
        waterMan.clearWater();
        playerAudio.PlayCleanWindow();
        respawnPoint = transform.position;
        currWindow.LoadWindowCleaning();
        numCleaned++;

        float windowTime = Time.time - windowStartTime;
        windowTimes.Add(windowTime);
        TelemetryManager.instance.AddMetricSample(windowTimeMetric, windowTime);

        Debug.Log($"Window cleaned in {windowTime:F2} seconds.");
        windowStartTime = Time.time;
    }

    void CleanBubbleWindow()
    {
        waterMan.clearWater();
        playerAudio.PlayCleanWindow();
        respawnPoint = transform.position;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        bubbleWindow.LoadBubbleWindowCleaning();
        numCleaned++;

        float windowTime = Time.time - windowStartTime;
        windowTimes.Add(windowTime);
        TelemetryManager.instance.AddMetricSample(windowTimeMetric, windowTime);

        Debug.Log($"Window cleaned in {windowTime:F2} seconds.");
        windowStartTime = Time.time;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGroundCollision(collision);

        // Moving platform detection
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    currentPlatform = collision.rigidbody;
                    break;
                }
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            StartCoroutine(ResetGroundedAfterFrame());
        }

        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            currentPlatform = null;
        }
    }

    void CheckGroundCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MovingPlatform"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    return;
                }
            }
        }
    }

    System.Collections.IEnumerator ResetGroundedAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        isGrounded = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            Destroy(collision.transform.parent.gameObject);
            waterMan.addWater();
            playerAudio.PlayPickupWater();
        }

        if (collision.CompareTag("Window") && waterMan.canClean())
        {
            if (!isLevel2 && !collision.GetComponent<Window>().isClean())
            {
                cleanWindowText.SetActive(true);
                currWindow = collision.GetComponent<Window>();
                nearWindow = true;
            }
            else if (isLevel2 && !collision.GetComponent<BubbleWindow>().isClean())
            {
                cleanWindowText.SetActive(true);
                bubbleWindow = collision.GetComponent<BubbleWindow>();
                nearWindow = true;
            }
        }

        if (collision.CompareTag("Ladder"))
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

        if (collision.CompareTag("Ladder"))
        {
            isClimbing = false;
        }
    }

    public void PrintAllWindowTimes()
    {
        for (int i = 0; i < windowTimes.Count; i++)
        {
            Debug.Log($"Window {i + 1}: {windowTimes[i]:F2} seconds");
        }
    }
}
