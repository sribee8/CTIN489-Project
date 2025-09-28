using UnityEngine;
using UnityEngine.SceneManagement;

public class Window : MonoBehaviour
{
    bool cleaned;
    public Color cleanWindow;
    public GameObject windowBG;
    public GameObject windowGraffiti;
    public GameObject squeegee;
    public DialogueManager dialogueManager;
    public int windowNum;
    private string sectionName;
    private string sectionFinish;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cleaned = false;
        windowBG.SetActive(false);
        windowGraffiti.SetActive(false);
        squeegee.SetActive(false);
        sectionName = "Window" + windowNum;
        sectionFinish = "Window" + windowNum + "Fin";
        Debug.Log(sectionFinish);
        Debug.Log(sectionName);
    }

    // Update is called once per frame
    void Update()
    {
        if (cleaned && dialogueManager.CurrentSection == sectionFinish && !dialogueManager.IsDialogueActive)
        {
            windowBG.SetActive(false);
        }
    }

    public void CleanWindow()
    {
        // Load minigame and then change window color
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.color = cleanWindow;
        }
        cleaned = true;


    }

    public bool isClean() { return cleaned; }

    public void LoadWindowCleaning()
    {
        windowBG.SetActive(true);
        windowGraffiti.SetActive(true);
        squeegee.SetActive(true);
        dialogueManager.StartSection(sectionName);
    }

    public void CompleteMinigame()
    {
        windowGraffiti.SetActive(false);
        squeegee.SetActive(false);
        dialogueManager.StartSection(sectionFinish);
        CleanWindow();
    }
}
