using UnityEngine;

// #7: Balls deal bonus damage to the first brick they hit each shot. Flagged in the design doc
// as high risk of being OP -- needs a careful balance pass on BonusDamage before shipping.
[CreateAssetMenu(fileName = "FirstHitBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/First Hit Bonus Damage")]
public class FirstHitBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamage = 3;

    public override int GetBonusDamage(BallHitContext context)
    {
        return context.IsFirstHitThisShot ? BonusDamage : 0;
    }
}
