using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator _animator;
    private PlayerMovement _movement;
    private Rigidbody2D _rb;

    // Cached parameter hashes for performance (avoids string lookups every frame)
    private static readonly int SpeedParam           = Animator.StringToHash("Speed");
    private static readonly int IsGroundedParam      = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalVelocityParam = Animator.StringToHash("VerticalVelocity");

    void Start()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<PlayerMovement>();
        _rb       = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (_movement == null || _animator == null || _rb == null) return;

        // 1. Horizontal speed -> drives Idle <-> Run transition
        float speed = Mathf.Abs(_rb.linearVelocity.x);
        _animator.SetFloat(SpeedParam, speed);

        // 2. IsGrounded -> drives AnyState -> Jump and Jump -> Idle/Run transitions
        _animator.SetBool(IsGroundedParam, _movement.IsGrounded);

        // 3. Vertical velocity -> available for future fall / apex animations
        _animator.SetFloat(VerticalVelocityParam, _rb.linearVelocity.y);
    }
}
