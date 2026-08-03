using System.Collections.Generic;
using UnityEngine;

// #3: Damage all bricks in the entire bottom row.
[CreateAssetMenu(fileName = "DamageBottomRowConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Damage Bottom Row")]
public class DamageBottomRowConsumableEffect : BaseConsumableEffect
{
    public int Damage = 15;

    public override void Use()
    {
        BrickManager brickManager = GameManager.Instance.BrickManager;

        // BottomRow + 1, not BottomRow itself -- BottomRow is the reach-bottom boundary a brick
        // never actually occupies, same convention LastRowsSkillEffect uses for "closest to the
        // bottom".
        int row = brickManager.BottomRow + 1;

        List<BrickController> bricks = brickManager.GetBricksInRow(row);
        foreach (BrickController brick in bricks)
        {
            brick.DamageBrick(Damage);
        }
    }
}
