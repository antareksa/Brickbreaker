using System.Collections.Generic;
using UnityEngine;

public class LightningBallHitEffect : BaseBallHitEffect
{
    [Header("Lightning Ball Config")]
    [Range(0, 1)] public float spreadChance = 0.5f;
    public GameObject LightningVfx;

    protected override BallEnhanceType EnhanceType => BallEnhanceType.Lightning;

    protected override void ResolveHit(BrickController brickController)
    {
        DealDamage(brickController);

        float chance = GetEnhancedChance(spreadChance);
        if (Random.value < chance)
        {
            PlayVfx(LightningVfx, brickController.transform.position);

            // Range axis: how many tiles the spread reaches -- base 1 matches the original
            // immediate-neighbor-only behavior exactly, Level 2/3 extend it further.
            int reach = GetEnhancedRange(1);
            List<BrickController> neighbors = GameManager.Instance.BrickManager.GetDiagonalNeighbors(brickController, reach);
            foreach (BrickController neighbor in neighbors)
            {
                DealSpreadDamage(neighbor);
            }
        }
    }
}
