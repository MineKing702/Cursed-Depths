using System.Collections;
using UnityEngine;

public abstract class AbilityDefinition : ScriptableObject
{
    [SerializeField] private string abilityId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private AbilitySlot defaultSlot = AbilitySlot.Slot1;

    public string AbilityId => abilityId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public float Cooldown => cooldown;
    public AbilitySlot DefaultSlot => defaultSlot;

    public abstract IEnumerator Execute(AbilityContext context);
}
