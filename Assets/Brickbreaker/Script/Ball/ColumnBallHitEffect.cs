using System.Collections.Generic;
using UnityEngine;

public class ColumnBallHitEffect : BaseBallHitEffect
{
    [Header("Column Ball Config")]
    [Range(0, 1)] public float spreadChance = 0.5f;
    public GameObject HitVfx;
    public GameObject ColumnVfx;

    public override void OnHitBrick(BrickController brickController)
    {
        DealDamage(brickController);

        if (Random.value < spreadChance)
        {
            PlayVfx(HitVfx, brickController.transform.position);

            int column = brickController.GridPosition.x;
            PlayVfx(ColumnVfx, GameManager.Instance.BrickManager.GetColumnBottomWorldPosition(column));

            List<BrickController> columnBricks = GameManager.Instance.BrickManager.GetBricksInColumn(column);
            foreach (BrickController brick in columnBricks)
            {
                if (brick == brickController) continue;
                DealDamage(brick);
            }
        }
    }
}
