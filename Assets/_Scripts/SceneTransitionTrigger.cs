using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Area2";
    [SerializeField] private string targetSpawnId = "Area2PlayerSpawn";
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

        hasTriggered = true;
        SceneTransitionManager.Instance.TransitionToScene(targetSceneName, targetSpawnId, player);
    }
}
