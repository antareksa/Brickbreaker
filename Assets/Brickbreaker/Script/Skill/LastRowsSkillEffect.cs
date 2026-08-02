using System.Collections.Generic;
using UnityEngine;

public class LastRowsSkillEffect : BaseSkillEffect
{
    public int RowCount = 3;
    public GameObject RowVfx;

    public override void Activate()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        for (int i = 1; i <= RowCount; i++)
        {
            int row = brickManager.BottomRow + i;

            PlayVfx(RowVfx, brickManager.GetRowLeftWorldPosition(row));

            List<BrickController> bricks = brickManager.GetBricksInRow(row);
            foreach (BrickController brick in bricks)
            {
                brick.DamageBrick(CurrentDamage);
            }
        }
    }
}
