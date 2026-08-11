using UnityEngine;

// One-time effect resolved immediately when a Consumable is used -- mirrors BaseSkillEffect's
// Activate() shape, not BasePowerUpEffect's passive-hook shape, since a Consumable fires once and
// is gone rather than staying equipped for the rest of the run.
public abstract class BaseConsumableEffect : ScriptableObject
{
    public abstract void Use();

    // Shop/HUD text built from this effect's own tuned values, so the numbers shown can never
    // drift from the numbers actually in play. Empty means "no generated text" -- BaseConsumable
    // then falls back to its hand-typed Description.
    public virtual string GetDescription() => string.Empty;
}
