using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpPanel : MonoBehaviour
{
    public Image PowerUpIcon;
    public TMP_Text PowerUpNameText;
    public TMP_Text DescriptionText;
    public Button SellButton;
    public TMP_Text SellButtonText;

    private BasePowerUp _powerUp;

    public void SetInfo(BasePowerUp powerUp)
    {
        _powerUp = powerUp;

        PowerUpIcon.sprite = powerUp.PowerUpImage;
        PowerUpNameText.text = powerUp.PowerUpName;
        DescriptionText.text = powerUp.GetDescription();

        if (SellButtonText != null) SellButtonText.text = $"Sell ({powerUp.BuyCost / 2})";

        if (SellButton != null)
        {
            SellButton.onClick.RemoveAllListeners();
            SellButton.onClick.AddListener(HandleSellClicked);
        }
    }

    private void HandleSellClicked()
    {
        PowerUpManager.Instance.Sell(_powerUp);
    }

    // Selling is a Shop transaction -- the button only shows while the Shop is actually open,
    // not during normal gameplay.
    public void SetSellButtonVisible(bool visible)
    {
        if (SellButton != null) SellButton.gameObject.SetActive(visible);
    }
}
