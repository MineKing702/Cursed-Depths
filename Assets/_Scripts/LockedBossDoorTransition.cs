using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class LockedBossDoorTransition : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Boss Battle";
    [SerializeField] private string targetSpawnId = "BossBattleSpawnFromBossDoor";
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && triggerOnce)
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null || !inventory.HasBossKey)
        {
            Debug.Log("Boss door is locked. You need the boss key.", this);
            return;
        }

        hasTriggered = true;
        SceneTransitionManager.Instance.TransitionToScene(targetSceneName, targetSpawnId, player);
    }
}
