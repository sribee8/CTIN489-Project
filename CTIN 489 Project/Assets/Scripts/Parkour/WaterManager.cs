using UnityEngine;
using UnityEngine.UI;

public class WaterManager : MonoBehaviour
{
    private int numWater = 0;
    private int maxWater = 2;

    public Image bottle;          // assign this in the inspector
    public Sprite [] watSprites;

    void Start()
    {
    }

    public void addWater()
    {
        numWater++;
        bottle.sprite = watSprites[Mathf.Min(numWater, 2)];
    }

    public bool canClean()
    {
        return numWater == maxWater;
    }

    public void clearWater()
    {
        numWater -= 2;
        bottle.sprite = watSprites[numWater];
    }
}
