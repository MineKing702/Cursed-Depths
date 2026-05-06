using CursedDepths.Core.Settings;
using UnityEngine;

/// <summary>
/// Handles 2D player movement, jumping, and animation state synchronization.
///
/// Animation is driven from stable facts instead of animator-state guesses:
/// - IsGrounded comes from collision normals and/or an optional ground probe.
/// - IsRunning is true only when there is horizontal input while grounded.
/// - YVelocity mirrors Rigidbody2D vertical velocity every frame.
/// - Jump and Land are edge-triggered once per real jump/landing event.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public sealed class PlayerController : MonoBehaviour
{
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int YVelocityHash = Animator.StringToHash("YVelocity");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int LandHash = Animator.StringToHash("Land");

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Jump Forgiveness")]
    [SerializeField, Min(0f)] private float coyoteTime = 0.1f;
    [SerializeField, Min(0f)] private float jumpBufferTime = 0.1f;

    [Header("Ground Detection")]
    [SerializeField, Min(0f)] private float groundIgnoreAfterJump = 0.06f;
    [SerializeField] private Transform groundCheck;
    [SerializeField, Min(0.01f)] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField, Range(0f, 1f)] private float minGroundNormalY = 0.65f;
    [SerializeField] private string groundTag = "Ground";

    [Header("Animation")]
    [SerializeField] private float fallStartVelocity = -0.05f;
    [SerializeField] private float minLandingVelocity = -0.05f;

    [Header("Debugging")]
    [SerializeField] private bool logAnimationEvents;
    [SerializeField] private bool logAnimatorState;
    [SerializeField, Min(0.05f)] private float animatorStateLogInterval = 0.25f;
    [SerializeField] private bool drawGroundCheckGizmo = true;

    private readonly ContactPoint2D[] collisionContacts = new ContactPoint2D[8];

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    private PlayerSettings playerSettings;

    private bool isGrounded;
    private bool wasGrounded;
    private bool contactGrounded;
    private bool hasJumpedSinceLastGrounded;
    private bool wasFalling;
    private float horizontalInput;
    private float lastGroundedTime = float.NegativeInfinity;
    private float lastJumpPressedTime = float.NegativeInfinity;
    private float ignoreGroundUntilTime = float.NegativeInfinity;
    private float lastAnimatorStateLogTime;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();

        // 2D characters should be controlled by physics velocity, not animated root motion.
        playerAnimator.applyRootMotion = false;
    }

    private void Start()
    {
        SettingsManager settingsManager = SettingsManager.Instance;
        if (settingsManager == null)
        {
            Debug.LogError("SettingsManager is missing. Input defaults will be used.", this);
            return;
        }

        playerSettings = settingsManager.GetOrLoadSettings();
    }

    private void Update()
    {
        if (playerSettings == null)
        {
            return;
        }

        wasGrounded = isGrounded;
        horizontalInput = ReadHorizontalInput();
        UpdateFacingDirection(horizontalInput);
        RefreshGroundedState();

        if (Input.GetKeyDown(playerSettings.Jump))
        {
            lastJumpPressedTime = Time.time;
            LogAnimationEvent("Jump input buffered");
        }

        TryConsumeBufferedJump();
        UpdateAnimatorParameters();
        LogAnimatorStateIfNeeded();
    }

    private void FixedUpdate()
    {
        playerRigidbody.linearVelocity = new Vector2(horizontalInput * speed, playerRigidbody.linearVelocity.y);
    }

    /// <summary>
    /// Attempts to jump immediately. Kept public for UI/debug callers.
    /// </summary>
    public void Jump()
    {
        lastJumpPressedTime = Time.time;
        TryConsumeBufferedJump();
    }

    private void TryConsumeBufferedJump()
    {
        bool hasBufferedJump = Time.time <= lastJumpPressedTime + jumpBufferTime;
        bool canUseCoyoteTime = Time.time <= lastGroundedTime + coyoteTime;

        if (!hasBufferedJump || !canUseCoyoteTime)
        {
            return;
        }

        ExecuteJump();
    }

    private void ExecuteJump()
    {
        lastJumpPressedTime = float.NegativeInfinity;
        lastGroundedTime = float.NegativeInfinity;
        isGrounded = false;
        contactGrounded = false;
        hasJumpedSinceLastGrounded = true;
        ignoreGroundUntilTime = Time.time + groundIgnoreAfterJump;

        playerAnimator.ResetTrigger(LandHash);
        playerAnimator.SetBool(IsGroundedHash, false);
        playerAnimator.SetBool(IsRunningHash, false);
        playerAnimator.SetFloat(YVelocityHash, jumpForce);
        playerAnimator.SetTrigger(JumpHash);

        playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, 0f);
        playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        LogAnimationEvent($"Jump started. velocity={playerRigidbody.linearVelocity}");
    }

    private void RefreshGroundedState()
    {
        if (Time.time < ignoreGroundUntilTime)
        {
            isGrounded = false;
            return;
        }

        bool probeGrounded = IsGroundProbeTouchingGround();
        isGrounded = contactGrounded || probeGrounded;

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (!wasGrounded && isGrounded)
        {
            OnLanded();
        }
        else if (wasGrounded && !isGrounded)
        {
            LogAnimationEvent("Left ground");
        }
    }

    private void OnLanded()
    {
        bool shouldPlayLand = hasJumpedSinceLastGrounded || playerRigidbody.linearVelocity.y <= minLandingVelocity;
        hasJumpedSinceLastGrounded = false;

        playerAnimator.ResetTrigger(JumpHash);
        playerAnimator.SetBool(IsGroundedHash, true);

        if (shouldPlayLand)
        {
            playerAnimator.SetTrigger(LandHash);
            LogAnimationEvent($"Land triggered. velocity={playerRigidbody.linearVelocity}");
        }
        else
        {
            LogAnimationEvent($"Grounded without landing animation. velocity={playerRigidbody.linearVelocity}");
        }
    }

    private void UpdateAnimatorParameters()
    {
        float yVelocity = playerRigidbody.linearVelocity.y;
        bool isRunning = isGrounded && Mathf.Abs(horizontalInput) > Mathf.Epsilon;

        playerAnimator.SetBool(IsGroundedHash, isGrounded);
        playerAnimator.SetBool(IsRunningHash, isRunning);
        playerAnimator.SetFloat(YVelocityHash, yVelocity);

        bool isFalling = !isGrounded && yVelocity < fallStartVelocity;
        if (isFalling && !wasFalling)
        {
            LogAnimationEvent($"Fall threshold crossed. velocity={yVelocity:0.000}");
        }

        wasFalling = isFalling;

        // Fall is now derived by the Animator from !IsGrounded and YVelocity < fallStartVelocity.
        // This avoids a sticky IsFalling bool that can remain true after missed collision events.
    }

    private bool IsGroundProbeTouchingGround()
    {
        if (groundCheck == null || groundLayerMask.value == 0)
        {
            return false;
        }

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask) != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EvaluateGroundCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        EvaluateGroundCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGroundCollision(collision))
        {
            contactGrounded = false;
        }
    }

    private void EvaluateGroundCollision(Collision2D collision)
    {
        if (Time.time < ignoreGroundUntilTime || !IsGroundCollision(collision))
        {
            return;
        }

        contactGrounded = false;

        int contactCount = collision.GetContacts(collisionContacts);
        for (int i = 0; i < contactCount; i++)
        {
            if (collisionContacts[i].normal.y >= minGroundNormalY)
            {
                contactGrounded = true;
                return;
            }
        }
    }

    private bool IsGroundCollision(Collision2D collision)
    {
        bool matchesLayer = groundLayerMask.value != 0 && (groundLayerMask.value & (1 << collision.gameObject.layer)) != 0;
        bool matchesTag = !string.IsNullOrWhiteSpace(groundTag) && collision.gameObject.CompareTag(groundTag);
        return matchesLayer || matchesTag;
    }

    private float ReadHorizontalInput()
    {
        if (Input.GetKey(playerSettings.WalkLeft))
        {
            return -1f;
        }

        if (Input.GetKey(playerSettings.WalkRight))
        {
            return 1f;
        }

        return 0f;
    }

    private void UpdateFacingDirection(float input)
    {
        if (input < 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (input > 0f)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    private void LogAnimationEvent(string message)
    {
        if (logAnimationEvents)
        {
            Debug.Log($"[PlayerAnimation] {message} | grounded={isGrounded}, yVel={playerRigidbody.linearVelocity.y:0.000}, inputX={horizontalInput:0.0}", this);
        }
    }

    private void LogAnimatorStateIfNeeded()
    {
        if (!logAnimatorState || Time.time < lastAnimatorStateLogTime + animatorStateLogInterval)
        {
            return;
        }

        AnimatorStateInfo state = playerAnimator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[PlayerAnimation] Animator state hash={state.shortNameHash}, normalizedTime={state.normalizedTime:0.00}, grounded={isGrounded}, yVel={playerRigidbody.linearVelocity.y:0.000}", this);
        lastAnimatorStateLogTime = Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGroundCheckGizmo || groundCheck == null)
        {
            return;
        }

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
