using UnityEngine;

// #17: Skill triggers 1 extra time before resetting.
[CreateAssetMenu(fileName = "ExtraSkillTriggerEffect", menuName = "Brickbreaker/PowerUp Effect/Extra Skill Trigger")]
public class ExtraSkillTriggerEffect : BasePowerUpEffect
{
    public int ExtraTriggers = 1;

    public override int GetBonusSkillTriggers() => ExtraTriggers;

    public override string GetDescription() => $"Skill triggers {ExtraTriggers} extra time{(ExtraTriggers == 1 ? "" : "s")} per activation";
}
