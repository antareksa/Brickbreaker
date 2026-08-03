using UnityEngine;

// #26: +x chance to gain 2x Coin this wave.
[CreateAssetMenu(fileName = "DoubleCoinChanceEffect", menuName = "Brickbreaker/PowerUp Effect/Double Coin Chance")]
public class DoubleCoinChanceEffect : BasePowerUpEffect
{
    [Range(0f, 1f)] public float Chance = 0.1f;

    public override float GetDoubleCoinChance() => Chance;
}
