using UnityEngine;

// #6: Balls deal bonus damage that scales with how many bricks remain on the board.
[CreateAssetMenu(fileName = "BricksRemainingBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Bricks Remaining Bonus Damage")]
public class BricksRemainingBonusDamageEffect : BasePowerUpEffect
{
    public float DamagePerBrickRemaining = 0.1f;

    public override int GetBonusDamage(BallHitContext context)
    {
        return Mathf.FloorToInt(DamagePerBrickRemaining * context.BricksRemainingOnField);
    }
}
