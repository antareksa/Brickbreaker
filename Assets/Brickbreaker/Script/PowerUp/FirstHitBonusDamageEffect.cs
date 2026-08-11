using UnityEngine;

// #7: Balls deal bonus damage to the first brick they hit each shot. Kept at +1 because this
// pays out once PER BALL -- at 100 balls that's already +100 per shot, so the number here reads
// far smaller than what it's actually worth.
[CreateAssetMenu(fileName = "FirstHitBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/First Hit Bonus Damage")]
public class FirstHitBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamage = 1;

    public override int GetBonusDamage(BallHitContext context)
    {
        return context.IsFirstHitThisShot ? BonusDamage : 0;
    }

    public override string GetDescription() => $"+{BonusDamage} damage per ball, on the first brick it hits each shot";
}
