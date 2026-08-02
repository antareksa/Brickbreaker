using UnityEngine;

// #3: Balls do x3 damage but -20% per bounce, down to a minimum of 50%. Decay is multiplicative
// (each bounce multiplies the current value by 1 - DecayPerBounce), not a flat subtraction.
[CreateAssetMenu(fileName = "BounceDecayDamageMultiplierEffect", menuName = "Brickbreaker/PowerUp Effect/Bounce Decay Damage Multiplier")]
public class BounceDecayDamageMultiplierEffect : BasePowerUpEffect
{
    public float BaseMultiplier = 3f;
    [Range(0f, 1f)] public float DecayPerBounce = 0.2f;
    public float MinMultiplier = 0.5f;

    public override float GetDamageMultiplier(BallHitContext context)
    {
        float multiplier = BaseMultiplier * Mathf.Pow(1f - DecayPerBounce, context.BouncesThisShot);
        return Mathf.Max(multiplier, MinMultiplier);
    }
}
