using UnityEngine;

// #5: Balls that hit a wall before any brick deal bonus damage on their next hit.
[CreateAssetMenu(fileName = "WallBounceFirstHitBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Wall Bounce First Hit Bonus Damage")]
public class WallBounceFirstHitBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamage = 2;

    public override int GetBonusDamage(BallHitContext context)
    {
        return context.HitWallBeforeAnyBrick ? BonusDamage : 0;
    }

    public override string GetDescription() => $"+{BonusDamage} damage if the ball hit a wall before any brick";
}
