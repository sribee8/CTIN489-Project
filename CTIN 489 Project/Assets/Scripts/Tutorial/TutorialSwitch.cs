using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSwitch : MonoBehaviour
{
    public Window window;
    public DialogueManager dialogue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogue.StartSection("Intro");
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x > 12f && window.cleaned)
        {
            SceneManager.LoadScene("VerticalSlice");
        }
    }
}
