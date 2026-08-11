using UnityEngine;

// #30: Gain 1 flat HP for every 4 waves survived without losing HP.
[CreateAssetMenu(fileName = "BonusHpForWavesSurvivedEffect", menuName = "Brickbreaker/PowerUp Effect/Bonus HP For Waves Survived")]
public class BonusHpForWavesSurvivedEffect : BasePowerUpEffect
{
    public int WaveInterval = 4;
    public int BonusHp = 1;

    public override int GetBonusHpForWavesSurvived(int wavesSinceLastHpLoss)
    {
        if (WaveInterval <= 0) return 0;
        return (wavesSinceLastHpLoss > 0 && wavesSinceLastHpLoss % WaveInterval == 0) ? BonusHp : 0;
    }

    public override string GetDescription() => $"+{BonusHp} HP every {WaveInterval} waves survived without losing HP";
}
