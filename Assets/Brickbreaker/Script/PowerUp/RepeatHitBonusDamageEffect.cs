using UnityEngine;

// #8: Balls deal bonus damage on re-hitting the same brick within one shot, capped at +5.
// Counted per (ball, brick) via BallHitContext.RepeatHitsOnBrick -- only the SAME ball coming
// back around to the SAME brick earns this, so another ball's hits (or spread/skill damage on
// that brick) don't feed it.
[CreateAssetMenu(fileName = "RepeatHitBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Repeat Hit Bonus Damage")]
public class RepeatHitBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamagePerRepeatHit = 1;
    public int MaxBonusDamage = 5;

    public override int GetBonusDamage(BallHitContext context)
    {
        return Mathf.Min(BonusDamagePerRepeatHit * context.RepeatHitsOnBrick, MaxBonusDamage);
    }

    public override string GetDescription() => $"+{BonusDamagePerRepeatHit} damage each time the same ball re-hits the same brick (max +{MaxBonusDamage})";
}
