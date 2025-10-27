using UnityEngine;

public class Button : MonoBehaviour
{
    private bool canPress = false;
    public GameObject platform1;
    public GameObject platform2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canPress && Input.GetKeyDown(KeyCode.E))
        {
            platform1.SetActive(!platform1.activeSelf);
            platform2.SetActive(!platform2.activeSelf); 
        }   
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canPress = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canPress = false;
        }
    }

}
