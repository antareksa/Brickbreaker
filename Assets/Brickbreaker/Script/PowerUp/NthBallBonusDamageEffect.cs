using UnityEngine;

// #2: Every Nth ball fired deals bonus/guaranteed-crit damage.
[CreateAssetMenu(fileName = "NthBallBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Nth Ball Bonus Damage")]
public class NthBallBonusDamageEffect : BasePowerUpEffect
{
    public int N = 3;
    public int BonusDamage = 2;

    public override int GetBonusDamage(BallHitContext context)
    {
        if (N <= 0) return 0;
        return (context.FireIndexThisShot + 1) % N == 0 ? BonusDamage : 0;
    }
}
