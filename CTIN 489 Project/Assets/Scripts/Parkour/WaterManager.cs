using UnityEngine;
using UnityEngine.UI;

public class WaterManager : MonoBehaviour
{
    private int numWater = 0;
    public Slider waterSlider;
    private int maxWater = 2;

    void Start()
    {
        waterSlider.maxValue = maxWater;
        waterSlider.value = numWater;
    }

    public void addWater()
    {
        numWater++;
        waterSlider.value = numWater;
    }

    public bool canClean()
    {
        return numWater == maxWater;
    }

    public void clearWater()
    {
        numWater = 0;
        waterSlider.value = numWater;
    }
}
