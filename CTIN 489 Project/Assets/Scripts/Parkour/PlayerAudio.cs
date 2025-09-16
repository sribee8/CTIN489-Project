using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public AudioSource audioSource;       // Assign the AudioSource
    public AudioClip pickupWaterClip;     // Assign water pickup sound
    public AudioClip cleanWindowClip;     // Assign window clean sound

    public void PlayPickupWater()
    {
        audioSource.PlayOneShot(pickupWaterClip);
    }

    public void PlayCleanWindow()
    {
        audioSource.PlayOneShot(cleanWindowClip);
    }
}
