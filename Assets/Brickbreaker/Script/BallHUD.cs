using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallHUD : BaseHUD
{
    public TMP_Text CoinCountText;
    public Button AddOneBallButton;
    public Button AddSixBallButton;

    public TMP_Text AddOneBallCostText;
    public TMP_Text AddSixBallCostText;

    protected override void Start()
    {
        base.Start();

        AddOneBallButton.onClick.AddListener(() => OnBuyBall(1));
        AddSixBallButton.onClick.AddListener(() => OnBuyBall(6));

        AddOneBallCostText.text = GameManager.Instance.LaunchManager.GetBallCost(1).ToString();
        AddSixBallCostText.text = GameManager.Instance.LaunchManager.GetBallCost(6).ToString();

        GameManager.Instance.OnCoinChanged.AddListener(HandleCoinChanged);

        // Prime the display with the current value -- a subscription only fires on future
        // changes, not the value that already existed before this HUD subscribed.
        HandleCoinChanged(GameManager.Instance.GetCoin());
    }

    private void HandleCoinChanged(int coin)
    {
        CoinCountText.text = coin.ToString();

        AddOneBallButton.interactable = GameManager.Instance.LaunchManager.IsCanBuyBall(1);
        AddSixBallButton.interactable = GameManager.Instance.LaunchManager.IsCanBuyBall(6);
    }

    private void OnBuyBall(int totalBall)
    {
        GameManager.Instance.LaunchManager.BuyBall(totalBall);
    }
}
