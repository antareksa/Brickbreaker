using UnityEngine;

// #28: Gain 1 flat HP whenever Skill triggers.
[CreateAssetMenu(fileName = "BonusHpOnSkillTriggerEffect", menuName = "Brickbreaker/PowerUp Effect/Bonus HP On Skill Trigger")]
public class BonusHpOnSkillTriggerEffect : BasePowerUpEffect
{
    public int BonusHp = 1;

    public override int GetBonusHpOnSkillTrigger() => BonusHp;

    public override string GetDescription() => $"+{BonusHp} HP whenever the Skill triggers";
}
