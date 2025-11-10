using UnityEngine;

public class WindowBubbleSpawn : MonoBehaviour
{
    public GameObject bubblePrefab;
    public Transform areaObject;
    public int bubblesX = 10;
    public int bubblesY = 6;
    [Range(0f, 1f)] public float jitterAmount = 0.3f;
    public float bubbleZ = -3f;

    public void SpawnBubbles()
    {
        if (bubblePrefab == null || areaObject == null)
        {
            Debug.LogError("Missing bubblePrefab or areaObject reference!");
            return;
        }

        SpriteRenderer sr = areaObject.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("No SpriteRenderer on windowBG!");
            return;
        }

        Vector3 min = sr.bounds.min;
        Vector3 max = sr.bounds.max;
        float width = Mathf.Max(0.01f, max.x - min.x);
        float height = Mathf.Max(0.01f, max.y - min.y);

        int safeBubblesX = Mathf.Max(1, bubblesX);
        int safeBubblesY = Mathf.Max(1, bubblesY);

        float stepX = width / (safeBubblesX - 1 > 0 ? safeBubblesX - 1 : 1);
        float stepY = height / (safeBubblesY - 1 > 0 ? safeBubblesY - 1 : 1);

        for (int i = 0; i < safeBubblesX; i++)
        {
            for (int j = 0; j < safeBubblesY; j++)
            {
                Vector2 basePos = new Vector2(min.x + i * stepX, min.y + j * stepY);
                float offsetX = Random.Range(-stepX * jitterAmount, stepX * jitterAmount);
                float offsetY = Random.Range(-stepY * jitterAmount, stepY * jitterAmount);

                Vector3 spawnPos = new Vector3(basePos.x + offsetX, basePos.y + offsetY, bubbleZ);

                Instantiate(bubblePrefab, spawnPos, Quaternion.identity, areaObject);
            }
        }

        Debug.Log($"Spawned {safeBubblesX * safeBubblesY} bubbles safely.");
    }
}
