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
}
