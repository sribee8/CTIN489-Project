using UnityEngine;

public class BubbleColor : MonoBehaviour
{
    public Color startColor = Color.cyan;
    public Color endColor = Color.magenta;
    public float lifeTime = 5f;

    private SpriteRenderer sr;
    private float age = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = startColor;
    }

    // Update is called once per frame
    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / lifeTime);

        sr.color = Color.Lerp(startColor, endColor, t);
    }
}
