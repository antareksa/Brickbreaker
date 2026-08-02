using System.Collections.Generic;
using UnityEngine;

public class RowBallHitEffect : BaseBallHitEffect
{
    [Header("Row Ball Config")]
    [Range(0, 1)] public float spreadChance = 0.5f;
    public GameObject HitVfx;
    public GameObject RowVfx;

    public override void OnHitBrick(BrickController brickController)
    {
        DealDamage(brickController);

        if (Random.value < spreadChance)
        {
            PlayVfx(HitVfx, brickController.transform.position);

            int row = brickController.GridPosition.y;
            Vector3 vfxPosition = GameManager.Instance.BrickManager.GetRowLeftWorldPosition(row);
            Debug.Log($"[RowVFX debug] hit brick grid pos={brickController.GridPosition}, brick world pos={brickController.transform.position}, computed row={row}, computed vfxPosition={vfxPosition}");
            PlayVfx(RowVfx, vfxPosition);

            List<BrickController> rowBricks = GameManager.Instance.BrickManager.GetBricksInRow(row);
            foreach (BrickController brick in rowBricks)
            {
                if (brick == brickController) continue;
                DealDamage(brick);
            }
        }
    }
}
