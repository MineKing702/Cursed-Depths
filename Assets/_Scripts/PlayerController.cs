using System.Collections;
using CursedDepths.Core.Settings;
using UnityEngine;

/// <summary>
/// Player controller using Bandit-style movement/animations,
/// with health, fall damage, death, respawn, and settings support.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private Vector3 rightFacingScale = new Vector3(1.4f, 1.4f, 1f);
    [SerializeField] private Vector3 leftFacingScale = new Vector3(-1.4f, 1.4f, 1f);

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;

    [Header("Fall Damage")]
    [SerializeField] private float minimumFallDamageVelocity = 12f;
    [SerializeField] private float fallDamageMultiplier = 5f;
    [SerializeField] private int maxFallDamage = 50;

    [Header("Death / Respawn")]
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private int deathSmokeParticleCount = 16;

    private Animator playerAnimator;
    private Rigidbody2D playerRigidbody;
    private Sensor_Bandit groundSensor;
    private PlayerSettings playerSettings;

    private bool isGrounded;
    private bool combatIdle;
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
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody2D>();

        Transform groundSensorTransform = transform.Find("GroundSensor");
        if (groundSensorTransform != null)
        {
            groundSensor = groundSensorTransform.GetComponent<Sensor_Bandit>();
        }

        if (groundSensor == null)
        {
            Debug.LogError("GroundSensor with Sensor_Bandit component is missing.");
        }

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        playerColliders = GetComponentsInChildren<Collider2D>();

        startingPosition = transform.position;
        startingRotation = transform.rotation;

        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            cameraStartPos = mainCamera.transform.position;
        }

        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;

        invincibilityDuration = Mathf.Max(0f, invincibilityDuration);
        minimumFallDamageVelocity = Mathf.Max(0f, minimumFallDamageVelocity);
        fallDamageMultiplier = Mathf.Max(0f, fallDamageMultiplier);
        maxFallDamage = Mathf.Max(0, maxFallDamage);
        respawnDelay = Mathf.Max(0f, respawnDelay);
        deathSmokeParticleCount = Mathf.Max(0, deathSmokeParticleCount);

        SettingsManager settingsManager = SettingsManager.Instance;
        if (settingsManager != null)
        {
            playerSettings = settingsManager.GetOrLoadSettings();
        }
        else
        {
            Debug.LogWarning("SettingsManager is missing. Falling back to Unity Horizontal input, Space jump, and mouse attack.");
        }
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        UpdateGroundedState();

        horizontalInput = ReadHorizontalInput();

        UpdateFacingDirection(horizontalInput);

        playerRigidbody.linearVelocity = new Vector2(
            horizontalInput * speed,
            playerRigidbody.linearVelocity.y
        );

        if (!isGrounded)
        {
            maxFallSpeed = Mathf.Max(maxFallSpeed, -playerRigidbody.linearVelocity.y);
        }

        if (HasAnimatorParameter("AirSpeed"))
        {
            playerAnimator.SetFloat("AirSpeed", playerRigidbody.linearVelocity.y);
        }

        HandleActionsAndAnimations();
    }

    private void UpdateGroundedState()
    {
        if (groundSensor == null)
        {
            return;
        }

        bool sensorGrounded = groundSensor.State();

        if (!isGrounded && sensorGrounded)
        {
            isGrounded = true;

            ApplyFallDamage();

            if (HasAnimatorParameter("Grounded"))
            {
                playerAnimator.SetBool("Grounded", true);
            }

            if (!isDead && HasAnimatorParameter("Land"))
            {
                playerAnimator.SetTrigger("Land");
            }

            maxFallSpeed = 0f;
        }

        if (isGrounded && !sensorGrounded)
        {
            isGrounded = false;

            if (HasAnimatorParameter("Grounded"))
            {
                playerAnimator.SetBool("Grounded", false);
            }
        }
    }

    private void HandleActionsAndAnimations()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(10);
        }
        else if (AttackWasPressed())
        {
            Attack();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            combatIdle = !combatIdle;
        }
        else if (JumpWasPressed() && isGrounded)
        {
            Jump();
        }
        else if (Mathf.Abs(horizontalInput) > Mathf.Epsilon)
        {
            SetAnimState(2);
        }
        else if (combatIdle)
        {
            SetAnimState(1);
        }
        else
        {
            SetAnimState(0);
        }
    }

    public void Jump()
    {
        if (isDead || !isGrounded)
        {
            return;
        }

        if (HasAnimatorParameter("Jump"))
        {
            playerAnimator.SetTrigger("Jump");
        }

        isGrounded = false;
        maxFallSpeed = 0f;

        if (HasAnimatorParameter("Grounded"))
        {
            playerAnimator.SetBool("Grounded", false);
        }

        playerRigidbody.linearVelocity = new Vector2(
            playerRigidbody.linearVelocity.x,
            jumpForce
        );

        if (groundSensor != null)
        {
            groundSensor.Disable(0.2f);
        }
    }

    private void Attack()
    {
        if (isDead)
        {
            return;
        }

        if (HasAnimatorParameter("Attack"))
        {
            playerAnimator.SetTrigger("Attack");
        }
    }

    private float ReadHorizontalInput()
    {
        if (playerSettings != null)
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

        return Input.GetAxis("Horizontal");
    }

    private bool JumpWasPressed()
    {
        if (playerSettings != null)
        {
            return Input.GetKeyDown(playerSettings.Jump);
        }

        return Input.GetKeyDown(KeyCode.Space);
    }

    private bool AttackWasPressed()
    {
        bool mouseAttackPressed = Input.GetMouseButtonDown(0);

        if (playerSettings != null)
        {
            return mouseAttackPressed || Input.GetKeyDown(playerSettings.Attack);
        }

        return mouseAttackPressed;
    }

    private void UpdateFacingDirection(float input)
    {
        if (input > 0f)
        {
            transform.localScale = rightFacingScale;
        }
        else if (input < 0f)
        {
            transform.localScale = leftFacingScale;
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

        int damage = Mathf.CeilToInt(
            (maxFallSpeed - minimumFallDamageVelocity) * fallDamageMultiplier
        );

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

        SetAnimState(0);

        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        // This uses the Bandit death animation trigger.
        if (HasAnimatorParameter("Death"))
        {
            playerAnimator.SetTrigger("Death");
        }
        else if (HasAnimatorParameter("Die"))
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
        // Wait so the Bandit death animation can play.
        yield return new WaitForSeconds(respawnDelay);

        SpawnDeathSmoke(transform.position);

        SetSpriteRenderersEnabled(false);
        SetPlayerCollidersEnabled(false);
        playerRigidbody.simulated = false;

        Respawn();

        deathCoroutine = null;
    }

    private void Respawn()
    {
        transform.position = startingPosition;
        transform.rotation = startingRotation;
        transform.localScale = rightFacingScale;

        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            mainCamera.transform.position = cameraStartPos;
        }

        currentHealth = maxHealth;
        horizontalInput = 0f;
        maxFallSpeed = 0f;
        isGrounded = false;
        isInvincible = false;
        combatIdle = false;

        playerRigidbody.simulated = true;
        playerRigidbody.linearVelocity = Vector2.zero;

        SetPlayerCollidersEnabled(true);
        SetSpriteRenderersEnabled(true);

        if (HasAnimatorParameter("Grounded"))
        {
            playerAnimator.SetBool("Grounded", false);
        }

        // Bandit animator uses Recover to leave death state.
        if (HasAnimatorParameter("Recover"))
        {
            playerAnimator.SetTrigger("Recover");
        }

        SetAnimState(0);

        isDead = false;
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
            new Color(0.9f, 0.9f, 0.9f, 0.35f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = smoke.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)deathSmokeParticleCount)
        });

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

    private void SetAnimState(int state)
    {
        if (HasAnimatorParameter("AnimState"))
        {
            playerAnimator.SetInteger("AnimState", state);
        }
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