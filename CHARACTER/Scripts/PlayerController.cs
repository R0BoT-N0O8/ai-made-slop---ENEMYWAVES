using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothTime = 0.1f;
    private Vector2 currentVelocity;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float nextFireTime;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Ensure top-down physics
        mainCamera = Camera.main;
        
        // Listen to death
        GetComponent<Health>().OnDeath += HandleDeath;
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void HandleInput()
    {
        // Poll Keyboard directly
        if (Keyboard.current != null)
        {
            float x = 0f;
            float y = 0f;

            if (Keyboard.current.wKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;

            moveInput = new Vector2(x, y).normalized;
        }
    }

    private void Move()
    {
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, moveInput * moveSpeed, ref currentVelocity, smoothTime);

        // Flip sprite based on direction
        if (moveInput.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (moveInput.x > 0)
        {
            transform.localScale = Vector3.one;
        }
    }

    // RotateTowardsMouse removed as requested

    private void HandleDeath()
    {
        Debug.Log("Player Died!");
        // Disable controls or show game over screen
        this.enabled = false;
        rb.linearVelocity = Vector2.zero;
    }
}
