using UnityEngine;

// #27: +x chance to block/negate an HP loss.
[CreateAssetMenu(fileName = "BlockHpLossChanceEffect", menuName = "Brickbreaker/PowerUp Effect/Block HP Loss Chance")]
public class BlockHpLossChanceEffect : BasePowerUpEffect
{
    [Range(0f, 1f)] public float Chance = 0.2f;

    public override float GetBlockHpLossChance() => Chance;
}
