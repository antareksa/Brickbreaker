using System.Collections.Generic;
using UnityEngine;

// Session-scoped (like PowerUpManager/ConsumableManager, unlike TraitManager) -- Ball Enhance is
// bought with Coin Shop at the boss-Shop phase same as PowerUp/Consumable, so levels reset every
// RestartGame rather than persisting across runs.
//
// Per-(type, axis) level, 0-3. Level 0 means "not purchased" -- callers fall back to their own
// hardcoded base value (GetChanceValue/GetRangeValue) or to "never" (GetProcChance), since no
// hit effect currently ever procs twice or hits extra neighbors without this system.
// Levels REPLACE the base value, they don't stack on top of it.
public class BallEnhanceManager : MonoBehaviour
{
    public static BallEnhanceManager Instance { get; private set; }

    public const int MaxLevel = 3;

    // Flat cost to open a whole pack (mirrors Balatro's Booster Pack -- you buy the sealed pack
    // once, then freely pick from whatever it reveals). Not a per-pick cost -- pack pricing is
    // flat for now per the Shop doc, no per-level scaling.
    public int PackBuyCost = 3;

    // Shared across every type -- Chance/ProcCount's whole point is a consistent, easy-to-reason
    // ladder regardless of which ball type you're upgrading. Range uses a flat count/distance
    // instead of a percent, since it's a discrete unlock, not a probability.
    private static readonly float[] ChanceByLevel = { 0.3f, 0.6f, 0.9f };
    private static readonly float[] ProcChanceByLevel = { 0.2f, 0.4f, 0.6f };
    private static readonly int[] RangeByLevel = { 1, 2, 3 };

    private static readonly BallEnhanceType[] EnhanceableTypes =
    {
        BallEnhanceType.Bomb, BallEnhanceType.Fire, BallEnhanceType.Lightning,
        BallEnhanceType.Row, BallEnhanceType.Column, BallEnhanceType.Cross,
    };

    private static readonly BallEnhanceAxis[] AllAxes =
    {
        BallEnhanceAxis.Chance, BallEnhanceAxis.Range, BallEnhanceAxis.ProcCount,
    };

    private readonly Dictionary<(BallEnhanceType, BallEnhanceAxis), int> _levels = new Dictionary<(BallEnhanceType, BallEnhanceAxis), int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Row/Column/Cross have no Range axis -- their effect already spans a full line by
    // definition, so "extending reach" has nothing to modify. Basic has no axes at all.
    public bool HasAxis(BallEnhanceType type, BallEnhanceAxis axis)
    {
        if (type == BallEnhanceType.Basic) return false;

        if (axis == BallEnhanceAxis.Range)
        {
            return type == BallEnhanceType.Bomb || type == BallEnhanceType.Fire || type == BallEnhanceType.Lightning;
        }

        return true;
    }

    public int GetLevel(BallEnhanceType type, BallEnhanceAxis axis)
    {
        _levels.TryGetValue((type, axis), out int level);
        return level;
    }

    public bool IsMaxed(BallEnhanceType type, BallEnhanceAxis axis) => GetLevel(type, axis) >= MaxLevel;

    // Picking an already-owned entry upgrades it rather than stacking a duplicate -- the pool
    // stays at 15 distinct (type, axis) entries, not 45.
    public bool TryUpgrade(BallEnhanceType type, BallEnhanceAxis axis)
    {
        if (!HasAxis(type, axis)) return false;
        if (IsMaxed(type, axis)) return false;

        _levels[(type, axis)] = GetLevel(type, axis) + 1;
        return true;
    }

    // Spends the flat pack price -- called once when a sealed BallEnhancePackOfferPanel is
    // bought, before its 4 options are even rolled/revealed. Picking from the revealed options
    // afterward is free (TryUpgrade), since the pack itself already covers the cost.
    public bool TrySpendForPack()
    {
        if (GameManager.Instance.GetCoinShop() < PackBuyCost) return false;

        GameManager.Instance.TrySpendCoinShop(PackBuyCost);
        return true;
    }

    // baseValue is the hit effect's own hardcoded field -- used unmodified until Level 1 replaces it.
    public float GetChanceValue(BallEnhanceType type, float baseValue)
    {
        int level = GetLevel(type, BallEnhanceAxis.Chance);
        return level > 0 ? ChanceByLevel[level - 1] : baseValue;
    }

    public int GetRangeValue(BallEnhanceType type, int baseValue)
    {
        int level = GetLevel(type, BallEnhanceAxis.Range);
        return level > 0 ? RangeByLevel[level - 1] : baseValue;
    }

    // No base fallback here -- unenhanced means 0% chance to trigger a 2nd time, since no hit
    // effect currently ever double-triggers on its own.
    public float GetProcChance(BallEnhanceType type)
    {
        int level = GetLevel(type, BallEnhanceAxis.ProcCount);
        return level > 0 ? ProcChanceByLevel[level - 1] : 0f;
    }

