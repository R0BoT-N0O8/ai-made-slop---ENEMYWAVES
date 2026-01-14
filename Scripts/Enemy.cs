using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class Enemy : MonoBehaviour
{
    public enum AttackStyle
    {
        StaysDistant,
        StaysClose,
        Kamikaze
    }

    public enum AttackType
    {
        Melee,
        Projectile
    }

    [Header("Stats")]
    [Range(0, 100)] public float spawnRateWeight = 50f; // Could be used by spawner
    [Range(0, 10000)] [SerializeField] private float maxHealth = 100f;
    [Range(0, 100)] [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float smoothTime = 0.1f;
    private Vector2 currentVelocity;

    [Header("Visuals")]
    [SerializeField] private bool facePlayer = true;

    [Header("Combat Configuration")]
    [SerializeField] private AttackStyle attackStyle = AttackStyle.Kamikaze;
    [SerializeField] private AttackType attackType = AttackType.Melee;
    [SerializeField] private float damage = 10f;
    [Range(0, 10)] [SerializeField] private float attackRate = 1f;

    [Header("Distancing")]
    [SerializeField] private float closeRange = 3f;
    [SerializeField] private float distantRange = 8f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [Range(0, 100)] [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private bool shotgunFire = false;
    [SerializeField] private int shotgunPelletCount = 5;

    [Header("Audio")]
    [SerializeField] private AudioClip[] attackSounds;
    private AudioSource audioSource;

    private float currentHealth;
    private float nextAttackTime;
    private Transform player;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent physics spin
        
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;

        // Find player
        var playerCtrl = FindFirstObjectByType<PlayerController>();
        if (playerCtrl != null)
        {
            player = playerCtrl.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        Move();
        AttemptAttack();
    }

    private void Move()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        Vector2 movement = Vector2.zero;

        switch (attackStyle)
        {
            case AttackStyle.Kamikaze:
                movement = dirToPlayer;
                break;

            case AttackStyle.StaysClose:
                if (distToPlayer > closeRange)
                {
                    movement = dirToPlayer;
                }
                else if (distToPlayer < closeRange * 0.5f)
                {
                    movement = -dirToPlayer; 
                }
                break;

            case AttackStyle.StaysDistant:
                if (distToPlayer > distantRange)
                {
                    movement = dirToPlayer;
                }
                else if (distToPlayer < distantRange - 1f)
                {
                    movement = -dirToPlayer;
                }
                break;
        }

        // Apply smooth movement
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, movement * moveSpeed, ref currentVelocity, smoothTime);
        
        // Face player if enabled
        if (facePlayer)
        {
            float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
    }

    private void AttemptAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            bool canAttack = false;

            if (attackType == AttackType.Melee)
            {
                if (distToPlayer <= 1.5f) canAttack = true;
            }
            else // Projectile
            {
                if (distToPlayer <= 15f) canAttack = true;
            }

            if (canAttack)
            {
                PerformAttack();
                nextAttackTime = Time.time + (1f / attackRate);
            }
        }
    }

    private void PerformAttack()
    {
        // Play sound
        if (attackSounds != null && attackSounds.Length > 0 && audioSource != null)
        {
            var clip = attackSounds[Random.Range(0, attackSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        if (attackType == AttackType.Melee)
        {
            if (player.TryGetComponent<Health>(out var hp))
            {
                hp.TakeDamage(damage);
            }
        }
        else // Projectile
        {
            if (projectilePrefab == null) return;

            if (shotgunFire)
            {
                for (int i = 0; i < shotgunPelletCount; i++)
                {
                    SpawnProjectile(true);
                }
            }
            else
            {
                SpawnProjectile(false);
            }
        }
    }

    private void SpawnProjectile(bool randomSpread)
    {
        // Calculate rotation towards player regardless of current rotation
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0, 0, baseAngle);

        if (randomSpread)
        {
            float spreadAngle = Random.Range(-30f, 30f);
            rotation = Quaternion.Euler(0, 0, baseAngle + spreadAngle);
        }

        GameObject proj = Instantiate(projectilePrefab, transform.position, rotation);
        
        // Initialize to ignore collision
        if (proj.TryGetComponent<EnemyProjectile>(out var ep))
        {
            ep.Initialize(gameObject, dirToPlayer);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    // Allow collision damage for Kamikaze/Melee if they touch player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (attackType == AttackType.Melee || attackStyle == AttackStyle.Kamikaze)
        {
            if (collision.gameObject.TryGetComponent<PlayerController>(out var pc))
            {
                // Continuous damage or bounce might be better, but simple hit for now
                if (pc.TryGetComponent<Health>(out var hp))
                {
                    hp.TakeDamage(damage);
                }
            }
        }
    }
}
