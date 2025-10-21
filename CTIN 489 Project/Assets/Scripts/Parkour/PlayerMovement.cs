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

    // Timing and telemetry
    private float windowStartTime;
    private List<float> windowTimes = new();
    private MetricId windowTimeMetric;
    private MetricId respawnMetric;

    private static PlayerMovement instance;

    void Awake()
    {
        // Persist across scenes (only one copy)
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset the timer for new level
        windowStartTime = Time.time;
        respawnPoint = transform.position;
        windowTimes.Clear();
        Debug.Log($"Loaded {scene.name}, timer reset.");
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

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

        // Climbing
        if (isClimbing)
        {
            rb.gravityScale = 0f;

            if (vertical != 0)
            {
                rb.linearVelocity = new Vector2(horizontal * moveSpeed, vertical * climbSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector2(horizontal * moveSpeed, 0f);
            }
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
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
            }
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

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
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
