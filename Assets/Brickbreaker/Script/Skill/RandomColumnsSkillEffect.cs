using System.Collections.Generic;
using UnityEngine;

public class RandomColumnsSkillEffect : BaseSkillEffect
{
    private static readonly int[] EvenColumns = { 2, 4, 6 };
    private static readonly int[] OddColumns = { 1, 3, 5 };

    public override void Activate()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        int[] columns = Random.value < 0.5f ? EvenColumns : OddColumns;

        foreach (int column in columns)
        {
            List<BrickController> bricks = brickManager.GetBricksInColumn(column);

            foreach (BrickController brick in bricks)
            {
                DealDamageToBrick(brick);
            }
        }
    }
}
