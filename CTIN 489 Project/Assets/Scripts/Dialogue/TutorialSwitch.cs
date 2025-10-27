using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSwitch : MonoBehaviour
{
    public Window window;
    public DialogueManager dialogue;
    private bool playedIntro = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!playedIntro && Input.anyKeyDown)
        {
            playedIntro = true;
            dialogue.StartSection("Intro");
        }
        if (transform.position.x > 12f && window.cleaned)
        {
            SceneManager.LoadScene("VerticalSlice");
        }
    }
}
