using UnityEngine;

public class Tree : MonoBehaviour
{
    private bool shaken = false;
    public GameObject water;
    private bool canShake = false;
    private bool playedDialogue = false;
    public DialogueManager dialogueManager;
    private bool isFalling = false;
    public float fallSpeed = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!shaken && canShake)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                shaken = true;
                isFalling = true;
            }
        }
        if (isFalling && water)
        {
            // Move water downward over time
            water.transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            // Stop falling once it reaches a certain point (y = -3 for example)
            if (water.transform.position.y <= -2.4f)
            {
                isFalling = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Entered trigger");
            canShake = true;
            if (!playedDialogue)
            {
                dialogueManager.StartSection("Tree");
                playedDialogue = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canShake = false;
        }
    }
}
