using UnityEngine;

// #12: Gain a flat bonus amount of Coin immediately.
[CreateAssetMenu(fileName = "FlatCoinConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Flat Coin")]
public class FlatCoinConsumableEffect : BaseConsumableEffect
{
    public int Amount = 50;

    public override void Use()
    {
        GameManager.Instance.AddCoin(Amount);
    }

    public override string GetDescription() => $"Gain {Amount} Coin";
}
