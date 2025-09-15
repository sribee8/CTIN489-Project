using UnityEngine;
using TMPro;

public class StatsHUD : MonoBehaviour
{
    public DirtLayer2D dirt;
    public TextMeshProUGUI text;

    void Update()
    {
        if (!dirt || !text) return;
        text.text = $"Clean\n{dirt.CleanPercent():0.0}%\nRemaining\n{dirt.RemainingPixels()}\nStrokes {dirt.StrokeCount()}";
    }
}
