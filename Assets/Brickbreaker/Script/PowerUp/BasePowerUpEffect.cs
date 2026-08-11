using UnityEngine;

// The actual behavior for a PowerUp -- referenced from a BasePowerUp asset's Effect field.
// Concrete effects (one .cs + one or more assets per effect type) override whichever hook is
// relevant to them; everything else defaults to a no-op. PowerUpManager only aggregates across
// equipped PowerUps -- it never contains effect-specific logic itself.
public abstract class BasePowerUpEffect : ScriptableObject
{
    // Shop/HUD text built from this effect's own tuned values, so the numbers shown can never
    // drift from the numbers actually in play. Empty means "no generated text" -- BasePowerUp
    // then falls back to its hand-typed Description.
    public virtual string GetDescription() => string.Empty;

    public virtual int GetBonusBallDamage() => 0;

    // Additive bonus based on this specific hit's context (bounces this shot, fire order,
    // whether it bounced off a wall before hitting anything, etc.).
    public virtual int GetBonusDamage(BallHitContext context) => 0;

    // Multiplier applied to damage before additive bonuses -- 1 = no change. Used by effects
    // that scale damage multiplicatively instead of adding a flat/scaling bonus.
    public virtual float GetDamageMultiplier(BallHitContext context) => 1f;

    // Multiplier applied to skill charge gained per hit -- 1 = no change.
    public virtual float GetSkillChargeMultiplier() => 1f;

    // -- Skill-focused --

    // Flat bonus added to the skill's CurrentDamage before the multiplier below, once per
    // activation (not per brick hit) -- for effects like flat bonus damage, or damage that scales
    // with wave/brick-count conditions the effect checks itself.
    public virtual int GetBonusSkillDamage() => 0;

    // Extra times the skill's Activate() fires per activation, on top of the normal one.
    public virtual int GetBonusSkillTriggers() => 0;

    // Flat charge kept in the meter after an activation, instead of resetting to 0.
    public virtual float GetSkillChargeLeftover() => 0f;

    // Multiplier applied to the skill's CurrentDamage (after the flat bonus above) -- 1 = no change.
    public virtual float GetSkillDamageMultiplier() => 1f;

    // Additive bonus based on the specific brick the skill is hitting (e.g. its row) -- applied
    // per brick, unlike GetBonusSkillDamage which applies once per activation.
    public virtual int GetBonusSkillDamageForBrick(BrickController brick) => 0;

    // -- Economy-focused --

    // Multiplier applied to the total coin value collected in CollectAllCoins -- 1 = no change.
    public virtual float GetCoinValueMultiplier() => 1f;

    // Extra chance (0-1) for a non-designated brick to spawn as Gold instead of Basic.
    public virtual float GetBonusGoldChance() => 0f;

    // Flat coin value added per brick destroyed, on top of its normal Silver/Gold value.
    public virtual int GetBonusCoinPerBrick() => 0;

    // Chance (0-1) to double the total coin value collected in CollectAllCoins.
    public virtual float GetDoubleCoinChance() => 0f;

    // -- HP-focused --

    // Chance (0-1) to block/negate an HP loss entirely -- the bricks reaching bottom still get
    // cleared, but no chance/HP is spent.
    public virtual float GetBlockHpLossChance() => 0f;

    // Flat HP gained whenever the skill activates.
    public virtual int GetBonusHpOnSkillTrigger() => 0;

    // Flat skill charge gained whenever the player actually loses HP (not when blocked).
    public virtual float GetBonusSkillChargeOnHpLoss() => 0f;

    // Flat HP gained when wavesSinceLastHpLoss lands on this effect's interval -- called once per
    // wave advance with the running count (reset to 0 whenever HP is actually lost).
    public virtual int GetBonusHpForWavesSurvived(int wavesSinceLastHpLoss) => 0;
}
