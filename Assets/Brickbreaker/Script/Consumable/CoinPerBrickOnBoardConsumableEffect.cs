using UnityEngine;

// #15: Gain 1 flat Coin for every brick currently on the board (scales with board state at
// moment of use).
[CreateAssetMenu(fileName = "CoinPerBrickOnBoardConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Coin Per Brick On Board")]
public class CoinPerBrickOnBoardConsumableEffect : BaseConsumableEffect
{
    public int CoinPerBrick = 1;

    public override void Use()
    {
        int brickCount = GameManager.Instance.BrickManager.GetBrickCount();
        GameManager.Instance.AddCoin(brickCount * CoinPerBrick);
    }

    public override string GetDescription() => $"Gain {CoinPerBrick} Coin per brick currently on the board";
}
