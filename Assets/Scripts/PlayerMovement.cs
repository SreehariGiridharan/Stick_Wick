using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure we have a ground check object if not assigned
        if (groundCheck == null)
        {
            GameObject newGroundCheck = new GameObject("GroundCheck");
            newGroundCheck.transform.parent = transform;
            newGroundCheck.transform.localPosition = new Vector3(0, -0.6f, 0); // Approx bottom of a standard sprite
            groundCheck = newGroundCheck.transform;
            Debug.Log("Created a GroundCheck child object automatically. Adjust its position if needed.");
        }
    }

    void Update()
    {
        // 1. Input Processing (New Input System)
        // Check for keyboard inputs
        if (Keyboard.current == null) return; // Basic safety check

        moveInput = 0f;
        
        // Horizontal Movement (Arrows or A/D)
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            moveInput = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            moveInput = 1f;

        // Jump Input (Up Arrow or Space)
        bool jumpPressed = Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
        {
            Jump();
        }

        // Flip the sprite to face direction
        if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void FixedUpdate()
    {
        // 2. Physics Movement
        // Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Apply Velocity (Using linearVelocity as updated by user for newer Unity versions)
        // If compilation fails here (older Unity), user can revert to 'velocity'
        #if UNITY_2023_3_OR_NEWER
            // Unity 6+ uses linearVelocity
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        #else
            // Older versions use velocity
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        #endif
    }

    void Jump()
    {
        #if UNITY_2023_3_OR_NEWER
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        #else
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        #endif
    }

    // Visualize the ground check circle in the editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
