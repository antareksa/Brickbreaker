using UnityEngine;

// #13: Next ball purchase is guaranteed SSR (top rarity). Only the single next ball added is
// guaranteed -- buying a 6-ball bundle after using this gets 1 guaranteed SSR + 5 normal rolls,
// not all 6 (see LaunchManager.GuaranteeSsrNextBall).
[CreateAssetMenu(fileName = "GuaranteedSsrBallConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Guaranteed SSR Ball")]
public class GuaranteedSsrBallConsumableEffect : BaseConsumableEffect
{
    public override void Use()
    {
        GameManager.Instance.LaunchManager.GuaranteeSsrNextBall();
    }
}
