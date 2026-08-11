using System.Collections.Generic;
using UnityEngine;

public class FireBallHitEffect : BaseBallHitEffect
{
    [Header("Fire Ball Config")]
    [Range(0, 1)] public float spreadChance = 0.5f;
    public GameObject FireVfx;

    protected override BallEnhanceType EnhanceType => BallEnhanceType.Fire;

    protected override void ResolveHit(BrickController brickController)
    {
        DealDamage(brickController);

        float chance = GetEnhancedChance(spreadChance);
        if (Random.value < chance)
        {
            PlayVfx(FireVfx, brickController.transform.position);

            // Range axis: how many tiles the spread reaches -- base 1 matches the original
            // immediate-neighbor-only behavior exactly, Level 2/3 extend it further.
            int reach = GetEnhancedRange(1);
            List<BrickController> neighbors = GameManager.Instance.BrickManager.GetSideNeighbors(brickController, reach);
            foreach (BrickController neighbor in neighbors)
            {
                DealSpreadDamage(neighbor);
            }
        }
    }
}
