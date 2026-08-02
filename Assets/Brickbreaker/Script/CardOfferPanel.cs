using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One offered PowerUp in the Shop's card-offer section (top area in the reference) --
// selectable, unlike PowerUpPanel which is read-only display for already-equipped PowerUps.
public class CardOfferPanel : MonoBehaviour
{
    public Image PowerUpIcon;
    public TMP_Text PowerUpNameText;
    public TMP_Text DescriptionText;
    public TMP_Text CostText;
    public Button SelectButton;
    public TMP_Text SelectButtonText;

    public BasePowerUp PowerUp { get; private set; }
    public bool IsSold { get; private set; }

    public void SetInfo(BasePowerUp powerUp)
    {
        PowerUp = powerUp;
        IsSold = false;

        PowerUpIcon.sprite = powerUp.PowerUpImage;
        PowerUpNameText.text = powerUp.PowerUpName;
        DescriptionText.text = powerUp.Description;
        if (CostText != null) CostText.text = powerUp.BuyCost.ToString();

        SelectButton.interactable = true;
        if (SelectButtonText != null) SelectButtonText.text = "Select";
    }

    public void MarkSelected()
    {
        IsSold = true;
        SelectButton.interactable = false;
        if (SelectButtonText != null) SelectButtonText.text = "Equipped";
    }

    // Called when Coin Shop changes (reroll spend, purchase spend) so unsold panels reflect
    // whether they're still affordable -- doesn't touch already-sold panels.
    public void RefreshAffordability(int currentCoinShop)
    {
        if (IsSold) return;
        SelectButton.interactable = currentCoinShop >= PowerUp.BuyCost;
    }
}
