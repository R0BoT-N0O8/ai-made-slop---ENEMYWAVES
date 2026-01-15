using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    public enum ProjectileType
    {
        Linear,
        Accelerating,
        Tracking,
        Uncontrolled
    }

    [Header("Behavior Settings")]
    [SerializeField] private ProjectileType type = ProjectileType.Linear;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;

    [Header("Parameters")]
    [Tooltip("How fast it accelerates (added to speed per second)")]
    [SerializeField] private float accelerationRate = 5f;
    
    [Tooltip("How strongly it steers towards the player")]
    [SerializeField] private float turnSpeed = 200f;
    
    [Tooltip("Randomness strength for Uncontrolled movement")]
    [SerializeField] private float chaosStrength = 5f;

    private Rigidbody2D rb;
    private Transform player;
    private float currentSpeed;
    private float timeAlive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Ensure projectile is a trigger to pass through enemies (unless they are the potential target?)
        // The user requested projectiles pass through enemies. 
        // Note: Use a CompositeCollider2D or multiple colliders carefully if manually setting, 
        // but typically a simple Grab works.
        foreach(var col in GetComponentsInChildren<Collider2D>())
        {
            col.isTrigger = true;
        }

        // Set default speed immediately
        currentSpeed = speed;

        // Find player - assuming one player with PlayerController
        var playerCtrl = FindFirstObjectByType<PlayerController>();
        if (playerCtrl != null)
        {
            player = playerCtrl.transform;
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
        
        // Initial velocity for Linear/Accelerating/Uncontrolled
        // Tracking handles its own rotation in FixedUpdate
        if (type != ProjectileType.Tracking)
        {
            // Use transform.up as direction, which is set by the spawner
            // Only set if velocity is zero (Initialize might have set it already)
            if (rb.linearVelocity == Vector2.zero)
            {
                rb.linearVelocity = transform.up * currentSpeed;
            }
        }
    }

    public void Initialize(GameObject shooter, Vector2 direction = default)
    {
        // Ignore collision with shooter and its children
        Collider2D[] projectileColliders = GetComponentsInChildren<Collider2D>();
        var shooterColliders = shooter.GetComponentsInChildren<Collider2D>();

        foreach (var pCol in projectileColliders)
        {
            foreach (var sCol in shooterColliders)
            {
                Physics2D.IgnoreCollision(pCol, sCol);
            }
        }

        if (direction != Vector2.zero && type != ProjectileType.Tracking)
        {
            if (type == ProjectileType.Uncontrolled)
            {
                // Uncontrolled projectiles start in a completely random direction
                transform.up = Random.insideUnitCircle.normalized;
            }
            else
            {
                transform.up = direction;
            }
            rb.linearVelocity = transform.up * currentSpeed;
        }
    }

    private void FixedUpdate()
    {
        timeAlive += Time.fixedDeltaTime;

        switch (type)
        {
            case ProjectileType.Linear:
                // Velocity set in Start, remains constant
                break;

            case ProjectileType.Accelerating:
                currentSpeed += accelerationRate * Time.fixedDeltaTime;
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
                break;

            case ProjectileType.Tracking:
                if (player != null)
                {
                    Vector2 direction = (Vector2)player.position - rb.position;
                    direction.Normalize();
                    float rotateAmount = Vector3.Cross(direction, transform.up).z;
                    rb.angularVelocity = -rotateAmount * turnSpeed;
                    rb.linearVelocity = transform.up * speed;
                }
                else
                {
                    rb.linearVelocity = transform.up * speed;
                }
                break;

            case ProjectileType.Uncontrolled:
                // Add stronger random noise to direction
                float randomAngle = Random.Range(-chaosStrength * 5f, chaosStrength * 5f);
                rb.rotation += randomAngle;
                
                // Also vary speed slightly for extra chaos? 
                // Let's keep speed constant but direction erratic.
                rb.linearVelocity = transform.up * speed;
                break;
        }
    }

    // Support both Trigger and Collider
    private void OnTriggerEnter2D(Collider2D other) => HandleHit(other.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => HandleHit(collision.gameObject);

    private void HandleHit(GameObject hitObject)
    {
        // Ignore other projectiles
        if (hitObject.GetComponent<EnemyProjectile>() != null) return;

        if (hitObject.TryGetComponent<Health>(out var health))
        {
            // Prevent damaging other enemies (assuming they don't have PlayerController)
            // Or if we want strict Player only:
            // if (hitObject.CompareTag("Player") || hitObject.GetComponent<PlayerController>() != null)
            
            // For now, let's keep it robust: check if it has Health, AND check if it's the Player
            // If we want friendly fire, we remove the Player check.
            
            bool isPlayer = hitObject.GetComponent<PlayerController>() != null;
            if (isPlayer)
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else 
        {
            // Hit wall or obstacle
            Destroy(gameObject);
        }
    }
}
