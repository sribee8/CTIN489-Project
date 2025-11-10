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

    public PlayerMovement player;
    public WindowBubbleSpawn bubbleSpawner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cleaned = false;
        windowBG.SetActive(false);
        sectionName = "Window" + windowNum;
        sectionFinish = "Window" + windowNum + "Fin";
    }

    // Update is called once per frame
    void Update()
    {
        if (cleaned && dialogueManager.CurrentSection == sectionFinish && !dialogueManager.IsDialogueActive)
        {
            bottle.SetActive(true);
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
        yield return null; // wait one frame so player has fully stopped
        windowBG.SetActive(true);
        bottle.SetActive(false);

        Camera cam = Camera.main;
        Vector3 camPos = cam.transform.position;
        windowBG.transform.position = new Vector3(camPos.x, camPos.y, windowBG.transform.position.z);

        dialogueManager.StartSection(sectionName);
        bubbleSpawner.areaObject = windowBG.transform;
        bubbleSpawner.SpawnBubbles();
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
