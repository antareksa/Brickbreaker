using System.Collections.Generic;
using UnityEngine;

public class BombBallHitEffect : BaseBallHitEffect
{
    [Header("Bomb Ball Config")]
    [Range(0, 1)] public float extraDamageChance = 0.5f;
    public GameObject BombEffectVfx;

    protected override BallEnhanceType EnhanceType => BallEnhanceType.Bomb;

    protected override void ResolveHit(BrickController brickController)
    {
        DealDamage(brickController);

        float chance = GetEnhancedChance(extraDamageChance);
        if (Random.value < chance)
        {
            DealDamage(brickController);
            PlayVfx(BombEffectVfx, brickController.transform.position);
        }

        // Range axis: a guaranteed (not chance-gated) number of random neighboring bricks also
        // damaged -- 0 at base (unpurchased), since Bomb currently never touches neighbors at all.
        int neighborCount = GetEnhancedRange(0);
        if (neighborCount > 0)
        {
            List<BrickController> candidates = GameManager.Instance.BrickManager.GetAllNeighbors(brickController);
            for (int i = 0; i < neighborCount && candidates.Count > 0; i++)
            {
                int index = Random.Range(0, candidates.Count);
                DealDamage(candidates[index]);
                candidates.RemoveAt(index);
            }
        }
    }
}
