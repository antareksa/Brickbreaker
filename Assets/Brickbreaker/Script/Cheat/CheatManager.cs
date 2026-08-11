using System.Collections.Generic;
using UnityEngine;

// Debug-only cheat actions for testing -- not part of normal gameplay flow.
public class CheatManager : MonoBehaviour
{
    public void AddGold(int amount)
    {
        GameManager.Instance.AddCoin(amount);
    }

    public void AddCoinShop(int amount)
    {
        GameManager.Instance.AddCoinShop(amount);
    }

    // Skips the normal hit-by-hit charge -- fills the meter to whatever the current level needs,
    // then activates through the usual path so boss-routing/reset behavior stays identical to a
    // real activation.
    public void ActivateSkill()
    {
        SkillManager skillManager = GameManager.Instance.SkillManager;
        skillManager.AddSkillPoint(skillManager.GetSkillPointNeeded());
        skillManager.TryActivateSkill();
    }

    public void ForceRestart()
    {
        GameManager.Instance.BrickManager.RestartGame();
    }

    // Rebuilds the board at the given wave's difficulty, leaving PowerUps/Consumables/enhances/
    // balls alone so a test setup can be assembled in any order. Negatives are bumped to 1 rather
    // than passed through -- BrickConfig's per-wave curves aren't defined below 0.
    public void JumpToWave(int wave)
    {
        if (wave < 0) wave = 1;
        GameManager.Instance.BrickManager.CheatJumpToWave(wave);
    }

    // Routed through BrickManager's normal hit-queue/Shop flow -- not a direct BossManager call
    // -- so cheat-testing a phase kill still opens the Shop the same way a real one would.
    public void AttackBoss(int hitCount)
    {
        GameManager.Instance.BrickManager.CheatAttackBoss(hitCount);
    }

    // Equips directly, bypassing the Shop -- fails silently (no-op) if slots are already full.
    public void AddPowerUp(BasePowerUp powerUp)
    {
        if (powerUp == null) return;
        PowerUpManager.Instance.TryEquip(powerUp);
    }

    // Adds directly, bypassing the Shop -- fails silently (no-op) if slots are already full.
    public void AddConsumable(BaseConsumable consumable)
    {
        if (consumable == null) return;
        ConsumableManager.Instance.TryAdd(consumable);
    }

    // Routed through the normal Aiming-only gate -- fails silently (no-op) if not currently
    // Aiming or the consumable isn't actually held.
    public void UseConsumable(BaseConsumable consumable)
    {
        if (consumable == null) return;
        ConsumableManager.Instance.TryUse(consumable);
    }

    // Roster lookups are 1-based so the numbers typed into the cheat panel line up with how the
    // rosters are listed/documented -- out-of-range input is a silent no-op rather than a throw.
    public BasePowerUp GetPowerUpByNumber(int number)
    {
        List<BasePowerUp> roster = PowerUpManager.Instance.Roster;
        return number >= 1 && number <= roster.Count ? roster[number - 1] : null;
    }

    public BaseConsumable GetConsumableByNumber(int number)
    {
        List<BaseConsumable> roster = ConsumableManager.Instance.Roster;
        return number >= 1 && number <= roster.Count ? roster[number - 1] : null;
    }

    public void AddPowerUpByNumber(int number)
    {
        AddPowerUp(GetPowerUpByNumber(number));
    }

    public void AddConsumableByNumber(int number)
    {
        AddConsumable(GetConsumableByNumber(number));
    }

    public void UseConsumableByNumber(int number)
    {
        UseConsumable(GetConsumableByNumber(number));
    }

    // Skips the pack purchase/reveal flow entirely and just levels the (type, axis) directly --
    // fails silently (no-op) if already maxed or the axis doesn't apply to this type (e.g.
    // Range on Row/Column/Cross).
    public void UpgradeBallEnhance(BallEnhanceType type, BallEnhanceAxis axis)
    {
        BallEnhanceManager.Instance.TryUpgrade(type, axis);
    }
}
