using System.Collections.Generic;
using UnityEngine;

public class RowBallHitEffect : BaseBallHitEffect
{
    [Header("Row Ball Config")]
    [Range(0, 1)] public float spreadChance = 0.5f;
    public GameObject HitVfx;
    public GameObject RowVfx;

    protected override BallEnhanceType EnhanceType => BallEnhanceType.Row;

    protected override void ResolveHit(BrickController brickController)
    {
        DealDamage(brickController);

        float chance = GetEnhancedChance(spreadChance);
        if (Random.value < chance)
        {
            PlayVfx(HitVfx, brickController.transform.position);

            int row = brickController.GridPosition.y;
            Vector3 vfxPosition = GameManager.Instance.BrickManager.GetRowLeftWorldPosition(row);
            PlayVfx(RowVfx, vfxPosition);

            List<BrickController> rowBricks = GameManager.Instance.BrickManager.GetBricksInRow(row);
            foreach (BrickController brick in rowBricks)
            {
                if (brick == brickController) continue;
                DealSpreadDamage(brick);
            }
        }
    }
}
