using UnityEngine;

public class SqueegeeFollow : MonoBehaviour
{
    [Header("Settings")]
    public Vector3 offset = Vector3.zero;   // optional offset if you want to move it slightly off the cursor
    public bool smoothFollow = false;       // set true for smooth movement
    public float followSpeed = 10f;         // how quickly it catches up if smooth is enabled

    void Update()
    {
        Vector3 targetPos = Input.mousePosition + offset;

        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPos;
        }
    }
}
