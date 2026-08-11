using System.Collections.Generic;
using UnityEngine;

// #5: Damage all bricks in two random rows.
[CreateAssetMenu(fileName = "DamageTwoRandomRowsConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Damage Two Random Rows")]
public class DamageTwoRandomRowsConsumableEffect : BaseConsumableEffect
{
    public int Damage = 15;

    public override void Use()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        HashSet<int> rowSet = new HashSet<int>();
        foreach (BrickController brick in brickManager.GetAllBricks())
        {
            rowSet.Add(brick.GridPosition.y);
        }
        if (rowSet.Count == 0) return;

        List<int> rows = new List<int>(rowSet);

        int firstIndex = Random.Range(0, rows.Count);
        int firstRow = rows[firstIndex];
        rows.RemoveAt(firstIndex);

        List<int> targetRows = new List<int> { firstRow };
        if (rows.Count > 0)
        {
            targetRows.Add(rows[Random.Range(0, rows.Count)]);
        }

        foreach (int row in targetRows)
        {
            foreach (BrickController brick in brickManager.GetBricksInRow(row))
            {
                brick.DamageBrick(Damage);
            }
        }
    }

    public override string GetDescription() => $"Deal {Damage} damage to every brick in two random rows";
}
