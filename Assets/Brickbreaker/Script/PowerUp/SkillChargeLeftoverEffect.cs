using UnityEngine;

// #18: Skill charge keeps a flat leftover amount instead of resetting to 0 on activation.
[CreateAssetMenu(fileName = "SkillChargeLeftoverEffect", menuName = "Brickbreaker/PowerUp Effect/Skill Charge Leftover")]
public class SkillChargeLeftoverEffect : BasePowerUpEffect
{
    public float LeftoverAmount = 10f;

    public override float GetSkillChargeLeftover() => LeftoverAmount;
}
