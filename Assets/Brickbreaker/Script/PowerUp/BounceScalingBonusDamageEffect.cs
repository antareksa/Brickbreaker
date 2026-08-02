using UnityEngine;

// #4: Balls deal bonus damage that scales with bounces had this shot (more bounces = more bonus).
[CreateAssetMenu(fileName = "BounceScalingBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Bounce Scaling Bonus Damage")]
public class BounceScalingBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamagePerBounce = 1;

    public override int GetBonusDamage(BallHitContext context)
    {
        return BonusDamagePerBounce * context.BouncesThisShot;
    }
}
