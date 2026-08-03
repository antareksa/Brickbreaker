using UnityEngine;

// #22: Skill deals bonus damage to bricks in the row closest to the bottom.
[CreateAssetMenu(fileName = "SkillBonusBottomRowEffect", menuName = "Brickbreaker/PowerUp Effect/Skill Bonus Bottom Row")]
public class SkillBonusBottomRowEffect : BasePowerUpEffect
{
    public int BonusDamage = 8;

    public override int GetBonusSkillDamageForBrick(BrickController brick)
    {
        if (brick == null) return 0;

        int bottomRow = GameManager.Instance.BrickManager.BottomRow;
        return brick.GridPosition.y == bottomRow ? BonusDamage : 0;
    }
}
