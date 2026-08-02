using System.Collections.Generic;
using UnityEngine;

public class CrossBallHitEffect : BaseBallHitEffect
{
    [Header("Cross Ball Config")]
    [Range(0, 1)] public float spreadChance = 0.5f;
    public GameObject HitVfx;
    public GameObject RowVfx;
    public GameObject ColumnVfx;

    public override void OnHitBrick(BrickController brickController)
    {
        DealDamage(brickController);

        if (Random.value < spreadChance)
        {
            PlayVfx(HitVfx, brickController.transform.position);

            BrickManager brickManager = GameManager.Instance.BrickManager;
            int row = brickController.GridPosition.y;
            int column = brickController.GridPosition.x;

            PlayVfx(RowVfx, brickManager.GetRowLeftWorldPosition(row));
            PlayVfx(ColumnVfx, brickManager.GetColumnBottomWorldPosition(column));

            List<BrickController> rowBricks = brickManager.GetBricksInRow(row);
            foreach (BrickController brick in rowBricks)
            {
                if (brick == brickController) continue;
                DealDamage(brick);
            }

            List<BrickController> columnBricks = brickManager.GetBricksInColumn(column);
            foreach (BrickController brick in columnBricks)
            {
                if (brick == brickController) continue;
                DealDamage(brick);
            }
        }
    }
}
