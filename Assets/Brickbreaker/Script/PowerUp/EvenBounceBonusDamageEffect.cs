using UnityEngine;

// #13: Balls deal bonus damage on even-numbered bounces (2nd, 4th, 6th...). 0 bounces doesn't
// count -- "even bounce" implies at least one has actually happened.
[CreateAssetMenu(fileName = "EvenBounceBonusDamageEffect", menuName = "Brickbreaker/PowerUp Effect/Even Bounce Bonus Damage")]
public class EvenBounceBonusDamageEffect : BasePowerUpEffect
{
    public int BonusDamage = 2;

    public override int GetBonusDamage(BallHitContext context)
    {
        return context.BouncesThisShot > 0 && context.BouncesThisShot % 2 == 0 ? BonusDamage : 0;
    }
}
