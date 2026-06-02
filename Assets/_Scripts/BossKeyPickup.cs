using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class BossKeyPickup : MonoBehaviour
{
    private bool collected;

    private void Reset()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = player.gameObject.AddComponent<PlayerInventory>();
        }

        inventory.AddBossKey();
        collected = true;
        Debug.Log("Picked up Boss Key", this);
        gameObject.SetActive(false);
    }
}
