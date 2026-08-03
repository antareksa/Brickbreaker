using UnityEngine;

// One-time effect resolved immediately when a Consumable is used -- mirrors BaseSkillEffect's
// Activate() shape, not BasePowerUpEffect's passive-hook shape, since a Consumable fires once and
// is gone rather than staying equipped for the rest of the run.
public abstract class BaseConsumableEffect : ScriptableObject
{
    public abstract void Use();
}
