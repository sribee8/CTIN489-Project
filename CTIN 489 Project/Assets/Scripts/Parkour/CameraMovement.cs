using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;        // Player transform
    public float smoothSpeed = 0.125f;
    public Vector3 offset;          // Camera offset from player

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
