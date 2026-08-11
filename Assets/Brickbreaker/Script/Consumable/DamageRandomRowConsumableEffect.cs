using System.Collections.Generic;
using UnityEngine;

// #1: Damage all bricks in a random row.
[CreateAssetMenu(fileName = "DamageRandomRowConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Damage Random Row")]
public class DamageRandomRowConsumableEffect : BaseConsumableEffect
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
        int row = rows[Random.Range(0, rows.Count)];

        foreach (BrickController brick in brickManager.GetBricksInRow(row))
        {
            brick.DamageBrick(Damage);
        }
    }

    public override string GetDescription() => $"Deal {Damage} damage to every brick in a random row";
}
