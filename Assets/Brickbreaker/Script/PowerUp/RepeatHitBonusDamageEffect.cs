using UnityEngine;

// #8: Balls deal bonus damage on re-hitting the same brick within one shot, capped at +5.
// HitBrick.HitsThisShot reflects hits BEFORE this one (DamageBrick, which increments it, runs
// after this is evaluated) -- so it's already exactly the "repeat hit count" needed here.
[CreateAssetMenu(fileName = "RepeatHitBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Repeat Hit Bonus Damage")]
public class RepeatHitBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamagePerRepeatHit = 1;
    public int MaxBonusDamage = 5;

    public override int GetBonusDamage(BallHitContext context)
    {
        if (context.HitBrick == null) return 0;

        int repeatHits = context.HitBrick.HitsThisShot;
        return Mathf.Min(BonusDamagePerRepeatHit * repeatHits, MaxBonusDamage);
    }
}
