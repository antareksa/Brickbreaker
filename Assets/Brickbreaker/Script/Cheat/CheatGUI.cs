using UnityEngine;

// Immediate-mode debug panel (OnGUI, not the Canvas-based HUD system) -- no prefab/Canvas wiring
// needed, just drop this on any GameObject with a CheatManager reference.
public class CheatGUI : MonoBehaviour
{
    public CheatManager CheatManager;
    public int GoldAmount = 100;
    public int CoinShopAmount = 20;

    // Queued hits, not raw damage -- each deals BrickManager.AttackPowerToBoss, same as a real
    // brick destruction would.
    public int BossAttackHitCount = 10;
    public BasePowerUp TestPowerUp;
    public BaseConsumable TestConsumable;

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 160, 260));

        if (GUILayout.Button($"Add Gold {GoldAmount}"))
        {
            CheatManager.AddGold(GoldAmount);
        }

        if (GUILayout.Button($"Add Coin Shop {CoinShopAmount}"))
        {
            CheatManager.AddCoinShop(CoinShopAmount);
        }

        if (GUILayout.Button("Activate Skill"))
        {
            CheatManager.ActivateSkill();
        }

        if (GUILayout.Button("Force Restart"))
        {
            CheatManager.ForceRestart();
        }

        if (GUILayout.Button($"Attack Boss x{BossAttackHitCount} hits"))
        {
            CheatManager.AttackBoss(BossAttackHitCount);
        }

        if (GUILayout.Button($"Add PowerUp: {(TestPowerUp != null ? TestPowerUp.PowerUpName : "none set")}"))
        {
            CheatManager.AddPowerUp(TestPowerUp);
        }

        if (GUILayout.Button($"Add Consumable: {(TestConsumable != null ? TestConsumable.ConsumableName : "none set")}"))
        {
            CheatManager.AddConsumable(TestConsumable);
        }

        if (GUILayout.Button($"Use Consumable: {(TestConsumable != null ? TestConsumable.ConsumableName : "none set")}"))
        {
            CheatManager.UseConsumable(TestConsumable);
        }

        GUILayout.EndArea();
    }
}
