using UnityEngine;

// #4: Balls deal bonus damage that scales with BRICK bounces had this shot -- the wall-bounce
// version of this idea is StackingWallBounceBonusDamageEffect (#11).
// Stepped and capped rather than +1 per bounce -- the counter is unbounded and keeps climbing for
// as long as a ball stays alive, so the raw per-bounce version hit +40 or more late in a shot and,
// multiplied across every ball, cleared the whole board in one go.
[CreateAssetMenu(fileName = "BounceScalingBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Bounce Scaling Bonus Damage")]
public class BounceScalingBonusDamageEffect : BasePowerUpEffect
{
    public int BouncesPerStep = 10;
    public int BonusDamagePerStep = 1;
    public int MaxBonusDamage = 10;

    public override int GetBonusDamage(BallHitContext context)
    {
        if (BouncesPerStep <= 0) return 0;

        int steps = context.BrickBouncesThisShot / BouncesPerStep;
        return Mathf.Min(BonusDamagePerStep * steps, MaxBonusDamage);
    }

    public override string GetDescription() => $"+{BonusDamagePerStep} damage per {BouncesPerStep} brick bounces this shot (max +{MaxBonusDamage})";
}
