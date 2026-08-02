using System.Collections.Generic;
using UnityEngine;

// Mirrors LastRowsSkillEffect but from the other end of the field: the newest rows near the
// spawn row, instead of the rows closest to the player.
public class FirstRowsSkillEffect : BaseSkillEffect
{
    public int RowCount = 3;
    public GameObject RowVfx;

    public override void Activate()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        for (int i = 0; i < RowCount; i++)
        {
            int row = brickManager.SpawnRow - i;

            PlayVfx(RowVfx, brickManager.GetRowLeftWorldPosition(row));

            List<BrickController> bricks = brickManager.GetBricksInRow(row);
            foreach (BrickController brick in bricks)
            {
                brick.DamageBrick(CurrentDamage);
            }
        }
    }
}
