using UnityEngine;
using UnityEngine.SceneManagement;


public class VerticalSliceSwitch : MonoBehaviour
{
    public PlayerMovement player;
    private bool canLoadNext = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.numCleaned == 2)
        {
            canLoadNext = true;
        }

        if (canLoadNext && transform.position.x > 55)
        {
            SceneManager.LoadScene("Level2");
        }
    }
}
