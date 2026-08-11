using System.Collections.Generic;
using UnityEngine;

// Meta-progression: persistent upgrades bought with Token, independent of any single game run.
// Own singleton (not nested under GameManager) since traits conceptually outlive a single
// session -- GameManager resets/restarts per run, traits don't.
// Persisted via PlayerPrefs (token + per-trait level) -- plain values, no obscuring/encryption
// yet, so it's trivially editable by the player for now.
public class TraitManager : MonoBehaviour
{
    public static TraitManager Instance { get; private set; }

    public List<TraitDefinition> Traits;

    private const string TokenKey = "Trait_Token";
    private const string LevelKeyPrefix = "Trait_Level_";

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
        Load();
    }

    public int GetToken() => _token;

    public void AddToken(int amount)
    {
        _token += amount;
        Save();
    }

    // Keyed by the enum's int value, not its name -- reordering TraitType would break saves
    // either way, but ints keep the keys short and match how Unity serializes the enum itself.
    private void Load()
    {
        _token = PlayerPrefs.GetInt(TokenKey, 0);

        _levels.Clear();
        foreach (TraitDefinition definition in Traits)
        {
            int level = PlayerPrefs.GetInt(LevelKeyPrefix + (int)definition.Type, 0);
            if (level > 0) _levels[definition.Type] = level;
        }
    }

    private void Save()
    {
        PlayerPrefs.SetInt(TokenKey, _token);

        foreach (TraitDefinition definition in Traits)
        {
            PlayerPrefs.SetInt(LevelKeyPrefix + (int)definition.Type, GetLevel(definition.Type));
        }

        PlayerPrefs.Save();
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
        Save();
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
        Save();
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
