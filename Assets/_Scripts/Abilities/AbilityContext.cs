using UnityEngine;

public sealed class AbilityContext
{
    public AbilityContext(PlayerController player, PlayerAbilityController abilityController)
    {
        Player = player;
        AbilityController = abilityController;
    }

    public PlayerController Player { get; }
    public PlayerAbilityController AbilityController { get; }
    public Transform PlayerTransform => Player != null ? Player.transform : null;
    public Vector2 FacingDirection => Player != null ? Player.GetFacingDirection() : Vector2.zero;

    public int PerformMeleeHit(float damageMultiplier, float rangeMultiplier)
    {
        if (Player == null)
        {
            return 0;
        }

        return Player.PerformAbilityMeleeHit(damageMultiplier, rangeMultiplier);
    }

    public void SpawnVfx(GameObject prefab, Vector2 position)
    {
        if (prefab == null)
        {
            return;
        }

        Transform parent = AbilityController != null ? AbilityController.VfxParent : null;
        GameObject spawnedVfx = Object.Instantiate(prefab, position, Quaternion.identity, parent);
        Object.Destroy(spawnedVfx, 2f);
    }
}
