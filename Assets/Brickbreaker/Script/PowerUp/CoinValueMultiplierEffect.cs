using UnityEngine;

// #23: +x Coin gained from brick kills.
[CreateAssetMenu(fileName = "CoinValueMultiplierEffect", menuName = "Brickbreaker/PowerUp Effect/Coin Value Multiplier")]
public class CoinValueMultiplierEffect : BasePowerUpEffect
{
    public float Multiplier = 1.2f;

    public override float GetCoinValueMultiplier() => Multiplier;
}
