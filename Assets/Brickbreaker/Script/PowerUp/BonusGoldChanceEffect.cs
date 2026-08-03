using UnityEngine;

// #24: +x chance a Basic brick spawns as Gold instead.
[CreateAssetMenu(fileName = "BonusGoldChanceEffect", menuName = "Brickbreaker/PowerUp Effect/Bonus Gold Chance")]
public class BonusGoldChanceEffect : BasePowerUpEffect
{
    [Range(0f, 1f)] public float BonusChance = 0.05f;

    public override float GetBonusGoldChance() => BonusChance;
}
