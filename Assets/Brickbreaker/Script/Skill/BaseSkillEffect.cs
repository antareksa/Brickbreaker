using UnityEngine;

// Same shape as BaseBallHitEffect, but for the player's chosen unique skill (triggered by
// GameManager.OnActivatedSkill once skill points fill up) instead of a per-ball hit effect.
// Not wired to anything yet -- this is scaffolding for the skill-selection system to come later.
public abstract class BaseSkillEffect : MonoBehaviour
{
    public string SkillName;
    [TextArea] public string Description;

    // Derived from the current wave, not set manually -- every 10 waves bumps the level by 1
    // (wave 1-10 -> Lvl.1, 11-20 -> Lvl.2, and so on).
    public int CurrentLevel => (GameManager.Instance.GetWave() / 10) + 1;

    // % of the charge meter one ball hit fills in, keyed by level. A graph instead of a formula
    // or hand-authored array because the real per-level values are still being tuned against
    // reference data (only levels 1, 2, 3, 7 are confirmed so far) -- drag keyframes in the
    // Inspector to adjust as more real data comes in.
    public AnimationCurve PercentPerHitByLevel = new AnimationCurve(
        new Keyframe(1, 0.5f),
        new Keyframe(2, 0.2f),
        new Keyframe(3, 0.12f),
        new Keyframe(7, 0.025f),
        new Keyframe(10, 0.0205f));

    public float SkillPointNeeded => 100f / Mathf.Max(PercentPerHitByLevel.Evaluate(CurrentLevel), 0.0001f);

    // Flat per-skill base damage, no longer a per-level array -- level scaling now comes from
    // multiplying by CurrentLevel below, with the SkillDamageBoost trait (persistent
    // meta-upgrade, independent of this run) applied as a percentage bonus on top of the base.
    public int BaseDamage;

    public int CurrentDamage
    {
        get
        {
            float traitPercent = TraitManager.Instance != null
                ? TraitManager.Instance.GetTraitValue(TraitType.SkillDamageBoost)
                : 0f;
            float bonusDamageFromTrait = BaseDamage * (traitPercent / 100f);

            float damage = (BaseDamage + bonusDamageFromTrait) * CurrentLevel;

            if (PowerUpManager.Instance != null)
            {
                damage += PowerUpManager.Instance.GetTotalBonusSkillDamage();
                damage *= PowerUpManager.Instance.GetTotalSkillDamageMultiplier();
            }

            return Mathf.RoundToInt(damage);
        }
    }

    public abstract void Activate();

    protected void PlayVfx(GameObject vfx, Vector3 position)
    {
        if (vfx == null) return;
        VFXManager.Instance.PlayVFX(vfx, position);
    }

    // Subclasses should call this instead of brick.DamageBrick(CurrentDamage) directly -- it
    // folds in per-brick PowerUp bonuses (e.g. extra damage to bottom-row bricks) that CurrentDamage
    // itself can't apply since it doesn't know which brick is being hit.
    protected void DealDamageToBrick(BrickController brick)
    {
        if (brick == null) return;

        int damage = CurrentDamage;
        if (PowerUpManager.Instance != null)
        {
            damage += PowerUpManager.Instance.GetTotalBonusSkillDamageForBrick(brick);
        }

        brick.DamageBrick(damage);
    }
}
