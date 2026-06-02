using UnityEngine;

public sealed class PlayerInventory : MonoBehaviour
{
    public const string BossKeyItemId = "BossKey";

    [SerializeField] private bool hasBossKey;

    public bool HasBossKey => hasBossKey;

    public void AddBossKey()
    {
        if (hasBossKey)
        {
            return;
        }

        hasBossKey = true;
    }

    public bool HasItem(string itemId)
    {
        return itemId == BossKeyItemId && hasBossKey;
    }
}
