using System.Collections.Generic;
using UnityEngine;

// Meta-progression: persistent upgrades bought with Token, independent of any single game run.
// Own singleton (not nested under GameManager) since traits conceptually outlive a single
// session -- GameManager resets/restarts per run, traits don't.
// Plain in-memory state for now -- no save/load, no obscuring/encryption yet. Resets every time
// the game starts until that's built.
public class TraitManager : MonoBehaviour
{
    public static TraitManager Instance { get; private set; }

    public List<TraitDefinition> Traits;

    private int _token;
    private readonly Dictionary<TraitType, int> _levels = new Dictionary<TraitType, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int GetToken() => _token;

    public void AddToken(int amount)
    {
        _token += amount;
    }

    public int GetLevel(TraitType type)
    {
        _levels.TryGetValue(type, out int level);
        return level;
    }

    // The trait's current bonus value -- 0 if never upgraded (level 0).
    public float GetTraitValue(TraitType type)
    {
        TraitDefinition definition = FindDefinition(type);
        int level = GetLevel(type);

        if (definition == null || level <= 0) return 0f;

        int index = Mathf.Clamp(level - 1, 0, definition.ValuePerLevel.Length - 1);
        return definition.ValuePerLevel[index];
    }

    public bool CanUpgrade(TraitType type)
    {
        TraitDefinition definition = FindDefinition(type);
        if (definition == null) return false;

        int level = GetLevel(type);
        if (level >= definition.MaxLevel) return false;

        return _token >= definition.CostPerLevel[level];
    }

    public void UpgradeTrait(TraitType type)
    {
        if (!CanUpgrade(type)) return;

        TraitDefinition definition = FindDefinition(type);
        int level = GetLevel(type);

        _token -= definition.CostPerLevel[level];
        _levels[type] = level + 1;
    }

    // Refunds every token spent across all traits (sum of CostPerLevel up to each trait's
    // current level) and zeroes every level back out.
    public void ResetAllTraits()
    {
        foreach (TraitDefinition definition in Traits)
        {
            int level = GetLevel(definition.Type);
            for (int i = 0; i < level; i++)
            {
                _token += definition.CostPerLevel[i];
            }
        }

        _levels.Clear();
    }

    private TraitDefinition FindDefinition(TraitType type)
    {
        foreach (TraitDefinition definition in Traits)
        {
            if (definition.Type == type) return definition;
        }
        return null;
    }
}
