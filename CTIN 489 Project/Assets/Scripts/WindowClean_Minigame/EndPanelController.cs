using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndPanelController : MonoBehaviour
{
    [Header("Refs")]
    public DirtLayer2D dirt;
    public GameObject endPanel;
    public TextMeshProUGUI summaryText;
    public Button returnButton;
    public RectTransform hudRoot;

    [Header("Finish rule")]
    [Range(50f, 100f)] public float targetPercent = 98f;

    [Header("Rating display only")]
    public float wCoverage = 0.60f;
    public float wPrecision = 0.25f;
    public float wStrokes = 0.15f;
    public int idealStrokes = 6;

    bool shown;

    void Start()
    {
        if (endPanel) endPanel.SetActive(false);
        if (returnButton) returnButton.onClick.AddListener(OnReturn);
        shown = false;
    }

    void Update()
    {
        if (shown || dirt == null) return;

        float clean = dirt.CleanPercent();
        if (clean >= targetPercent)
        {
            shown = true;
            if (hudRoot) hudRoot.gameObject.SetActive(false);
            if (endPanel) endPanel.SetActive(true);

            float coverage = Mathf.Clamp01(clean / 100f);
            float precision = PrecisionScore();
            float strokeScore = Mathf.Clamp01((float)idealStrokes /
                                  Mathf.Max(idealStrokes, Mathf.Max(1, dirt.StrokeCount())));
            float accuracy = 100f * Mathf.Clamp01(wCoverage * coverage +
                                                  wPrecision * precision +
                                                  wStrokes * strokeScore);
            if (summaryText)
                summaryText.text = $"Level complete!\nStroke number: {dirt.StrokeCount()}\nAccuracy: {accuracy:0.0}%";
        }
    }

    float PrecisionScore()
    {
        if (!dirt || dirt.RemainingPixels() < 0) return 1f;
        int off = dirt.OffPathCleaned();
        int path = Mathf.Max(1, dirt.RemainingPixels() + off);
        return 1f - Mathf.Clamp01((float)off / path);
    }

    void OnReturn()
    {
        Debug.Log("on return called");
        SceneManager.UnloadSceneAsync("LaurenPrototype1");
    }
}
