using UnityEngine;

public class BombBallHitEffect : BaseBallHitEffect
{
    [Header("Bomb Ball Config")]
    [Range(0, 1)] public float extraDamageChance = 0.5f;
    public GameObject BombEffectVfx;

    public override void OnHitBrick(BrickController brickController)
    {
        DealDamage(brickController);

        if (Random.value < extraDamageChance)
        {
            DealDamage(brickController);
            PlayVfx(BombEffectVfx, brickController.transform.position);
        }
    }
}
