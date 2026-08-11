using UnityEngine;

// #25: +1 flat Coin per brick destroyed (on top of normal coin value).
[CreateAssetMenu(fileName = "BonusCoinPerBrickEffect", menuName = "Brickbreaker/PowerUp Effect/Bonus Coin Per Brick")]
public class BonusCoinPerBrickEffect : BasePowerUpEffect
{
    public int BonusCoin = 1;

    public override int GetBonusCoinPerBrick() => BonusCoin;

    public override string GetDescription() => $"+{BonusCoin} Coin per brick destroyed";
}
