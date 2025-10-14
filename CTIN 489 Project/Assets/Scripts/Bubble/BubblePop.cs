using UnityEngine;
using System.Collections;

public class BubblePop : MonoBehaviour
{
    private Vector3 originalScale;
    public float popScale = 1.3f;
    public float popSpeed = 0.2f;
    public float minStartScale = 0.5f;
    public float maxStartScale = 1.5f;
    public float minFloatSpeed = 0.5f;
    public float maxFloatSpeed = 2.5f;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Randomize starting scale
        float randomSize = Random.Range(minStartScale, maxStartScale);
        transform.localScale = Vector3.one * randomSize;
        originalScale = transform.localScale;

        // Adding rigidbody reference
        rb = GetComponent<Rigidbody2D>();

        // Assign a random drifting velocity
        Vector2 randomDir = Random.insideUnitCircle.normalized;

        float randomSpeed = Random.Range(minFloatSpeed, maxFloatSpeed);

        rb.linearVelocity = randomDir * randomSpeed;
    }

    void OnMouseDown()
    {
        StartCoroutine(Pop());
    }
    // Update is called once per frame
    IEnumerator Pop()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / popSpeed;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * popScale, t);
            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / popSpeed;
            transform.localScale = Vector3.Lerp(originalScale * popScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }

}
