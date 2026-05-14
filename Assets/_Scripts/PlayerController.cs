using System.Collections;
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
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float minimumFallDamageVelocity = 12f;
    [SerializeField] private float fallDamageMultiplier = 5f;
    [SerializeField] private int maxFallDamage = 50;
    [SerializeField] private float deathFallDuration = 0.35f;
    [SerializeField] private float deathFallAngle = 90f;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private int deathSmokeParticleCount = 16;

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    private PlayerSettings playerSettings;
    private bool isGrounded;
    private bool isDead;
    private bool isInvincible;
    private float horizontalInput;
    private float maxFallSpeed;
    private Coroutine invincibilityCoroutine;
    private Coroutine deathCoroutine;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private Vector3 cameraStartPos;
    private SpriteRenderer[] spriteRenderers;
    private Collider2D[] playerColliders;

    public Health playerHealth;
    public Coordinate playerCoords;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        playerColliders = GetComponentsInChildren<Collider2D>();
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        cameraStartPos = GameObject.FindGameObjectWithTag("MainCamera").transform.position;

        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        invincibilityDuration = Mathf.Max(0f, invincibilityDuration);
        minimumFallDamageVelocity = Mathf.Max(0f, minimumFallDamageVelocity);
        fallDamageMultiplier = Mathf.Max(0f, fallDamageMultiplier);
        maxFallDamage = Mathf.Max(0, maxFallDamage);
        deathFallDuration = Mathf.Max(0f, deathFallDuration);
        respawnDelay = Mathf.Max(0f, respawnDelay);
        deathSmokeParticleCount = Mathf.Max(0, deathSmokeParticleCount);

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
        if (isDead || playerSettings == null)
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

        if (!isGrounded)
        {
            maxFallSpeed = Mathf.Max(maxFallSpeed, -playerRigidbody.linearVelocity.y);

            if (playerRigidbody.linearVelocity.y < 0)
            {
                playerAnimator.SetBool("IsFalling", true);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        playerRigidbody.linearVelocity = new Vector2(horizontalInput * speed, playerRigidbody.linearVelocity.y);
    }

    /// <summary>
    /// Applies upward impulse when the player is on the ground.
    /// </summary>
    public void Jump()
    {
        if (isDead || !isGrounded)
        {
            return;
        }

        isGrounded = false;
        maxFallSpeed = 0f;
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
            ApplyFallDamage();
            playerAnimator.SetBool("IsFalling", false);

            if (!isDead)
            {
                playerAnimator.SetTrigger("Land");
            }
        }

        isGrounded = true;
        maxFallSpeed = 0f;
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

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
        {
            return;
        }

        damage = Mathf.Max(0, damage);
        if (damage == 0)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (HasAnimatorParameter("Hurt"))
        {
            playerAnimator.SetTrigger("Hurt");
        }

        StartInvincibility();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyFallDamage()
    {
        if (maxFallSpeed <= minimumFallDamageVelocity)
        {
            return;
        }

        int damage = Mathf.CeilToInt((maxFallSpeed - minimumFallDamageVelocity) * fallDamageMultiplier);
        damage = Mathf.Clamp(damage, 0, maxFallDamage);
        TakeDamage(damage);
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isInvincible = false;
        horizontalInput = 0f;
        playerRigidbody.linearVelocity = Vector2.zero;
        playerAnimator.SetBool("IsRunning", false);
        playerAnimator.SetBool("IsFalling", false);

        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        if (HasAnimatorParameter("Die"))
        {
            playerAnimator.SetTrigger("Die");
        }

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
        }

        deathCoroutine = StartCoroutine(DeathAndRespawnRoutine());
    }

    private IEnumerator DeathAndRespawnRoutine()
    {
        yield return FallOverRoutine();

        SpawnDeathSmoke(transform.position);
        SetSpriteRenderersEnabled(false);
        SetPlayerCollidersEnabled(false);
        playerRigidbody.simulated = false;

        yield return new WaitForSeconds(respawnDelay);

        Respawn();
        deathCoroutine = null;
    }

    private IEnumerator FallOverRoutine()
    {
        Quaternion startRotation = transform.rotation;
        Vector3 startEulerAngles = startRotation.eulerAngles;
        float fallDirection = Mathf.Approximately(startEulerAngles.y, 180f) ? 1f : -1f;
        Quaternion endRotation = Quaternion.Euler(startEulerAngles.x, startEulerAngles.y, fallDirection * deathFallAngle);
        float elapsedTime = 0f;

        while (elapsedTime < deathFallDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = deathFallDuration > 0f ? elapsedTime / deathFallDuration : 1f;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
            yield return null;
        }

        transform.rotation = endRotation;
    }

    private void SpawnDeathSmoke(Vector3 position)
    {
        if (deathSmokeParticleCount <= 0)
        {
            return;
        }

        GameObject smokeObject = new GameObject("Player Death Smoke");
        smokeObject.transform.position = position;

        ParticleSystem smoke = smokeObject.AddComponent<ParticleSystem>();
        smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = smoke.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.55f, 0.55f, 0.55f, 0.8f),
            new Color(0.9f, 0.9f, 0.9f, 0.35f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = smoke.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)deathSmokeParticleCount) });

        ParticleSystem.ShapeModule shape = smoke.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        ParticleSystemRenderer smokeRenderer = smoke.GetComponent<ParticleSystemRenderer>();
        if (spriteRenderers.Length > 0)
        {
            smokeRenderer.sortingLayerID = spriteRenderers[0].sortingLayerID;
            smokeRenderer.sortingOrder = spriteRenderers[0].sortingOrder + 1;
        }

        smoke.Play();
        Destroy(smokeObject, main.duration + 1f);
    }

    private void Respawn()
    {
        transform.position = startingPosition;
        transform.rotation = startingRotation;
        GameObject.FindGameObjectWithTag("MainCamera").transform.position = cameraStartPos;
        currentHealth = maxHealth;
        horizontalInput = 0f;
        maxFallSpeed = 0f;
        isGrounded = false;
        isInvincible = false;

        playerRigidbody.simulated = true;
        playerRigidbody.linearVelocity = Vector2.zero;
        SetPlayerCollidersEnabled(true);
        SetSpriteRenderersEnabled(true);
        playerAnimator.SetBool("IsRunning", false);
        playerAnimator.SetBool("IsFalling", false);



        isDead = false;
    }

    private void SetSpriteRenderersEnabled(bool isEnabled)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.enabled = isEnabled;
        }
    }

    private void SetPlayerCollidersEnabled(bool isEnabled)
    {
        foreach (Collider2D playerCollider in playerColliders)
        {
            playerCollider.enabled = isEnabled;
        }
    }

    private void StartInvincibility()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }

        invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        invincibilityCoroutine = null;
    }

    private bool HasAnimatorParameter(string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in playerAnimator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
}
