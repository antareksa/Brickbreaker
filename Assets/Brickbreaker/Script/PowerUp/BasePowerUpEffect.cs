using UnityEngine;

// The actual behavior for a PowerUp -- referenced from a BasePowerUp asset's Effect field.
// Concrete effects (one .cs + one or more assets per effect type) override whichever hook is
// relevant to them; everything else defaults to a no-op. PowerUpManager only aggregates across
// equipped PowerUps -- it never contains effect-specific logic itself.
public abstract class BasePowerUpEffect : ScriptableObject
{
    public virtual int GetBonusBallDamage() => 0;

    // Additive bonus based on this specific hit's context (bounces this shot, fire order,
    // whether it bounced off a wall before hitting anything, etc.).
    public virtual int GetBonusDamage(BallHitContext context) => 0;

    // Multiplier applied to damage before additive bonuses -- 1 = no change. Used by effects
    // that scale damage multiplicatively instead of adding a flat/scaling bonus.
    public virtual float GetDamageMultiplier(BallHitContext context) => 1f;

    // Multiplier applied to skill charge gained per hit -- 1 = no change.
    public virtual float GetSkillChargeMultiplier() => 1f;
}
