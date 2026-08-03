using System.Collections.Generic;
using UnityEngine;

// #9: Damage all bricks in the rightmost column.
[CreateAssetMenu(fileName = "DamageRightmostColumnConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Damage Rightmost Column")]
public class DamageRightmostColumnConsumableEffect : BaseConsumableEffect
{
    public int Damage = 15;

    public override void Use()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        List<BrickController> bricks = brickManager.GetBricksInColumn(brickManager.SpawnColumnRange.y);
        foreach (BrickController brick in bricks)
        {
            brick.DamageBrick(Damage);
        }
    }
}
