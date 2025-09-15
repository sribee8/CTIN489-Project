using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SqueegeeAudio : MonoBehaviour
{
    [Header("Refs")]
    public BoxCollider2D paneCollider;

    [Header("Speed to sound")]
    public float minSpeed = 0.3f;
    public float maxSpeed = 3f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.2f;
    public float minVolume = 0.05f;
    public float maxVolume = 0.35f;

    [Header("Smoothing")]
    public float fadeTime = 0.08f;
    public float pitchLerp = 12f;

    AudioSource src;
    Vector3 lastPos;
    bool hadLast;

    void Reset()
    {
        src = GetComponent<AudioSource>();
        if (!src) src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f;
        src.volume = 0f;
    }

    void Awake()
    {
        src = GetComponent<AudioSource>();
        if (!src) src = gameObject.AddComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        src.volume = 0f;
    }

    void OnEnable()
    {
        hadLast = false;
        if (src.clip && !src.isPlaying) src.Play();
    }

    void OnDisable()
    {
        if (src) src.Stop();
        hadLast = false;
    }

    void Update()
    {
        if (!src) return;

        bool dragging = Input.GetMouseButton(0);
        if (paneCollider)
            dragging &= paneCollider.OverlapPoint(transform.position);

        float speed = 0f;
        if (hadLast)
            speed = (transform.position - lastPos).magnitude / Mathf.Max(Time.deltaTime, 1e-6f);
        lastPos = transform.position;
        hadLast = true;

        float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

        float targetPitch  = Mathf.Lerp(minPitch,  maxPitch,  t);
        float targetVolume = dragging ? Mathf.Lerp(minVolume, maxVolume, t) : 0f;

        src.pitch  = Mathf.Lerp(src.pitch, targetPitch,  pitchLerp * Time.deltaTime);
        float fadeRate = (fadeTime > 0f) ? (1f / fadeTime) : 999f;
        src.volume = Mathf.MoveTowards(src.volume, targetVolume, fadeRate * Time.deltaTime);

        if (src.clip != null && !src.isPlaying) src.Play();
    }
}
