using UnityEngine;

// #20: Skill deals bonus damage when total bricks on field is above a threshold.
[CreateAssetMenu(fileName = "SkillBonusAboveBrickThresholdEffect", menuName = "Brickbreaker/PowerUp Effect/Skill Bonus Above Brick Threshold")]
public class SkillBonusAboveBrickThresholdEffect : BasePowerUpEffect
{
    public int BrickCountThreshold = 10;
    public int BonusDamage = 8;

    public override int GetBonusSkillDamage()
    {
        int brickCount = GameManager.Instance.BrickManager.GetBrickCount();
        return brickCount > BrickCountThreshold ? BonusDamage : 0;
    }
}
