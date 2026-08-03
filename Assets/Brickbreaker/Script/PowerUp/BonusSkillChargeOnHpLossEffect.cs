using UnityEngine;

// #29: Losing HP grants bonus charge to the Skill bar.
[CreateAssetMenu(fileName = "BonusSkillChargeOnHpLossEffect", menuName = "Brickbreaker/PowerUp Effect/Bonus Skill Charge On HP Loss")]
public class BonusSkillChargeOnHpLossEffect : BasePowerUpEffect
{
    public float BonusCharge = 20f;

    public override float GetBonusSkillChargeOnHpLoss() => BonusCharge;
}
