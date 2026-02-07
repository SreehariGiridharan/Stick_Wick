using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleOscillator : MonoBehaviour
{
    [Header("Motion Settings")]
    [Tooltip("How far (distance) or how much (angle in degrees) to move/rotate.")]
    public float amplitude = 5f;

    [Tooltip("How fast the cycle repeats.")]
    public float frequency = 1f;

    [Tooltip("Offset the starting time (0-1) to desynchronize multiple objects.")]
    public float phaseShift = 0f;

    [Header("Type")]
    [Tooltip("If true, object moves back and forth. If false, it rotates like a swing.")]
    public bool usePosition = true;

    [Tooltip("The direction along which to move (e.g., (1,0) for X axis). Ignored for rotation.")]
    public Vector2 moveDirection = Vector2.right;

    private Rigidbody2D rb;
    private Vector2 initialPosition;
    private float initialRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // We use Kinematic because we want absolute control over movement
        // without gravity affection, but still interacting with other physics objects.
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true; // Ensure collisions are detected properly

        // Cache initial state
        initialPosition = rb.position;
        initialRotation = rb.rotation;
        
        // Normalize direction to ensure consistent behavior
        moveDirection.Normalize();
    }

    void FixedUpdate()
    {
        // Calculate sine wave value (-1 to 1) based on time
        float sineValue = Mathf.Sin((Time.time * frequency + phaseShift) * Mathf.PI * 2);

        if (usePosition)
        {
            // Apply position offset: initial + (direction * amount)
            Vector2 targetPosition = initialPosition + (moveDirection * sineValue * amplitude);
            rb.MovePosition(targetPosition);
        }
        else
        {
            // Apply rotation offset: Rotate around the Z axis by the angle (degrees)
            // This creates a swing motion relative to the initial rotation
            float targetRotation = initialRotation + (sineValue * amplitude);
            rb.MoveRotation(targetRotation);
        }
    }
}
