using CursedDepths.Core.Settings;
using UnityEngine;

/// <summary>
/// Handles player movement and jump behavior using configured key bindings.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 7f;

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    private PlayerSettings playerSettings;
    private bool isGrounded;
    private float horizontalInput;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();

        SettingsManager settingsManager = SettingsManager.Instance;
        if (settingsManager == null)
        {
            Debug.LogError("SettingsManager is missing. Input defaults will be used.");
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

        horizontalInput = ReadHorizontalInput();
        UpdateFacingDirection(horizontalInput);
        playerAnimator.SetBool("IsRunning", horizontalInput != 0f && isGrounded);

        if (Input.GetKeyDown(playerSettings.Jump))
        {
            Jump();
        }

        if (!isGrounded && playerRigidbody.linearVelocity.y < 0)
        {
            playerAnimator.SetBool("IsFalling", true);
        }
    }

    private void FixedUpdate()
    {
        playerRigidbody.linearVelocity = new Vector2(horizontalInput * speed, playerRigidbody.linearVelocity.y);
    }

    /// <summary>
    /// Applies upward impulse when the player is on the ground.
    /// </summary>
    public void Jump()
    {
        if (!isGrounded)
        {
            return;
        }

        isGrounded = false;
        playerAnimator.SetBool("IsRunning", false);
        playerAnimator.SetBool("IsFalling", false);
        playerAnimator.SetTrigger("Jump");

        playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, 0f);
        playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        if (!isGrounded)
        {
            playerAnimator.SetBool("IsFalling", false);
            playerAnimator.SetTrigger("Land");
        }

        isGrounded = true;
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
}
