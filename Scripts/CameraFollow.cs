using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    private Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            // Try to find player automatically if target is lost/not set
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) target = player.transform;
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }
}
