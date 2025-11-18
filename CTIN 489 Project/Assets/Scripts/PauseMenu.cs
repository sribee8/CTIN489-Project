using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseMenuObj;
    public BubbleSpawner bubbleSpawner;
    public PlayerMovement player;

    void Start()
    {
        PauseMenuObj.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool newState = !PauseMenuObj.activeSelf;
            PauseMenuObj.SetActive(newState);
            player.enabled = false;

            if (newState)
                bubbleSpawner.StartSpawning();
            else
                bubbleSpawner.StopSpawning();
        }
    }

    public void OnResume()
    {
        PauseMenuObj.SetActive(false);
        bubbleSpawner.StopSpawning();
        player.enabled = true;

        GameObject[] bubbles = GameObject.FindGameObjectsWithTag("Bubble");
        foreach (GameObject bubble in bubbles)
            Destroy(bubble);
        Debug.Log("Resume Clicked");
    }

    public void OnRestart()
    {
        SceneManager.LoadScene("StartScreen");
    }

    public void OnQuit()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        // If running in a built application, quit the application
        Application.Quit();
        #endif
    }
}
