using UnityEngine;
using UnityEditor;  

public class Quit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
