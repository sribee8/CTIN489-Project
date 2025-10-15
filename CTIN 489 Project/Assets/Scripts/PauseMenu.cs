using UnityEngine;
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
    }

    public void OnRestart()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
