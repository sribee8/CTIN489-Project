using UnityEngine;

public class Level2Dialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;
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
            dialogueManager.StartSection("Intro");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Platform")
        {
            dialogueManager.StartSection("Platform");
        }
    }
}
