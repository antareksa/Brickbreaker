using UnityEngine;

// #12: Balls deal bonus damage on odd-numbered bounces (1st, 3rd, 5th...).
[CreateAssetMenu(fileName = "OddBounceBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Odd Bounce Bonus Damage")]
public class OddBounceBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamage = 2;

    public override int GetBonusDamage(BallHitContext context)
    {
        return context.BouncesThisShot % 2 == 1 ? BonusDamage : 0;
    }

    public override string GetDescription() => $"+{BonusDamage} damage on odd-numbered bounces";
}
