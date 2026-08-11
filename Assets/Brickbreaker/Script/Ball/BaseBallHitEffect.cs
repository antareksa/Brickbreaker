using UnityEngine;

// Attached alongside BallControllerV2 on a ball prefab -- decides what happens when that ball
// hits a brick, including how much damage it deals. BallControllerV2 doesn't know or care about
// damage/VFX/bonus behavior at all; it just reports the hit and this component handles the rest.
public abstract class BaseBallHitEffect : MonoBehaviour
{
    public int AttackDamage = 1;

    // Which Ball Enhance type this effect's Chance/Range/ProcCount axes are keyed under -- Basic
    // returns BallEnhanceType.Basic, which BallEnhanceManager always reports as having no axes.
    protected abstract BallEnhanceType EnhanceType { get; }

    private BallControllerV2 _ballController;

    private void Awake()
    {
        _ballController = GetComponent<BallControllerV2>();
    }

    // Template method -- concrete effects implement ResolveHit for what happens on a single
    // trigger; this layers the shared Ball Enhance Proc Count axis on top, hard-capped at one
    // extra trigger (2 total per hit) regardless of level.
    public void OnHitBrick(BrickController brickController)
    {
        ResolveHit(brickController);

        float procChance = BallEnhanceManager.Instance != null ? BallEnhanceManager.Instance.GetProcChance(EnhanceType) : 0f;
        if (procChance > 0f && Random.value < procChance)
        {
            ResolveHit(brickController);
        }
    }

    protected abstract void ResolveHit(BrickController brickController);

    // baseChance/baseRange are the effect's own hardcoded field values -- used unmodified at
    // Level 0, replaced (not stacked) once Chance/Range has been purchased.
    protected float GetEnhancedChance(float baseChance)
    {
        return BallEnhanceManager.Instance != null ? BallEnhanceManager.Instance.GetChanceValue(EnhanceType, baseChance) : baseChance;
    }

    protected int GetEnhancedRange(int baseRange)
    {
        return BallEnhanceManager.Instance != null ? BallEnhanceManager.Instance.GetRangeValue(EnhanceType, baseRange) : baseRange;
    }

    // Full damage -- for the brick the ball physically hit.
    protected void DealDamage(BrickController brickController)
    {
        ApplyDamage(brickController, GetCurrentDamage());
    }

    // Half of what the directly-hit brick takes, floored at 1 -- for the EXTRA bricks a spread
    // effect reaches (Fire/Lightning neighbors, Row/Column/Cross lines), so a multi-brick effect
    // doesn't apply full damage across the whole board. Bomb deliberately doesn't use this: its
    // extra hit is a full-damage re-hit of the same brick.
    protected void DealSpreadDamage(BrickController brickController)
    {
        ApplyDamage(brickController, Mathf.Max(1, GetCurrentDamage() / 2));
    }

    private int GetCurrentDamage()
    {
        int damage = AttackDamage;

        if (PowerUpManager.Instance != null)
        {
            BallHitContext context = _ballController != null ? _ballController.CurrentHitContext : default;

            damage += PowerUpManager.Instance.GetTotalBonusBallDamage();
            damage += PowerUpManager.Instance.GetTotalBonusDamage(context);
            damage = Mathf.RoundToInt(damage * PowerUpManager.Instance.GetTotalDamageMultiplier(context));
        }

        return damage;
    }

    private void ApplyDamage(BrickController brickController, int damage)
    {
        if (brickController == null) return;

        brickController.DamageBrick(damage);

        float skillChargeMultiplier = PowerUpManager.Instance != null ? PowerUpManager.Instance.GetTotalSkillChargeMultiplier() : 1f;
        GameManager.Instance.SkillManager.AddSkillPoint(1f * skillChargeMultiplier);
    }

    protected void PlayVfx(GameObject vfx, Vector3 position)
    {
        if (vfx == null) return;
        VFXManager.Instance.PlayVFX(vfx, position);
    }
}
