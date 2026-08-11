using UnityEngine;

// #11: Balls deal bonus damage that increases with each WALL bounce so far this shot (stacking)
// -- the brick-bounce version of this idea is BounceScalingBonusDamageEffect (#4). Previously
// these two shared one script and one mixed counter, which made them behave identically.
// Stepped and capped for the same reason #4 is: the counter is unbounded over a long shot.
[CreateAssetMenu(fileName = "StackingWallBounceBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Stacking Wall Bounce Bonus Damage")]
public class StackingWallBounceBonusDamageEffect : BasePowerUpEffect
{
    public int BouncesPerStep = 10;
    public int BonusDamagePerStep = 1;
    public int MaxBonusDamage = 10;

    public override int GetBonusDamage(BallHitContext context)
    {
        if (BouncesPerStep <= 0) return 0;

        int steps = context.WallBouncesThisShot / BouncesPerStep;
        return Mathf.Min(BonusDamagePerStep * steps, MaxBonusDamage);
    }

    public override string GetDescription() => $"+{BonusDamagePerStep} damage per {BouncesPerStep} wall bounces this shot (max +{MaxBonusDamage})";
}
