using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Session-scoped (unlike TraitManager, which persists across runs) -- equipped PowerUps are
// meant to reset every RestartGame, same as everything else built up during a run.
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    public int MaxSlots = 3;

    // Every PowerUp asset that can appear in the game (the Shop draws from this) -- separate
    // from _equippedPowerUps, which is just the currently active subset.
    public List<BasePowerUp> Roster;

    public UnityEvent OnPowerUpsChanged = new UnityEvent();

    private readonly List<BasePowerUp> _equippedPowerUps = new List<BasePowerUp>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IReadOnlyList<BasePowerUp> GetEquipped() => _equippedPowerUps;

    public bool IsFull => _equippedPowerUps.Count >= MaxSlots;

    public bool TryEquip(BasePowerUp powerUp)
    {
        if (IsFull || _equippedPowerUps.Contains(powerUp)) return false;

        _equippedPowerUps.Add(powerUp);
        OnPowerUpsChanged?.Invoke();
        return true;
    }

    public void Unequip(BasePowerUp powerUp)
    {
        _equippedPowerUps.Remove(powerUp);
        OnPowerUpsChanged?.Invoke();
    }

    public void ResetPowerUps()
    {
        _equippedPowerUps.Clear();
        OnPowerUpsChanged?.Invoke();
    }

    // All three just aggregate -- the actual bonus-damage number/logic lives on each equipped
    // PowerUp's own Effect, not here.
    public int GetTotalBonusBallDamage()
    {
        int total = 0;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null)
            {
                total += powerUp.Effect.GetBonusBallDamage();
            }
        }
        return total;
    }

    public int GetTotalBonusDamage(BallHitContext context)
    {
        int total = 0;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null)
            {
                total += powerUp.Effect.GetBonusDamage(context);
            }
        }
        return total;
    }

    public float GetTotalDamageMultiplier(BallHitContext context)
    {
        float multiplier = 1f;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null)
            {
                multiplier *= powerUp.Effect.GetDamageMultiplier(context);
            }
        }
        return multiplier;
    }

    public float GetTotalSkillChargeMultiplier()
    {
        float multiplier = 1f;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null)
            {
                multiplier *= powerUp.Effect.GetSkillChargeMultiplier();
            }
        }
        return multiplier;
    }

    // -- Skill-focused --

    public int GetTotalBonusSkillDamage()
    {
        int total = 0;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetBonusSkillDamage();
        }
        return total;
    }

    public int GetTotalBonusSkillTriggers()
    {
        int total = 0;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetBonusSkillTriggers();
        }
        return total;
    }

    public float GetTotalSkillChargeLeftover()
    {
        float total = 0f;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetSkillChargeLeftover();
        }
        return total;
    }

    public float GetTotalSkillDamageMultiplier()
    {
        float multiplier = 1f;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) multiplier *= powerUp.Effect.GetSkillDamageMultiplier();
        }
        return multiplier;
    }

    public int GetTotalBonusSkillDamageForBrick(BrickController brick)
    {
        int total = 0;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetBonusSkillDamageForBrick(brick);
        }
        return total;
    }

    // -- Economy-focused --

    public float GetTotalCoinValueMultiplier()
    {
        float multiplier = 1f;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) multiplier *= powerUp.Effect.GetCoinValueMultiplier();
        }
        return multiplier;
    }

    public float GetTotalBonusGoldChance()
    {
        float total = 0f;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetBonusGoldChance();
        }
        return total;
    }

    public int GetTotalBonusCoinPerBrick()
    {
        int total = 0;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetBonusCoinPerBrick();
        }
        return total;
    }

    public float GetTotalDoubleCoinChance()
    {
        float total = 0f;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetDoubleCoinChance();
        }
        return total;
    }

    // -- HP-focused --

    public float GetTotalBlockHpLossChance()
    {
        float total = 0f;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetBlockHpLossChance();
        }
        return total;
    }

    public int GetTotalBonusHpOnSkillTrigger()
    {
        int total = 0;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetBonusHpOnSkillTrigger();
        }
        return total;
    }

    public float GetTotalBonusSkillChargeOnHpLoss()
    {
        float total = 0f;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetBonusSkillChargeOnHpLoss();
        }
        return total;
    }

    public int GetTotalBonusHpForWavesSurvived(int wavesSinceLastHpLoss)
    {
        int total = 0;
        foreach (BasePowerUp powerUp in _equippedPowerUps)
        {
            if (powerUp.Effect != null) total += powerUp.Effect.GetBonusHpForWavesSurvived(wavesSinceLastHpLoss);
        }
        return total;
    }
}
