using UnityEngine;

// #10: Balls bouncing off side walls (left/right) deal bonus damage on their next hit.
[CreateAssetMenu(fileName = "SideWallBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Side Wall Bonus Damage")]
public class SideWallBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamage = 2;

    public override int GetBonusDamage(BallHitContext context)
    {
        return context.SideWallBounceSinceLastHit ? BonusDamage : 0;
    }
}
