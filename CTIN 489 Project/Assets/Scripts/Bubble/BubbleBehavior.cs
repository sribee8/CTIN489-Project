using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BubbleBehavior : MonoBehaviour
{
    [Header("Color Settings")]
    public Color startColor = Color.cyan;
    public Color endColor = Color.magenta;
    public float lifeTime = 5f;

    [Header("Pop Settings")]
    public float basePopScale = 1.2f;   // min pop size
    public float maxPopScale = 2.0f;    // max pop size
    public float basePopSpeed = 0.3f;   // slowest pop
    public float maxPopSpeed = 0.1f;    // fastest pop

    private SpriteRenderer sr;
    private float age = 0f;
    private Vector3 originalScale;
    private bool isPopping = false;

    public float minStartScale = 0.5f;
    public float maxStartScale = 1.5f;
    public float minFloatSpeed = 0.5f;
    public float maxFloatSpeed = 2.5f;
    private Rigidbody2D rb;

    public AudioClip[] popSounds;
    public AudioSource audioSource;

    public GameObject popParticlesPrefab;

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
        sr = GetComponent<SpriteRenderer>();
        sr.color = startColor;
    }

    void Update()
    {
        // Increase age
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / lifeTime);

        // Update color gradually
        sr.color = Color.Lerp(startColor, endColor, t);
    }

    void OnMouseDown()
    {
        if (!isPopping)
            StartCoroutine(PopAndDestroy());
    }

    IEnumerator PopAndDestroy()
    {
        isPopping = true;

        // Age ratio (0 = fresh, 1 = old)
        float t = Mathf.Clamp01(age / lifeTime);

        AudioClip clipToPlay = null;
        if (popSounds.Length >= 2)
        {
            clipToPlay = t < 0.5f ? popSounds[0] : popSounds[1]; // young vs old
        }
        else if (popSounds.Length == 1)
        {
            clipToPlay = popSounds[0];
        }

        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay);

        if (popParticlesPrefab != null)
        {
            GameObject particles = Instantiate(popParticlesPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = particles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Optional: match particle color to bubble
                var main = ps.main;
                main.startColor = sr.color;
            }
            Destroy(particles, 1f); // destroy after 1s to clean up
        }

        // Pop settings scale with age
        float popScale = Mathf.Lerp(basePopScale, maxPopScale, t);
        float popSpeed = Mathf.Lerp(basePopSpeed, maxPopSpeed, t);

        // grow
        float lerp = 0;
        while (lerp < 1)
        {
            lerp += Time.deltaTime / popSpeed;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * popScale, lerp);
            yield return null;
        }

        // shrink to zero
        lerp = 0;
        while (lerp < 1)
        {
            lerp += Time.deltaTime / popSpeed;
            transform.localScale = Vector3.Lerp(originalScale * popScale, Vector3.zero, lerp);
            yield return null;
        }

        if (popParticlesPrefab != null)
        {
            GameObject particles = Instantiate(popParticlesPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = particles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Optional: match particle color to bubble
                var main = ps.main;
                main.startColor = sr.color;
            }
            Destroy(particles, 0.5f); // destroy after 1s to clean up
        }

        Destroy(gameObject);
    }
}
