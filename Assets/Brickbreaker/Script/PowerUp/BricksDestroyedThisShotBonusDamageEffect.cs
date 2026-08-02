using UnityEngine;

// #14: Balls deal bonus damage for every brick already destroyed this shot (stacking, resets
// each shot). Design doc left the cap undecided -- MaxBonusDamage is a placeholder, tune once
// that's confirmed.
[CreateAssetMenu(fileName = "BricksDestroyedThisShotBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Bricks Destroyed This Shot Bonus Damage")]
public class BricksDestroyedThisShotBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamagePerBrickDestroyed = 1;
    public int MaxBonusDamage = 10;

    public override int GetBonusDamage(BallHitContext context)
    {
        return Mathf.Min(BonusDamagePerBrickDestroyed * context.BricksDestroyedThisShot, MaxBonusDamage);
    }
}
