using UnityEngine;
using UnityEngine.UI;

public class WaterManager : MonoBehaviour
{
    private int numWater = 0;
    public Slider waterSlider;
    private int maxWater = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waterSlider.maxValue = maxWater;
        waterSlider.value = numWater;
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }
}
