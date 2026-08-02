using System.Collections.Generic;
using UnityEngine;

public class FireBallHitEffect : BaseBallHitEffect
{
    [Header("Fire Ball Config")]
    [Range(0, 1)] public float spreadChance = 0.5f;
    public GameObject FireVfx;

    public override void OnHitBrick(BrickController brickController)
    {
        DealDamage(brickController);

        if (Random.value < spreadChance)
        {
            PlayVfx(FireVfx, brickController.transform.position);

            List<BrickController> neighbors = GameManager.Instance.BrickManager.GetSideNeighbors(brickController);
            foreach (BrickController neighbor in neighbors)
            {
                DealDamage(neighbor);
            }
        }
    }
}
