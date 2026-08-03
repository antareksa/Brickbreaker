using UnityEngine;

// #21: Skill deals bonus damage equal to the number of waves survived so far.
[CreateAssetMenu(fileName = "SkillBonusByWavesSurvivedEffect", menuName = "Brickbreaker/PowerUp Effect/Skill Bonus By Waves Survived")]
public class SkillBonusByWavesSurvivedEffect : BasePowerUpEffect
{
    public float DamagePerWave = 0.5f;

    public override int GetBonusSkillDamage()
    {
        int wave = GameManager.Instance.GetWave();
        return Mathf.FloorToInt(DamagePerWave * wave);
    }
}
