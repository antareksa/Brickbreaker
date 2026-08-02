using UnityEngine;

// #9: Ball that hits a brick on the last row gets extra bonus damage.
[CreateAssetMenu(fileName = "LastRowBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Last Row Bonus Damage")]
public class LastRowBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamage = 2;

    public override int GetBonusDamage(BallHitContext context)
    {
        if (context.HitBrick == null) return 0;

        int bottomRow = GameManager.Instance.BrickManager.BottomRow;
        return context.HitBrick.GridPosition.y == bottomRow ? BonusDamage : 0;
    }
}
