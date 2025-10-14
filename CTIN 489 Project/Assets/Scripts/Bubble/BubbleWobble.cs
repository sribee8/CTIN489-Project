using UnityEngine;
using System.Collections;

public class BubbleWobble : MonoBehaviour
{
    public float wobbleScale = 1.2f;
    public float wobbleDuration = 0.2f;

    private bool isWobbling = false;

    public AudioSource wobble;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isWobbling) StartCoroutine(Wobble(collision));
    }
    
    IEnumerator Wobble(Collision2D collision)
    {
        isWobbling = true;

        wobble.Play();

        Vector3 baseScale = transform.localScale;

        // Pick a wobble direction based on collision normal
        Vector2 normal = collision.contacts[0].normal;
        Vector3 stretchDir = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), 0);

        // Scale up in the hit direction (stretch/squash)
        Vector3 stretchedScale = baseScale + stretchDir * (wobbleScale - 1f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / wobbleDuration;
            transform.localScale = Vector3.Lerp(baseScale, stretchedScale, t);
            yield return null;
        }

        // Return to normal
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / wobbleDuration;
            transform.localScale = Vector3.Lerp(stretchedScale, baseScale, t);
            yield return null;
        }

        transform.localScale = baseScale;
        isWobbling = false;
    }
}
