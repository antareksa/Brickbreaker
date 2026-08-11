using UnityEngine;

// #10: +1 flat HP.
[CreateAssetMenu(fileName = "FlatHpConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Flat HP")]
public class FlatHpConsumableEffect : BaseConsumableEffect
{
    public int BonusHp = 1;

    public override void Use()
    {
        GameManager.Instance.SetPlayerChanceCount(GameManager.Instance.GetPlayerChanceCount() + BonusHp);
    }

    public override string GetDescription() => $"Gain {BonusHp} HP";
}
