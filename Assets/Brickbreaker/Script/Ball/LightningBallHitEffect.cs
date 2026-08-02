using System.Collections.Generic;
using UnityEngine;

public class LightningBallHitEffect : BaseBallHitEffect
{
    [Header("Lightning Ball Config")]
    [Range(0, 1)] public float spreadChance = 0.5f;
    public GameObject LightningVfx;

    public override void OnHitBrick(BrickController brickController)
    {
        DealDamage(brickController);

        if (Random.value < spreadChance)
        {
            PlayVfx(LightningVfx, brickController.transform.position);

            List<BrickController> neighbors = GameManager.Instance.BrickManager.GetDiagonalNeighbors(brickController);
            foreach (BrickController neighbor in neighbors)
            {
                DealDamage(neighbor);
            }
        }
    }
}
