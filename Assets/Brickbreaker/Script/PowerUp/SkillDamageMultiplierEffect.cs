using UnityEngine;

// #19: Skill effect deals double damage to the bricks it hits.
[CreateAssetMenu(fileName = "SkillDamageMultiplierEffect", menuName = "Brickbreaker/PowerUp Effect/Skill Damage Multiplier")]
public class SkillDamageMultiplierEffect : BasePowerUpEffect
{
    public float Multiplier = 2f;

    public override float GetSkillDamageMultiplier() => Multiplier;
}
