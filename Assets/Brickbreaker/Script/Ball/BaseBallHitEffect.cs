using UnityEngine;

// Attached alongside BallControllerV2 on a ball prefab -- decides what happens when that ball
// hits a brick, including how much damage it deals. BallControllerV2 doesn't know or care about
// damage/VFX/bonus behavior at all; it just reports the hit and this component handles the rest.
public abstract class BaseBallHitEffect : MonoBehaviour
{
    public int AttackDamage = 1;

    private BallControllerV2 _ballController;

    private void Awake()
    {
        _ballController = GetComponent<BallControllerV2>();
    }

    public abstract void OnHitBrick(BrickController brickController);

    protected void DealDamage(BrickController brickController)
    {
        if (brickController == null) return;

        int damage = AttackDamage;

        if (PowerUpManager.Instance != null)
        {
            BallHitContext context = _ballController != null ? _ballController.CurrentHitContext : default;

            damage += PowerUpManager.Instance.GetTotalBonusBallDamage();
            damage += PowerUpManager.Instance.GetTotalBonusDamage(context);
            damage = Mathf.RoundToInt(damage * PowerUpManager.Instance.GetTotalDamageMultiplier(context));
        }

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
