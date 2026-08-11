using System.Collections.Generic;
using UnityEngine;

// #6: Damage all bricks in two random columns.
[CreateAssetMenu(fileName = "DamageTwoRandomColumnsConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Damage Two Random Columns")]
public class DamageTwoRandomColumnsConsumableEffect : BaseConsumableEffect
{
    public int Damage = 15;

    public override void Use()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        HashSet<int> columnSet = new HashSet<int>();
        foreach (BrickController brick in brickManager.GetAllBricks())
        {
            columnSet.Add(brick.GridPosition.x);
        }
        if (columnSet.Count == 0) return;

        List<int> columns = new List<int>(columnSet);

        int firstIndex = Random.Range(0, columns.Count);
        int firstColumn = columns[firstIndex];
        columns.RemoveAt(firstIndex);

        List<int> targetColumns = new List<int> { firstColumn };
        if (columns.Count > 0)
        {
            targetColumns.Add(columns[Random.Range(0, columns.Count)]);
        }

        foreach (int column in targetColumns)
        {
            foreach (BrickController brick in brickManager.GetBricksInColumn(column))
            {
                brick.DamageBrick(Damage);
            }
        }
    }

    public override string GetDescription() => $"Deal {Damage} damage to every brick in two random columns";
}
