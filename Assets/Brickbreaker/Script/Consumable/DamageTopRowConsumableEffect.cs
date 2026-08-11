using System.Collections.Generic;
using UnityEngine;

// #4: Damage all bricks in the entire top row.
[CreateAssetMenu(fileName = "DamageTopRowConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Damage Top Row")]
public class DamageTopRowConsumableEffect : BaseConsumableEffect
{
    public int Damage = 15;

    public override void Use()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        List<BrickController> bricks = brickManager.GetBricksInRow(brickManager.SpawnRow);
        foreach (BrickController brick in bricks)
        {
            brick.DamageBrick(Damage);
        }
    }

    public override string GetDescription() => $"Deal {Damage} damage to every brick in the top row";
}
