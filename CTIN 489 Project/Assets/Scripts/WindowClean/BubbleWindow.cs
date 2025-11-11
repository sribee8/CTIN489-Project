using UnityEngine;
using System.Collections;

public class BubbleWindow : MonoBehaviour
{
    public GameObject windowBG;
    public GameObject graffitiRM;

    public bool cleaned;
    public DialogueManager dialogueManager;
    public GameObject bottle;
    public int windowNum;
    private string sectionName;
    private string sectionFinish;

    public PlayerMovement player;            // reference to the PlayerMovement component
    public WindowBubbleSpawn bubbleSpawner;

    // Internal cached Rigidbody2D for quick access
    private Rigidbody2D playerRb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cleaned = false;
        if (windowBG != null) windowBG.SetActive(false);
        sectionName = "Window" + windowNum;
        sectionFinish = "Window" + windowNum + "Fin";

        if (player != null)
            playerRb = player.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // When minigame finished and dialogue section ends, re-enable things
        if (cleaned && dialogueManager != null && dialogueManager.CurrentSection == sectionFinish && !dialogueManager.IsDialogueActive)
        {
            bottle.SetActive(true);

            // restore physics simulation and movement
            if (playerRb != null)
            {
                // re-enable physics simulation
                playerRb.simulated = true;

                // optionally clear any constraints here if you set them elsewhere
                // playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            if (player != null)
                player.enabled = true;

            windowBG.SetActive(false);
        }
    }

    public bool isClean() { return cleaned; }

    void CleanWindow()
    {
        graffitiRM.SetActive(false);
        cleaned = true;
    }

    public void LoadBubbleWindowCleaning()
    {
        StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        // sanity checks
        if (player == null)
            Debug.LogWarning("BubbleWindow: player reference is null!");
        if (windowBG == null)
            Debug.LogWarning("BubbleWindow: windowBG reference is null!");
        if (bubbleSpawner == null)
            Debug.LogWarning("BubbleWindow: bubbleSpawner reference is null!");

        // 1) Immediately stop player's motion (if we have the Rigidbody)
        if (playerRb == null && player != null)
            playerRb = player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            // zero velocities immediately
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;

            // disable physics simulation — this prevents any other physics or forces from moving the player
            playerRb.simulated = false;
        }

        // 2) Disable the PlayerMovement component so Update/FixedUpdate won't apply new inputs
        if (player != null)
            player.enabled = false;

        // 3) Wait one fixed update (physics step) so the engine has fully settled
        yield return new WaitForFixedUpdate();

        // 4) Activate window UI and position it at the camera center (now stable)
        windowBG.SetActive(true);
        bottle.SetActive(false);

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 camPos = cam.transform.position;
            windowBG.transform.position = new Vector3(camPos.x, camPos.y, windowBG.transform.position.z);
        }
        else
        {
            Debug.LogWarning("BubbleWindow: Main Camera not found. Window will not be centered.");
        }

        // 5) Start dialogue and spawn bubbles
        if (dialogueManager != null)
            dialogueManager.StartSection(sectionName);

        if (bubbleSpawner != null)
        {
            bubbleSpawner.areaObject = windowBG.transform;
            bubbleSpawner.SpawnBubbles();
        }

        // 6) Start checking for popped bubbles
        StartCoroutine(CheckForAllBubblesPopped());
    }

    void CompleteMinigame()
    {
        dialogueManager.StartSection(sectionFinish);
        CleanWindow();
    }

    private IEnumerator CheckForAllBubblesPopped()
    {
        // Wait until all objects with tag "Bubble" are gone
        while (GameObject.FindGameObjectsWithTag("Bubble").Length > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("All bubbles popped!");
        CompleteMinigame();
    }
}
