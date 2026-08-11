using System.Collections.Generic;
using UnityEngine;

public class ColumnBallHitEffect : BaseBallHitEffect
{
    [Header("Column Ball Config")]
    [Range(0, 1)] public float spreadChance = 0.5f;
    public GameObject HitVfx;
    public GameObject ColumnVfx;

    protected override BallEnhanceType EnhanceType => BallEnhanceType.Column;

    protected override void ResolveHit(BrickController brickController)
    {
        DealDamage(brickController);

        float chance = GetEnhancedChance(spreadChance);
        if (Random.value < chance)
        {
            PlayVfx(HitVfx, brickController.transform.position);

            int column = brickController.GridPosition.x;
            PlayVfx(ColumnVfx, GameManager.Instance.BrickManager.GetColumnBottomWorldPosition(column));

            List<BrickController> columnBricks = GameManager.Instance.BrickManager.GetBricksInColumn(column);
            foreach (BrickController brick in columnBricks)
            {
                if (brick == brickController) continue;
                DealSpreadDamage(brick);
            }
        }
    }
}
