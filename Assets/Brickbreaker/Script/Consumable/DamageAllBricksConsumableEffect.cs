using System.Collections.Generic;
using UnityEngine;

// #7: Damage every brick on the board by a flat small amount.
[CreateAssetMenu(fileName = "DamageAllBricksConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Damage All Bricks")]
public class DamageAllBricksConsumableEffect : BaseConsumableEffect
{
    public int Damage = 5;

    public override void Use()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        List<BrickController> bricks = brickManager.GetAllBricks();
        foreach (BrickController brick in bricks)
        {
            brick.DamageBrick(Damage);
        }
    }

    public override string GetDescription() => $"Deal {Damage} damage to every brick on the board";
}
