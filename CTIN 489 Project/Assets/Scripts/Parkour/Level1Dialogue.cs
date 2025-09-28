using UnityEngine;

public class Level1Dialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;
    private bool playedIntro;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playedIntro = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        // picking up water
        if (collision.gameObject.CompareTag("Water") && !playedIntro)
        {
            dialogueManager.StartSection("Intro");
            playedIntro = true;
        }

    }

}
