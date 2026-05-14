using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Reusable 2D enemy AI controller that drives movement, custom enemy animations,
/// health, coordinates, and melee attacks with an explicit EnemyState state machine.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public sealed class EnemyController : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float loseInterestRange = 8f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private bool lockRotation = true;
    [SerializeField] private Transform[] patrolPoints = Array.Empty<Transform>();
    [SerializeField] private Vector3 rightFacingScale = new Vector3(1.4f, 1.4f, 1f);
    [SerializeField] private Vector3 leftFacingScale = new Vector3(-1.4f, 1.4f, 1f);
    [SerializeField] private bool startPatrolling = true;

    [Header("Ledge Detection")]
    [SerializeField] private bool avoidLedges = true;
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float ledgeRaycastDistance = 1.25f;
    [SerializeField] private float ledgeRaycastHorizontalOffset = 0.1f;
    [SerializeField] private float ledgeRaycastVerticalOffset = 0.05f;
    [SerializeField] private bool drawLedgeRaycasts;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int maxHealth = 100;
    [SerializeField, Range(0f, 1f)] private float lowHealthPercentToFlee = 0.25f;
    [SerializeField] private bool canFlee;
    [SerializeField] private float hurtDuration = 0.25f;
    [SerializeField] private float patrolPointArrivalDistance = 0.15f;

    [Header("Shared Core Data")]
    public Health enemyHealth;
    public Coordinate enemyCoords;

    [Header("Animation")]
    [SerializeField] private bool driveAnimator = true;
    [SerializeField] private bool useBanditAnimatorParameters;
    [SerializeField] private string stateParameterName = "EnemyState";
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private string movingParameterName = "IsMoving";
    [SerializeField] private string groundedParameterName = "Grounded";
    [SerializeField] private string verticalSpeedParameterName = "VerticalSpeed";
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string hurtTriggerName = "Hurt";
    [SerializeField] private string deathTriggerName = "Death";

    [Header("Debug")]
    [SerializeField] private EnemyState currentState;

    private Rigidbody2D enemyRigidbody;
    private Animator enemyAnimator;
    private Sensor_Bandit groundSensor;
    private Collider2D[] enemyColliders;
    private PlayerController playerController;
    private Health targetHealth;
    private Coordinate targetCoords;

    private int patrolIndex;
    private float lastAttackTime = float.NegativeInfinity;
    private float hurtEndsAt;
    private int fallbackCurrentHealth;
    private float lockedRotation;
    private bool isGrounded;
    private bool isDead;
    private bool missingPlayerWarningLogged;
    private bool missingTargetHealthWarningLogged;

    /// <summary>
    /// Gets the enemy's current AI state for UI, debugging, or external systems.
    /// </summary>
    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        enemyRigidbody = GetComponent<Rigidbody2D>();
        enemyAnimator = GetComponent<Animator>();
        enemyColliders = GetComponentsInChildren<Collider2D>();
        lockedRotation = enemyRigidbody.rotation;

        if (lockRotation)
        {
            enemyRigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
            StabilizeRotation();
        }

        Transform groundSensorTransform = transform.Find("GroundSensor");
        if (groundSensorTransform != null)
        {
            groundSensor = groundSensorTransform.GetComponent<Sensor_Bandit>();
        }

        maxHealth = Mathf.Max(1, maxHealth);
        attackDamage = Mathf.Max(0, attackDamage);
        attackCooldown = Mathf.Max(0f, attackCooldown);
        detectionRange = Mathf.Max(0f, detectionRange);
        attackRange = Mathf.Max(0f, attackRange);
        loseInterestRange = Mathf.Max(detectionRange, loseInterestRange);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        patrolSpeed = Mathf.Max(0f, patrolSpeed);
        fleeSpeed = Mathf.Max(0f, fleeSpeed);
        hurtDuration = Mathf.Max(0f, hurtDuration);
        patrolPointArrivalDistance = Mathf.Max(0.01f, patrolPointArrivalDistance);
        ledgeRaycastDistance = Mathf.Max(0.01f, ledgeRaycastDistance);
        ledgeRaycastHorizontalOffset = Mathf.Max(0f, ledgeRaycastHorizontalOffset);
        ledgeRaycastVerticalOffset = Mathf.Max(0f, ledgeRaycastVerticalOffset);
        lowHealthPercentToFlee = Mathf.Clamp01(lowHealthPercentToFlee);

        fallbackCurrentHealth = maxHealth;
        InitializeSharedCoreData();
    }

    private void Start()
    {
        FindPlayerTarget();
        SyncCoordinate(enemyCoords, transform.position);
        ChangeState(HasPatrolRoute() && startPatrolling ? EnemyState.Patrolling : EnemyState.Idle);
    }

    private void Update()
    {
        SyncCoordinate(enemyCoords, transform.position);
        UpdateGroundedState();

        if (isDead)
        {
            UpdateAnimator();
            return;
        }

        if (playerTarget == null)
        {
            TryFindPlayerFromLayer();
        }

        EvaluateStateTransitions();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Patrolling:
                HandlePatrolling();
                break;
            case EnemyState.Chasing:
                HandleChasing();
                break;
            case EnemyState.Attacking:
                HandleAttacking();
                break;
            case EnemyState.Fleeing:
                HandleFleeing();
                break;
            case EnemyState.Hurt:
                HandleHurt();
                break;
            case EnemyState.Dead:
                HandleDead();
                break;
        }

        StabilizeRotation();
    }

    /// <summary>
    /// Applies damage through the shared Health object when possible and transitions the enemy into Hurt or Dead.
    /// </summary>
    /// <param name="amount">The non-negative amount of damage to apply.</param>
    public void TakeDamage(int amount)
    {
        if (currentState == EnemyState.Dead || isDead)
        {
            return;
        }

        amount = Mathf.Max(0, amount);
        if (amount == 0)
        {
            return;
        }

        TryApplyHealthDamage(enemyHealth, amount);
        fallbackCurrentHealth = Mathf.Clamp(fallbackCurrentHealth - amount, 0, maxHealth);

        if (GetCurrentHealth() <= 0)
        {
            ChangeState(EnemyState.Dead);
            return;
        }

        ChangeState(ShouldFlee() ? EnemyState.Fleeing : EnemyState.Hurt);
    }

    private void InitializeSharedCoreData()
    {
        if (enemyHealth == null)
        {
            enemyHealth = CreateOrGetSharedData<Health>();
        }

        if (enemyCoords == null)
        {
            enemyCoords = CreateOrGetSharedData<Coordinate>();
        }

        SetHealthMaximum(enemyHealth, maxHealth);
        SetHealthCurrent(enemyHealth, maxHealth);
    }

    private T CreateOrGetSharedData<T>() where T : class
    {
        Type type = typeof(T);

        if (typeof(Component).IsAssignableFrom(type))
        {
            Component component = GetComponent(type);
            if (component == null)
            {
                component = gameObject.AddComponent(type);
            }

            return component as T;
        }

        if (typeof(ScriptableObject).IsAssignableFrom(type))
        {
            return ScriptableObject.CreateInstance(type) as T;
        }

        ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
        return constructor != null ? Activator.CreateInstance(type) as T : null;
    }

    private void FindPlayerTarget()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (playerTarget == null && taggedPlayer != null)
        {
            playerTarget = taggedPlayer.transform;
        }

        if (playerTarget != null)
        {
            playerController = playerTarget.GetComponent<PlayerController>();
        }

        if (playerController == null && taggedPlayer != null)
        {
            playerController = taggedPlayer.GetComponent<PlayerController>();
        }

        if (playerController != null)
        {
            targetHealth = playerController.playerHealth;
            targetCoords = playerController.playerCoords;
        }

        if (playerTarget == null)
        {
            LogMissingPlayerWarning("EnemyController could not find a GameObject tagged 'Player'. Enemy will stay idle until a target is found.");
        }
        else if (targetHealth == null || targetCoords == null)
        {
            Debug.LogWarning($"Player target '{playerTarget.name}' is missing Health or Coordinate references. Enemy will still track Transform position, but library health/coordinate interactions may be limited.", this);
        }
    }

    private void TryFindPlayerFromLayer()
    {
        if (playerLayer.value == 0)
        {
            LogMissingPlayerWarning("EnemyController has no player target and no playerLayer configured. Enemy remains idle.");
            return;
        }

        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (playerCollider == null)
        {
            LogMissingPlayerWarning("EnemyController could not find a player in detection range. Enemy remains idle.");
            return;
        }

        playerTarget = playerCollider.transform;
        playerController = playerCollider.GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            targetHealth = playerController.playerHealth;
            targetCoords = playerController.playerCoords;
        }
    }

    private void EvaluateStateTransitions()
    {
        if (currentState == EnemyState.Dead || isDead)
        {
            return;
        }

        if (GetCurrentHealth() <= 0)
        {
            ChangeState(EnemyState.Dead);
            return;
        }

        if (currentState == EnemyState.Hurt)
        {
            if (Time.time < hurtEndsAt)
            {
                return;
            }
        }

        if (ShouldFlee())
        {
            ChangeState(EnemyState.Fleeing);
            return;
        }

        if (!HasTarget())
        {
            ChangeState(HasPatrolRoute() && startPatrolling ? EnemyState.Patrolling : EnemyState.Idle);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, GetTargetPosition());

        if (currentState == EnemyState.Fleeing && distanceToPlayer < loseInterestRange)
        {
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chasing);
        }
        else if (distanceToPlayer > loseInterestRange)
        {
            ChangeState(HasPatrolRoute() && startPatrolling ? EnemyState.Patrolling : EnemyState.Idle);
        }
    }

    private void ChangeState(EnemyState nextState)
    {
        if (currentState == EnemyState.Dead || currentState == nextState)
        {
            return;
        }

        currentState = nextState;

        switch (currentState)
        {
            case EnemyState.Idle:
                SetAnimatorStateValue(GetAnimatorStateValue(currentState));
                break;
            case EnemyState.Patrolling:
            case EnemyState.Chasing:
            case EnemyState.Fleeing:
                SetAnimatorStateValue(GetAnimatorStateValue(currentState));
                break;
            case EnemyState.Attacking:
                StopHorizontalMovement();
                break;
            case EnemyState.Hurt:
                hurtEndsAt = Time.time + hurtDuration;
                TriggerAnimator(hurtTriggerName);
                StopHorizontalMovement();
                break;
            case EnemyState.Dead:
                isDead = true;
                StopAllMovement();
                SetAnimatorStateValue(GetAnimatorStateValue(currentState));
                TriggerAnimator(deathTriggerName);
                break;
        }
    }

    private void HandleIdle()
    {
        StopHorizontalMovement();
    }

    private void HandlePatrolling()
    {
        if (!HasPatrolRoute())
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        Transform patrolPoint = patrolPoints[patrolIndex];
        if (patrolPoint == null)
        {
            AdvancePatrolPoint();
            return;
        }

        float direction = Mathf.Sign(patrolPoint.position.x - transform.position.x);
        float distance = Mathf.Abs(patrolPoint.position.x - transform.position.x);

        if (distance <= patrolPointArrivalDistance)
        {
            AdvancePatrolPoint();
            return;
        }

        if (!MoveHorizontally(direction, patrolSpeed))
        {
            AdvancePatrolPoint();
        }
    }

    private void HandleChasing()
    {
        if (!HasTarget())
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        float direction = Mathf.Sign(GetTargetPosition().x - transform.position.x);
        MoveHorizontally(direction, moveSpeed);
    }

    private void HandleAttacking()
    {
        StopHorizontalMovement();

        if (!HasTarget())
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        FaceDirection(GetTargetPosition().x - transform.position.x);

        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        TriggerAnimator(attackTriggerName);
        ApplyDamageToTarget();
        lastAttackTime = Time.time;
    }

    private void HandleFleeing()
    {
        if (!canFlee)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (!HasTarget())
        {
            MoveHorizontally(-GetFacingSign(), fleeSpeed);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, GetTargetPosition());
        if (distanceToPlayer >= loseInterestRange)
        {
            ChangeState(HasPatrolRoute() && startPatrolling ? EnemyState.Patrolling : EnemyState.Idle);
            return;
        }

        float direction = Mathf.Sign(transform.position.x - GetTargetPosition().x);
        MoveHorizontally(direction, fleeSpeed);
    }

    private void HandleHurt()
    {
        StopHorizontalMovement();
    }

    private void HandleDead()
    {
        StopAllMovement();
    }

    private void ApplyDamageToTarget()
    {
        if (attackDamage <= 0)
        {
            return;
        }

        if (targetHealth != null && TryApplyHealthDamage(targetHealth, attackDamage))
        {
            return;
        }

        if (playerController != null)
        {
            playerController.TakeDamage(attackDamage);
            return;
        }

        if (!missingTargetHealthWarningLogged)
        {
            Debug.LogWarning("EnemyController cannot damage the player because the target Health reference is missing.", this);
            missingTargetHealthWarningLogged = true;
        }
    }

    private bool MoveHorizontally(float direction, float speed)
    {
        if (Mathf.Abs(direction) <= Mathf.Epsilon || speed <= 0f)
        {
            StopHorizontalMovement();
            return false;
        }

        FaceDirection(direction);

        if (!HasGroundAhead(direction))
        {
            StopHorizontalMovement();
            return false;
        }

        enemyRigidbody.linearVelocity = new Vector2(direction * speed, enemyRigidbody.linearVelocity.y);
        return true;
    }

    private bool HasGroundAhead(float direction)
    {
        if (!avoidLedges || groundLayer.value == 0 || (groundSensor != null && !isGrounded))
        {
            return true;
        }

        Vector2 origin = GetLedgeRaycastOrigin(direction);
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, ledgeRaycastDistance, groundLayer);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && !IsOwnCollider(hit.collider))
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 GetLedgeRaycastOrigin(float direction)
    {
        Bounds bounds = GetMovementBounds();
        float xOffset = bounds.extents.x + ledgeRaycastHorizontalOffset;
        return new Vector2(
            bounds.center.x + Mathf.Sign(direction) * xOffset,
            bounds.min.y + ledgeRaycastVerticalOffset
        );
    }

    private Bounds GetMovementBounds()
    {
        if (enemyColliders == null || enemyColliders.Length == 0)
        {
            return new Bounds(transform.position, Vector3.one);
        }

        bool hasBounds = false;
        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider == null || !enemyCollider.enabled || enemyCollider.isTrigger)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = enemyCollider.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(enemyCollider.bounds);
            }
        }

        return hasBounds ? combinedBounds : new Bounds(transform.position, Vector3.one);
    }

    private bool IsOwnCollider(Collider2D otherCollider)
    {
        if (enemyColliders == null)
        {
            return false;
        }

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (otherCollider == enemyCollider)
            {
                return true;
            }
        }

        return false;
    }

    private void StopHorizontalMovement()
    {
        enemyRigidbody.linearVelocity = new Vector2(0f, enemyRigidbody.linearVelocity.y);
    }

    private void StopAllMovement()
    {
        enemyRigidbody.linearVelocity = Vector2.zero;
        enemyRigidbody.angularVelocity = 0f;
    }

    private void StabilizeRotation()
    {
        if (!lockRotation)
        {
            return;
        }

        enemyRigidbody.angularVelocity = 0f;
        enemyRigidbody.SetRotation(lockedRotation);
    }

    private void FaceDirection(float direction)
    {
        if (direction > Mathf.Epsilon)
        {
            transform.localScale = rightFacingScale;
        }
        else if (direction < -Mathf.Epsilon)
        {
            transform.localScale = leftFacingScale;
        }
    }

    private float GetFacingSign()
    {
        return Mathf.Sign(transform.localScale.x == 0f ? rightFacingScale.x : transform.localScale.x);
    }

    private void AdvancePatrolPoint()
    {
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    private bool HasPatrolRoute()
    {
        return patrolPoints != null && patrolPoints.Length > 0;
    }

    private bool HasTarget()
    {
        return playerTarget != null;
    }

    private Vector2 GetTargetPosition()
    {
        if (targetCoords != null && TryReadCoordinate(targetCoords, out Vector2 coordinatePosition))
        {
            return coordinatePosition;
        }

        return playerTarget != null ? playerTarget.position : transform.position;
    }

    private bool ShouldFlee()
    {
        if (!canFlee || GetHealthPercent() > lowHealthPercentToFlee || !HasTarget())
        {
            return false;
        }

        return Vector2.Distance(transform.position, GetTargetPosition()) < loseInterestRange;
    }

    private int GetCurrentHealth()
    {
        return TryReadHealthCurrent(enemyHealth, out int currentHealth) ? currentHealth : fallbackCurrentHealth;
    }

    private float GetHealthPercent()
    {
        int currentHealth = GetCurrentHealth();
        int maximumHealth = TryReadHealthMaximum(enemyHealth, out int maximum) ? Mathf.Max(1, maximum) : maxHealth;
        return currentHealth / (float)Mathf.Max(1, maximumHealth);
    }

    private void UpdateGroundedState()
    {
        if (groundSensor == null)
        {
            return;
        }

        isGrounded = groundSensor.State();
    }

    private void UpdateAnimator()
    {
        if (!driveAnimator || enemyAnimator == null)
        {
            return;
        }

        string groundedName = useBanditAnimatorParameters ? "Grounded" : groundedParameterName;
        string verticalSpeedName = useBanditAnimatorParameters ? "AirSpeed" : verticalSpeedParameterName;

        SetAnimatorBool(groundedName, isGrounded);
        SetAnimatorFloat(verticalSpeedName, enemyRigidbody.linearVelocity.y);

        if (!useBanditAnimatorParameters)
        {
            SetAnimatorFloat(speedParameterName, Mathf.Abs(enemyRigidbody.linearVelocity.x));
            SetAnimatorBool(movingParameterName, Mathf.Abs(enemyRigidbody.linearVelocity.x) > Mathf.Epsilon);
        }
        SetAnimatorStateValue(GetAnimatorStateValue(currentState));
    }

    private int GetAnimatorStateValue(EnemyState state)
    {
        if (!useBanditAnimatorParameters)
        {
            return (int)state;
        }

        switch (state)
        {
            case EnemyState.Attacking:
                return 1;
            case EnemyState.Patrolling:
            case EnemyState.Chasing:
            case EnemyState.Fleeing:
                return 2;
            default:
                return 0;
        }
    }

    private void SetAnimatorStateValue(int state)
    {
        if (!driveAnimator || enemyAnimator == null)
        {
            return;
        }

        string parameterName = useBanditAnimatorParameters ? "AnimState" : stateParameterName;
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Int))
        {
            enemyAnimator.SetInteger(parameterName, state);
        }
    }

    private void SetAnimatorFloat(string parameterName, float value)
    {
        if (!driveAnimator || enemyAnimator == null || string.IsNullOrWhiteSpace(parameterName))
        {
            return;
        }

        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Float))
        {
            enemyAnimator.SetFloat(parameterName, value);
        }
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (!driveAnimator || enemyAnimator == null || string.IsNullOrWhiteSpace(parameterName))
        {
            return;
        }

        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            enemyAnimator.SetBool(parameterName, value);
        }
    }

    private void TriggerAnimator(string parameterName)
    {
        if (!driveAnimator || enemyAnimator == null || string.IsNullOrWhiteSpace(parameterName))
        {
            return;
        }

        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Trigger))
        {
            enemyAnimator.SetTrigger(parameterName);
        }
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (string.IsNullOrWhiteSpace(parameterName) || enemyAnimator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in enemyAnimator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }

    private void SyncCoordinate(Coordinate coordinate, Vector3 position)
    {
        if (coordinate == null)
        {
            return;
        }

        Type type = coordinate.GetType();
        SetNumericMember(type, coordinate, "x", position.x);
        SetNumericMember(type, coordinate, "y", position.y);
        SetNumericMember(type, coordinate, "z", position.z);
        SetNumericMember(type, coordinate, "X", position.x);
        SetNumericMember(type, coordinate, "Y", position.y);
        SetNumericMember(type, coordinate, "Z", position.z);
        SetVectorMember(type, coordinate, "Position", position);
        SetVectorMember(type, coordinate, "position", position);
        SetVectorMember(type, coordinate, "Value", position);
        SetVectorMember(type, coordinate, "value", position);
    }

    private bool TryReadCoordinate(Coordinate coordinate, out Vector2 position)
    {
        position = default;
        Type type = coordinate.GetType();

        if (TryGetVectorMember(type, coordinate, "Position", out position) ||
            TryGetVectorMember(type, coordinate, "position", out position) ||
            TryGetVectorMember(type, coordinate, "Value", out position) ||
            TryGetVectorMember(type, coordinate, "value", out position))
        {
            return true;
        }

        bool hasX = TryGetNumericMember(type, coordinate, "x", out float x) || TryGetNumericMember(type, coordinate, "X", out x);
        bool hasY = TryGetNumericMember(type, coordinate, "y", out float y) || TryGetNumericMember(type, coordinate, "Y", out y);
        if (hasX && hasY)
        {
            position = new Vector2(x, y);
            return true;
        }

        return false;
    }

    private void SetHealthMaximum(Health health, int value)
    {
        if (health == null)
        {
            return;
        }

        Type type = health.GetType();
        SetNumericMember(type, health, "MaxHealth", value);
        SetNumericMember(type, health, "maxHealth", value);
        SetNumericMember(type, health, "maxHp", value);
        SetNumericMember(type, health, "MaximumHealth", value);
        SetNumericMember(type, health, "maximumHealth", value);
    }

    private void SetHealthCurrent(Health health, int value)
    {
        if (health == null)
        {
            return;
        }

        Type type = health.GetType();
        SetNumericMember(type, health, "CurrentHealth", value);
        SetNumericMember(type, health, "currentHealth", value);
        SetNumericMember(type, health, "currentHp", value);
        SetNumericMember(type, health, "HealthPoints", value);
        SetNumericMember(type, health, "health", value);
    }

    private bool TryApplyHealthDamage(Health health, int amount)
    {
        if (health == null)
        {
            return false;
        }

        Type type = health.GetType();
        string[] methodNames = { "TakeDamage", "DealDamage", "Damage", "ApplyDamage", "RemoveHealth", "Subtract" };

        foreach (string methodName in methodNames)
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
            if (method != null)
            {
                method.Invoke(health, new object[] { amount });
                return true;
            }

            method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(float) }, null);
            if (method != null)
            {
                method.Invoke(health, new object[] { (float)amount });
                return true;
            }
        }

        if (TryReadHealthCurrent(health, out int currentHealth))
        {
            SetHealthCurrent(health, Mathf.Max(0, currentHealth - amount));
            return true;
        }

        return false;
    }

    private bool TryReadHealthCurrent(Health health, out int currentHealth)
    {
        currentHealth = fallbackCurrentHealth;
        if (health == null)
        {
            return false;
        }

        Type type = health.GetType();
        return TryGetIntMember(type, health, "CurrentHealth", out currentHealth) ||
               TryGetIntMember(type, health, "currentHealth", out currentHealth) ||
               TryGetIntMember(type, health, "currentHp", out currentHealth) ||
               TryGetIntMember(type, health, "HealthPoints", out currentHealth) ||
               TryGetIntMember(type, health, "health", out currentHealth);
    }

    private bool TryReadHealthMaximum(Health health, out int maximumHealth)
    {
        maximumHealth = maxHealth;
        if (health == null)
        {
            return false;
        }

        Type type = health.GetType();
        return TryGetIntMember(type, health, "MaxHealth", out maximumHealth) ||
               TryGetIntMember(type, health, "maxHealth", out maximumHealth) ||
               TryGetIntMember(type, health, "maxHp", out maximumHealth) ||
               TryGetIntMember(type, health, "MaximumHealth", out maximumHealth) ||
               TryGetIntMember(type, health, "maximumHealth", out maximumHealth);
    }

    private static bool TryGetIntMember(Type type, object target, string memberName, out int value)
    {
        value = 0;
        if (TryGetNumericMember(type, target, memberName, out float floatValue))
        {
            value = Mathf.RoundToInt(floatValue);
            return true;
        }

        return false;
    }

    private static bool TryGetNumericMember(Type type, object target, string memberName, out float value)
    {
        value = 0f;
        PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.CanRead && TryConvertToFloat(property.GetValue(target), out value))
        {
            return true;
        }

        FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return field != null && TryConvertToFloat(field.GetValue(target), out value);
    }

    private static void SetNumericMember(Type type, object target, string memberName, float value)
    {
        PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.CanWrite && TryChangeType(value, property.PropertyType, out object propertyValue))
        {
            property.SetValue(target, propertyValue);
            return;
        }

        FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && TryChangeType(value, field.FieldType, out object fieldValue))
        {
            field.SetValue(target, fieldValue);
        }
    }

    private static bool TryGetVectorMember(Type type, object target, string memberName, out Vector2 value)
    {
        value = default;
        PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.CanRead && TryConvertToVector2(property.GetValue(target), out value))
        {
            return true;
        }

        FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return field != null && TryConvertToVector2(field.GetValue(target), out value);
    }

    private static void SetVectorMember(Type type, object target, string memberName, Vector3 value)
    {
        PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.CanWrite && TryConvertVector(value, property.PropertyType, out object propertyValue))
        {
            property.SetValue(target, propertyValue);
            return;
        }

        FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && TryConvertVector(value, field.FieldType, out object fieldValue))
        {
            field.SetValue(target, fieldValue);
        }
    }

    private static bool TryConvertToFloat(object source, out float value)
    {
        value = 0f;
        if (source == null)
        {
            return false;
        }

        try
        {
            value = Convert.ToSingle(source);
            return true;
        }
        catch (InvalidCastException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryChangeType(float source, Type targetType, out object value)
    {
        value = null;
        if (targetType == typeof(float))
        {
            value = source;
            return true;
        }

        if (targetType == typeof(int))
        {
            value = Mathf.RoundToInt(source);
            return true;
        }

        if (targetType == typeof(double))
        {
            value = (double)source;
            return true;
        }

        return false;
    }

    private static bool TryConvertToVector2(object source, out Vector2 value)
    {
        value = default;
        switch (source)
        {
            case Vector2 vector2:
                value = vector2;
                return true;
            case Vector3 vector3:
                value = vector3;
                return true;
            default:
                return false;
        }
    }

    private static bool TryConvertVector(Vector3 source, Type targetType, out object value)
    {
        value = null;
        if (targetType == typeof(Vector2))
        {
            value = (Vector2)source;
            return true;
        }

        if (targetType == typeof(Vector3))
        {
            value = source;
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawLedgeRaycasts || !avoidLedges)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        DrawLedgeGizmo(1f);
        DrawLedgeGizmo(-1f);
    }

    private void DrawLedgeGizmo(float direction)
    {
        Vector2 origin = Application.isPlaying ? GetLedgeRaycastOrigin(direction) : GetEditorLedgeRaycastOrigin(direction);
        Vector2 end = origin + Vector2.down * Mathf.Max(0.01f, ledgeRaycastDistance);
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(end, 0.04f);
    }

    private Vector2 GetEditorLedgeRaycastOrigin(float direction)
    {
        Collider2D editorCollider = GetComponentInChildren<Collider2D>();
        Bounds bounds = editorCollider != null ? editorCollider.bounds : new Bounds(transform.position, Vector3.one);
        float xOffset = bounds.extents.x + Mathf.Max(0f, ledgeRaycastHorizontalOffset);
        return new Vector2(
            bounds.center.x + Mathf.Sign(direction) * xOffset,
            bounds.min.y + Mathf.Max(0f, ledgeRaycastVerticalOffset)
        );
    }

    private void LogMissingPlayerWarning(string message)
    {
        if (missingPlayerWarningLogged)
        {
            return;
        }

        Debug.LogWarning(message, this);
        missingPlayerWarningLogged = true;
    }
}
