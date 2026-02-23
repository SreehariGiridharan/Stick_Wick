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
    private bool _isGrounded;
    public bool IsGrounded => _isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Prevent the player from tilting/rotating when colliding or moving sideways
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Auto-create a ground check point if not assigned
        if (groundCheck == null)
        {
            GameObject newGroundCheck = new GameObject("GroundCheck");
            newGroundCheck.transform.parent = transform;
            newGroundCheck.transform.localPosition = new Vector3(0, -0.6f, 0);
            groundCheck = newGroundCheck.transform;
            Debug.Log("[PlayerMovement] Created GroundCheck automatically. Adjust its Y position if needed.");
        }

        // Auto-detect ground layer if not set
        if (groundLayer.value == 0)
        {
            int mask = LayerMask.GetMask("Ground");
            if (mask == 0) mask = LayerMask.GetMask("Default");
            if (mask != 0)
            {
                groundLayer = mask;
                Debug.LogWarning("[PlayerMovement] Ground Layer auto-set. Set it manually in Inspector for best results.");
            }
            else
            {
                Debug.LogError("[PlayerMovement] Ground Layer is not set! Please set it in the Inspector.");
            }
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        moveInput = 0f;

        // Horizontal Movement
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            moveInput = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            moveInput = 1f;

        // Jump
        bool jumpPressed = Keyboard.current.upArrowKey.wasPressedThisFrame
                        || Keyboard.current.spaceKey.wasPressedThisFrame
                        || Keyboard.current.wKey.wasPressedThisFrame;

        if (jumpPressed && _isGrounded)
            Jump();

        // Flip sprite direction
        if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void FixedUpdate()
    {
        // Ground check — also grab the platform we're standing on (if any)
        Collider2D groundHit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        _isGrounded = groundHit != null;

        // Platform velocity inheritance: if the ground is a moving kinematic body,
        // add its velocity so the player rides along instead of falling off.
        Vector2 platformVelocity = Vector2.zero;
        if (_isGrounded)
        {
            Rigidbody2D platformRb = groundHit.attachedRigidbody;
            if (platformRb != null && platformRb.bodyType == RigidbodyType2D.Kinematic)
                platformVelocity = platformRb.linearVelocity;
        }

        // Apply horizontal movement + platform offset, preserve vertical velocity (gravity/jump)
        rb.linearVelocity = new Vector2(moveInput * moveSpeed + platformVelocity.x, rb.linearVelocity.y + platformVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    // Draw ground check circle in Scene view for easy debugging
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
