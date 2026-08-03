using UnityEngine;

// #14: Double the Coin earned this wave.
[CreateAssetMenu(fileName = "DoubleCoinThisWaveConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Double Coin This Wave")]
public class DoubleCoinThisWaveConsumableEffect : BaseConsumableEffect
{
    public override void Use()
    {
        GameManager.Instance.CoinManager.DoubleNextCollection();
    }
}
