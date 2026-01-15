using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    private Vector3 currentVelocity;

    private float initialZ;

    private void Awake()
    {
        initialZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            // Try to find player automatically if target is lost/not set
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) target = player.transform;
            return;
        }
        
        // Calculate desired X and Y based on target + offset
        float targetX = target.position.x + offset.x;
        float targetY = target.position.y + offset.y;

        // We want to arrive at (targetX, targetY, initialZ)
        Vector3 desiredPosition = new Vector3(targetX, targetY, initialZ);

        // SmoothDamp towards that position
        Vector3 nextPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        
        // Force Z to remain locked exactly at initialZ (fix floating point drift or damp issues)
        nextPosition.z = initialZ;

        transform.position = nextPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
