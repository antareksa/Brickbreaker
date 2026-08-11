using UnityEngine;

// "+x damage to all ball hits" -- design doc's Ball-Focused #1.
[CreateAssetMenu(fileName = "BonusBallDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Bonus Ball Damage")]
public class BonusBallDamageEffect : BasePowerUpEffect
{
    public int BonusDamage = 1;

    public override int GetBonusBallDamage() => BonusDamage;

    public override string GetDescription() => $"+{BonusDamage} damage on every ball hit";
}
