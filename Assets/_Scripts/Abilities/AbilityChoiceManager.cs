using System.Collections.Generic;
using UnityEngine;

public sealed class AbilityChoiceManager : MonoBehaviour
{
    [SerializeField] private List<AbilityDefinition> abilityPool = new List<AbilityDefinition>();
    [SerializeField] private PlayerAbilityController playerAbilityController;
    [SerializeField] private AbilitySlot slotForChosenAbility = AbilitySlot.Slot1;

    private readonly List<AbilityDefinition> currentChoices = new List<AbilityDefinition>();

    public IReadOnlyList<AbilityDefinition> CurrentChoices => currentChoices;

    private void Awake()
    {
        if (playerAbilityController == null)
        {
            playerAbilityController = FindFirstObjectByType<PlayerAbilityController>();
        }
    }

    public List<AbilityDefinition> GenerateChoices(int count = 3)
    {
        currentChoices.Clear();

        List<AbilityDefinition> candidates = new List<AbilityDefinition>();
        foreach (AbilityDefinition ability in abilityPool)
        {
            if (ability != null && !candidates.Contains(ability))
            {
                candidates.Add(ability);
            }
        }

        int selectionCount = Mathf.Clamp(count, 0, candidates.Count);
        for (int i = 0; i < selectionCount; i++)
        {
            int randomIndex = Random.Range(i, candidates.Count);
            AbilityDefinition tmp = candidates[i];
            candidates[i] = candidates[randomIndex];
            candidates[randomIndex] = tmp;
            currentChoices.Add(candidates[i]);
        }

        if (currentChoices.Count == 0)
        {
            Debug.Log("No ability choices generated (ability pool empty).", this);
        }
        else
        {
            Debug.Log($"Generated {currentChoices.Count} ability choice(s): {string.Join(", ", currentChoices.ConvertAll(a => a.DisplayName))}", this);
        }

        return new List<AbilityDefinition>(currentChoices);
    }

    public void ChooseAbility(AbilityDefinition ability)
    {
        if (ability == null)
        {
            Debug.LogWarning("ChooseAbility called with null ability.", this);
            return;
        }

        if (playerAbilityController == null)
        {
            Debug.LogWarning("No PlayerAbilityController assigned; cannot equip ability.", this);
            return;
        }

        playerAbilityController.EquipAbility(ability, slotForChosenAbility);
        Debug.Log($"Equipped ability '{ability.DisplayName}' to {slotForChosenAbility}.", this);
    }

    public void ChooseAbility(int index)
    {
        if (index < 0 || index >= currentChoices.Count)
        {
            Debug.LogWarning($"ChooseAbility index {index} is out of range.", this);
            return;
        }

        ChooseAbility(currentChoices[index]);
    }
}
