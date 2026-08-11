using UnityEngine;

// #2: Every Nth ball fired deals bonus damage -- once a ball qualifies, the bonus applies to
// every brick it hits for the rest of that shot, not just its first.
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

    public override string GetDescription() => $"Every {GetOrdinal(N)} ball fired deals +{BonusDamage} damage for the whole shot";

    // Hardcoding "th" produced "3th" in the shop. 11/12/13 are the exception to the last-digit
    // rule (11th, not 11st).
    private static string GetOrdinal(int value)
    {
        int lastTwoDigits = value % 100;
        if (lastTwoDigits >= 11 && lastTwoDigits <= 13) return value + "th";

        switch (value % 10)
        {
            case 1: return value + "st";
            case 2: return value + "nd";
            case 3: return value + "rd";
            default: return value + "th";
        }
    }
}
