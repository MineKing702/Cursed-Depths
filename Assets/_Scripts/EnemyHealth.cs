using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;

    [Header("Death")]
    [SerializeField] private float destroyDelay = 0.75f;
    [SerializeField] private bool disableCollidersOnDeath = true;

    private int currentHealth;
    private bool isDead;
    private Animator enemyAnimator;
    private Collider2D[] enemyColliders;

    private void Awake()
    {
        enemyAnimator = GetComponent<Animator>();
        enemyColliders = GetComponentsInChildren<Collider2D>();

        maxHealth = Mathf.Max(1, maxHealth);
        destroyDelay = Mathf.Max(0f, destroyDelay);

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        damage = Mathf.Max(0, damage);
        if (damage == 0)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (enemyAnimator != null && HasAnimatorParameter("Hurt"))
        {
            enemyAnimator.SetTrigger("Hurt");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (enemyAnimator != null)
        {
            if (HasAnimatorParameter("Death"))
            {
                enemyAnimator.SetTrigger("Death");
            }
            else if (HasAnimatorParameter("Die"))
            {
                enemyAnimator.SetTrigger("Die");
            }
        }

        if (disableCollidersOnDeath)
        {
            foreach (Collider2D enemyCollider in enemyColliders)
            {
                enemyCollider.enabled = false;
            }
        }

        StartCoroutine(DestroyAfterDelayRoutine());
    }

    private IEnumerator DestroyAfterDelayRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private bool HasAnimatorParameter(string parameterName)
    {
        if (enemyAnimator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in enemyAnimator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
}
