using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Divergent Slice",
    menuName = "Cursed Depths/Abilities/Divergent Slice"
)]
public sealed class DivergentSliceAbility : AbilityDefinition
{
    [SerializeField] private float followUpDelay = 0.5f;
    [SerializeField] private float followUpDamageMultiplier = 0.5f;
    [SerializeField] private float followUpRangeMultiplier = 0.75f;
    [SerializeField] private GameObject blueFlareVfxPrefab;
    [SerializeField] private Vector2 vfxOffset = Vector2.zero;

    public override IEnumerator Execute(AbilityContext context)
    {
        yield return context.PerformAnimatedBaseAttack();

        yield return new WaitForSeconds(followUpDelay);

        if (context.Player == null || context.Player.IsDead)
        {
            yield break;
        }

        Vector2 center = context.Player.GetAbilityAttackCenter(followUpRangeMultiplier);
        context.SpawnVfx(blueFlareVfxPrefab, center + vfxOffset);
        context.PerformMeleeHit(followUpDamageMultiplier, followUpRangeMultiplier);
    }
}
