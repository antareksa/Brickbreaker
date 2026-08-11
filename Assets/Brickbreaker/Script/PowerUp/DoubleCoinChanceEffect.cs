using UnityEngine;

// #26: +x chance to gain 2x Coin this wave.
[CreateAssetMenu(fileName = "DoubleCoinChanceEffect", menuName = "Brickbreaker/PowerUp Effect/Double Coin Chance")]
public class DoubleCoinChanceEffect : BasePowerUpEffect
{
    [Range(0f, 1f)] public float Chance = 0.1f;

    public override float GetDoubleCoinChance() => Chance;

    public override string GetDescription() => $"{Chance * 100f:F0}% chance to double the Coin collected each wave";
}