    // Value at a specific level with no base-value fallback -- for UI listing already-owned
    // (level >= 1) entries, where the ball's own hardcoded base no longer applies.
    public float GetChanceValueAtLevel(int level)
    {
        return level > 0 ? ChanceByLevel[Mathf.Clamp(level - 1, 0, ChanceByLevel.Length - 1)] : 0f;
    }

    public int GetRangeValueAtLevel(int level)
    {
        return level > 0 ? RangeByLevel[Mathf.Clamp(level - 1, 0, RangeByLevel.Length - 1)] : 0;
    }

    public float GetProcChanceAtLevel(int level)
    {
        return level > 0 ? ProcChanceByLevel[Mathf.Clamp(level - 1, 0, ProcChanceByLevel.Length - 1)] : 0f;
    }

    // Every (type, axis) the player currently owns at least one level of.
    public List<(BallEnhanceType Type, BallEnhanceAxis Axis)> GetOwned()
    {
        List<(BallEnhanceType Type, BallEnhanceAxis Axis)> owned = new List<(BallEnhanceType, BallEnhanceAxis)>();

        foreach (BallEnhanceType type in EnhanceableTypes)
        {
            foreach (BallEnhanceAxis axis in AllAxes)
            {
                if (!HasAxis(type, axis)) continue;
                if (GetLevel(type, axis) <= 0) continue;
                owned.Add((type, axis));
            }
        }

        return owned;
    }

    // Plain-English explanation of what an axis actually governs for a given ball type -- mirrors
    // the "what it governs" column from the Ball Enhance design doc's per-type table. Lives here
    // rather than on a panel since both the Shop pick UI and the owned-info UI display it.
    public static string GetAxisDescription(BallEnhanceType type, BallEnhanceAxis axis)
    {
        switch (axis)
        {
            case BallEnhanceAxis.Chance:
                switch (type)
                {
                    case BallEnhanceType.Bomb: return "Chance to re-hit the same brick";
                    case BallEnhanceType.Fire: return "Chance to spread to orthogonal neighbors";
                    case BallEnhanceType.Lightning: return "Chance to spread to diagonal neighbors";
                    case BallEnhanceType.Row: return "Chance to damage the full row";
                    case BallEnhanceType.Column: return "Chance to damage the full column";
                    case BallEnhanceType.Cross: return "Chance to damage the full row + column";
                    default: return "Chance for this ball's effect to trigger";
                }

            case BallEnhanceAxis.Range:
                switch (type)
                {
                    case BallEnhanceType.Bomb: return "Number of random neighboring bricks also damaged";
                    case BallEnhanceType.Fire:
                    case BallEnhanceType.Lightning: return "Tile reach distance of the spread";
                    default: return "Spatial reach of this ball's effect";
                }

            case BallEnhanceAxis.ProcCount:
                return "Chance the effect triggers a 2nd time (capped at 2 total per hit)";

            default:
                return string.Empty;
        }
    }

    // One themed pack: up to 4 (type, axis) picks. Slot 0 is guaranteed to match rolledTheme (a
    // random axis belonging to that type); slots 1-3 are random from whatever's left in the pool
    // (any type, all distinct, maxed entries excluded so a slot never offers something with
    // nothing left to upgrade). Can return fewer than 4 if the pool runs out.
    public List<(BallEnhanceType Type, BallEnhanceAxis Axis)> GenerateThemedPack()
    {
        BallEnhanceType theme = EnhanceableTypes[Random.Range(0, EnhanceableTypes.Length)];

        List<(BallEnhanceType Type, BallEnhanceAxis Axis)> pool = new List<(BallEnhanceType, BallEnhanceAxis)>();
        foreach (BallEnhanceType type in EnhanceableTypes)
        {
            foreach (BallEnhanceAxis axis in AllAxes)
            {
                if (!HasAxis(type, axis)) continue;
                if (IsMaxed(type, axis)) continue;
                pool.Add((type, axis));
            }
        }

        List<(BallEnhanceType Type, BallEnhanceAxis Axis)> result = new List<(BallEnhanceType, BallEnhanceAxis)>();

        List<(BallEnhanceType Type, BallEnhanceAxis Axis)> themedOptions = pool.FindAll(entry => entry.Type == theme);
        if (themedOptions.Count > 0)
        {
            (BallEnhanceType Type, BallEnhanceAxis Axis) slot0 = themedOptions[Random.Range(0, themedOptions.Count)];
            result.Add(slot0);
            pool.Remove(slot0);
        }

        while (result.Count < 4 && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    public void ResetEnhances()
    {
        _levels.Clear();
    }
}
