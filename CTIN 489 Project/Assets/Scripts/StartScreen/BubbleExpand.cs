using UnityEngine;
using System.Collections;

public class BubbleExpand : MonoBehaviour
{
    private Vector3 originalScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // store the bubble's starting scale
        originalScale = transform.localScale;

        // set the bubble to invisible (scale 0)
        transform.localScale = Vector3.zero;

        // pick a random delay before expansion
        float delay = Random.Range(0f, 1f);

        // start the coroutine that handles the expansion
        StartCoroutine(ExpandAfterDelay(delay));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ExpandAfterDelay(float delay)
    {
        // wait for the random delay
        yield return new WaitForSeconds(delay);

        // how fast the bubble grows
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // interpolate from 0 to full size
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // make sure it ends at full size
        transform.localScale = originalScale;
    }
}
