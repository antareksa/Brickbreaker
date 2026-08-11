using UnityEngine;

// #16: Skill deals bonus flat damage on top of its normal level-scaling.
[CreateAssetMenu(fileName = "FlatSkillBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Flat Skill Bonus Damage")]
public class FlatSkillBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamage = 10;

    public override int GetBonusSkillDamage() => BonusDamage;

    public override string GetDescription() => $"Skill deals +{BonusDamage} damage";
}
