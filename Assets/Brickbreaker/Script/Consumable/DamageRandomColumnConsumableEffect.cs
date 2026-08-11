using System.Collections.Generic;
using UnityEngine;

// #2: Damage all bricks in a random column.
[CreateAssetMenu(fileName = "DamageRandomColumnConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Damage Random Column")]
public class DamageRandomColumnConsumableEffect : BaseConsumableEffect
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
        int column = columns[Random.Range(0, columns.Count)];

        foreach (BrickController brick in brickManager.GetBricksInColumn(column))
        {
            brick.DamageBrick(Damage);
        }
    }

    public override string GetDescription() => $"Deal {Damage} damage to every brick in a random column";
}
