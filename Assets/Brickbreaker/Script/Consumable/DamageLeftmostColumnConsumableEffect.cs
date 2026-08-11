using System.Collections.Generic;
using UnityEngine;

// #8: Damage all bricks in the leftmost column.
[CreateAssetMenu(fileName = "DamageLeftmostColumnConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Damage Leftmost Column")]
public class DamageLeftmostColumnConsumableEffect : BaseConsumableEffect
{
    public int Damage = 15;

    public override void Use()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        List<BrickController> bricks = brickManager.GetBricksInColumn(brickManager.SpawnColumnRange.x);
        foreach (BrickController brick in bricks)
        {
            brick.DamageBrick(Damage);
        }
    }

    public override string GetDescription() => $"Deal {Damage} damage to every brick in the leftmost column";
}
