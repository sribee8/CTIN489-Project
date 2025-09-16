using UnityEngine;
using UnityEngine.SceneManagement;

public class Window : MonoBehaviour
{
    bool cleaned;
    public Color cleanWindow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cleaned = false;
    }   

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CleanWindow()
    {
        // Load minigame and then change window color
        LoadWindowCleaning();
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.color = cleanWindow;
        }
        cleaned = true;


    }

    public bool isClean() { return cleaned; }

    public void LoadWindowCleaning()
    {
        SceneManager.LoadScene("LaurenPrototype1", LoadSceneMode.Additive);
    }
}
