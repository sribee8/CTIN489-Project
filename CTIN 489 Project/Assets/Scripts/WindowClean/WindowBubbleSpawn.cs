using UnityEngine;

public class WindowBubbleSpawn : MonoBehaviour
{
    [Header("Prefab & Area")]
    public GameObject bubblePrefab;
    public Transform areaObject;

    [Header("Grid")]
    public int bubblesX = 10;
    public int bubblesY = 6;
    [Range(0f, 1f)] public float jitterAmount = 0.3f;
    public float bubbleZ = -3f;

    [Header("Edge Padding (choose one)")]
    // If true, padding will be computed based on the prefab's max scale.
    public bool useDynamicPrefabPadding = true;

    // When using fixed padding, tweak this value (world units).
    public float fixedPadding = 0.2f;

    // safety clamp for grid
    public int maxAllowedPerAxis = 50;

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
            Debug.LogError("No SpriteRenderer on areaObject (windowBG)!");
            return;
        }

        Bounds bounds = sr.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        float width = Mathf.Max(0.0001f, max.x - min.x);
        float height = Mathf.Max(0.0001f, max.y - min.y);

        int safeBubblesX = Mathf.Clamp(bubblesX, 1, maxAllowedPerAxis);
        int safeBubblesY = Mathf.Clamp(bubblesY, 1, maxAllowedPerAxis);

        // Compute padding
        float padding = fixedPadding;

        if (useDynamicPrefabPadding)
        {
            // Try to compute a safe padding based on the bubble prefab's sprite and BubbleBehavior.maxStartScale
            BubbleBehavior prefabBehaviour = bubblePrefab.GetComponent<BubbleBehavior>();
            SpriteRenderer bubbleSr = bubblePrefab.GetComponent<SpriteRenderer>();

            if (prefabBehaviour != null && bubbleSr != null)
            {
                // sprite bounds are in world units only when in scene; for prefab this gives local sprite size in units.
                // Use bubbleSr.bounds.size which usually represents the sprite size * transform scale.
                Vector2 spriteSize = bubbleSr.bounds.size;
                // take the larger dimension, assume bubble is roughly circular
                float spriteDiameter = Mathf.Max(spriteSize.x, spriteSize.y);

                // account for the prefab's localScale and potential maxStartScale randomness inside BubbleBehavior
                float prefabScaleFactor = bubblePrefab.transform.localScale.x; // assume uniform scale
                float estimatedMaxDiameter = spriteDiameter * prefabScaleFactor * prefabBehaviour.maxStartScale;

                // radius = diameter / 2
                float estimatedMaxRadius = estimatedMaxDiameter * 0.5f;

                // add a small extra margin
                padding = estimatedMaxRadius + 0.05f;
            }
            else
            {
                // fallback if prefab doesn't have those components
                padding = fixedPadding;
            }
        }

        // Ensure padding does not exceed half the window size (would make spawn area inverted)
        float maxPaddingX = width * 0.5f - 0.001f;
        float maxPaddingY = height * 0.5f - 0.001f;
        float usedPaddingX = Mathf.Min(padding, maxPaddingX);
        float usedPaddingY = Mathf.Min(padding, maxPaddingY);

        if (usedPaddingX <= 0f || usedPaddingY <= 0f)
        {
            Debug.LogWarning("Padding too large for window size — reducing padding to small value.");
            usedPaddingX = Mathf.Min(0.01f, maxPaddingX);
            usedPaddingY = Mathf.Min(0.01f, maxPaddingY);
        }

        // Shrink the spawn rect by the padding on all sides
        Vector3 paddedMin = new Vector3(min.x + usedPaddingX, min.y + usedPaddingY, min.z);
        Vector3 paddedMax = new Vector3(max.x - usedPaddingX, max.y - usedPaddingY, max.z);

        float spawnWidth = paddedMax.x - paddedMin.x;
        float spawnHeight = paddedMax.y - paddedMin.y;

        // Guard in case padded rect is effectively zero
        if (spawnWidth <= 0f || spawnHeight <= 0f)
        {
            Debug.LogError("Padded spawn area is invalid (padding too large for window). Aborting spawn.");
            return;
        }

        float stepX = spawnWidth / (safeBubblesX - 1 > 0 ? safeBubblesX - 1 : 1);
        float stepY = spawnHeight / (safeBubblesY - 1 > 0 ? safeBubblesY - 1 : 1);

        int spawnCount = 0;
        for (int i = 0; i < safeBubblesX; i++)
        {
            for (int j = 0; j < safeBubblesY; j++)
            {
                Vector2 basePos = new Vector2(paddedMin.x + i * stepX, paddedMin.y + j * stepY);
                float offsetX = Random.Range(-stepX * jitterAmount, stepX * jitterAmount);
                float offsetY = Random.Range(-stepY * jitterAmount, stepY * jitterAmount);

                // ensure jitter doesn't push beyond padded area
                float finalX = Mathf.Clamp(basePos.x + offsetX, paddedMin.x, paddedMax.x);
                float finalY = Mathf.Clamp(basePos.y + offsetY, paddedMin.y, paddedMax.y);

                Vector3 spawnPos = new Vector3(finalX, finalY, bubbleZ);

                // Instantiate without parenting so scale isn't inherited (change if you want parented)
                Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
                spawnCount++;
            }
        }

        Debug.Log($"Spawned {spawnCount} bubbles within padded bounds (padding={padding:F3}).");
    }
}
