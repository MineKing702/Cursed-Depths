using CursedDepths.Core.Settings;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerSettings settings;

    public float Speed;
    public float jumpForce;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Animator playerAnim;

    private float moveInput;

    void Start()
    {
        GameObject settingManager = GameObject.FindWithTag("SettingsManager");
        settings = settingManager.GetComponent<SettingsManager>().playerSettings;

        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
    }

    void Update()
    {
        moveInput = 0f;

        if (Input.GetKey(settings.WalkLeft))
        {
            moveInput = -1f;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (Input.GetKey(settings.WalkRight))
        {
            moveInput = 1f;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        playerAnim.SetBool("IsRunning", moveInput != 0f && isGrounded);

        if (Input.GetKeyDown(settings.Jump))
        {
            Jump();
        }
        if (Input.GetKeyDown(settings.Attack))
        {
            Punch();
        }

        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            playerAnim.SetBool("IsFalling", true);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * Speed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (isGrounded)
        {
            isGrounded = false;

            playerAnim.SetBool("IsRunning", false);
            playerAnim.SetBool("IsFalling", false);
            playerAnim.SetTrigger("Jump");

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void Punch()
    {
        playerAnim.SetBool("IsRunning", false);
        playerAnim.SetBool("IsFalling", false);
        playerAnim.SetTrigger("Punch");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isGrounded)
            {
                playerAnim.SetBool("IsFalling", false);
                playerAnim.SetTrigger("Land");
            }

            isGrounded = true;
        }
    }
}