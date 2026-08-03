using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One offered PowerUp or Consumable in the Shop's card-offer section (top area in the reference)
// -- selectable, unlike PowerUpPanel/ConsumablePanel which are read-only/use-only display for
// already-owned items. Both share this same slot/prefab since they compete in the same offer pool
// (see ShopHUD.GenerateCardOffers) -- only one of PowerUp/Consumable is ever set at a time.
public class CardOfferPanel : MonoBehaviour
{
    public Image PowerUpIcon;
    public TMP_Text PowerUpNameText;
    public TMP_Text DescriptionText;
    public TMP_Text CostText;
    public Button SelectButton;
    public TMP_Text SelectButtonText;

    public BasePowerUp PowerUp { get; private set; }
    public BaseConsumable Consumable { get; private set; }
    public bool IsSold { get; private set; }

    private int _cost;

    public void SetInfo(BasePowerUp powerUp)
    {
        PowerUp = powerUp;
        Consumable = null;
        IsSold = false;
        _cost = powerUp.BuyCost;

        PowerUpIcon.sprite = powerUp.PowerUpImage;
        PowerUpNameText.text = powerUp.PowerUpName;
        DescriptionText.text = powerUp.Description;
        if (CostText != null) CostText.text = _cost.ToString();

        SelectButton.interactable = true;
        if (SelectButtonText != null) SelectButtonText.text = "Select";
    }

    public void SetInfo(BaseConsumable consumable)
    {
        Consumable = consumable;
        PowerUp = null;
        IsSold = false;
        _cost = consumable.BuyCost;

        PowerUpIcon.sprite = consumable.ConsumableImage;
        PowerUpNameText.text = consumable.ConsumableName;
        DescriptionText.text = consumable.Description;
        if (CostText != null) CostText.text = _cost.ToString();

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
        SelectButton.interactable = currentCoinShop >= _cost;
    }
}
