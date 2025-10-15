using UnityEngine;
using System.Collections;

public class BubbleSpawner : MonoBehaviour
{
    public GameObject bubblePrefab;
    public Vector2 spawnAreaMin = new Vector2(-5f, -4f);
    public Vector2 spawnAreaMax = new Vector2(5f, 4f);
    public float spawnZ = -3.0f;
    public float minSpawnInterval = 0.25f;
    public float maxSpawnInterval = 1.0f;
    public Camera mainCamera;

    private Coroutine spawnRoutine;

    public void StartSpawning()
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnBubbles());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    IEnumerator SpawnBubbles()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            Vector3 camPos = mainCamera.transform.position;
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x) + camPos.x;
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y) + camPos.y;
            Vector3 spawnPos = new Vector3(x, y, spawnZ);

            Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
        }
    }
}
