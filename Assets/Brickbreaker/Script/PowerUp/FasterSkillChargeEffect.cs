using UnityEngine;

// #15: Skill charges faster.
[CreateAssetMenu(fileName = "FasterSkillChargeEffect", menuName = "Brickbreaker/PowerUp Effect/Faster Skill Charge")]
public class FasterSkillChargeEffect : BasePowerUpEffect
{
    [Range(1f, 5f)] public float ChargeMultiplier = 1.5f;

    public override float GetSkillChargeMultiplier() => ChargeMultiplier;
}
