using System.Collections;
using UnityEngine;

public sealed class PlayerAbilityController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private AbilityDefinition equippedAbility;
    [SerializeField] private AbilitySlot equippedSlot = AbilitySlot.Slot1;
    [SerializeField] private Transform vfxParent;

    private Coroutine activeAbilityRoutine;
    private float nextReadyTime;

    public AbilityDefinition EquippedAbility => equippedAbility;
    public AbilitySlot EquippedSlot => equippedSlot;
    public float CooldownRemaining => Mathf.Max(0f, nextReadyTime - Time.time);
    public bool IsAbilityRunning => activeAbilityRoutine != null;
    public Transform VfxParent => vfxParent;

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (equippedAbility == null)
        {
            return;
        }

        if (Input.GetKeyDown(GetKeyForSlot(equippedSlot)))
        {
            TryUseEquippedAbility();
        }
    }

    public void EquipAbility(AbilityDefinition ability, AbilitySlot slot)
    {
        equippedAbility = ability;
        equippedSlot = slot;
        nextReadyTime = 0f;
    }

    public void ClearAbility()
    {
        equippedAbility = null;
        nextReadyTime = 0f;

        if (activeAbilityRoutine != null)
        {
            StopCoroutine(activeAbilityRoutine);
            activeAbilityRoutine = null;
        }
    }

    public bool TryUseEquippedAbility()
    {
        if (equippedAbility == null || IsAbilityRunning || CooldownRemaining > 0f)
        {
            return false;
        }

        if (player == null || player.IsDead)
        {
            return false;
        }

        nextReadyTime = Time.time + Mathf.Max(0f, equippedAbility.Cooldown);
        activeAbilityRoutine = StartCoroutine(ExecuteAbilityRoutine(equippedAbility));
        return true;
    }

    private IEnumerator ExecuteAbilityRoutine(AbilityDefinition ability)
    {
        AbilityContext context = new AbilityContext(player, this);
        yield return ability.Execute(context);
        activeAbilityRoutine = null;
    }

    private static KeyCode GetKeyForSlot(AbilitySlot slot)
    {
        switch (slot)
        {
            case AbilitySlot.Slot1:
                return KeyCode.R;
            case AbilitySlot.Slot2:
                return KeyCode.E;
            case AbilitySlot.Slot3:
                return KeyCode.Q;
            default:
                return KeyCode.R;
        }
    }
}
