using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One offered PowerUp or Consumable in the Shop's card-offer section (top area in the reference)
// -- selectable, unlike PowerUpPanel/ConsumablePanel which are read-only/use-only display for
// already-owned items. Both share this same slot/prefab since they compete in the same offer pool
// (see ShopHUD.GenerateCardOffers) -- only one of PowerUp/Consumable is ever set at a time.
public class CardOfferPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image PowerUpIcon;
    public TMP_Text PowerUpNameText;

    // Which of the two competing card types this slot rolled -- the panel looks identical
    // otherwise, so without this the player can't tell a passive PowerUp from a one-use Consumable.
    public TMP_Text TypeText;

    public TMP_Text DescriptionText;
    public TMP_Text CostText;
    public Button SelectButton;
    public TMP_Text SelectButtonText;

    public GameObject RedLabel;
    public GameObject BlueLabel;

    public float HoverScale = 1.1f;

    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    public BasePowerUp PowerUp { get; private set; }
    public BaseConsumable Consumable { get; private set; }
    public bool IsSold { get; private set; }

    private int _cost;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void SetInfo(BasePowerUp powerUp)
    {
        PowerUp = powerUp;
        Consumable = null;
        IsSold = false;
        _cost = powerUp.BuyCost;

        PowerUpIcon.sprite = powerUp.PowerUpImage;
        PowerUpNameText.text = powerUp.PowerUpName;
        DescriptionText.text = powerUp.GetDescription();
        if (TypeText != null) 
        { 
            TypeText.text = "PowerUp";
            RedLabel.SetActive(false);
            BlueLabel.SetActive(true);
        }
        if (CostText != null) CostText.text = _cost.ToString();

        SelectButton.interactable = true;
        if (SelectButtonText != null) SelectButtonText.text = _cost.ToString();
    }

    public void SetInfo(BaseConsumable consumable)
    {
        Consumable = consumable;
        PowerUp = null;
        IsSold = false;
        _cost = consumable.BuyCost;

        PowerUpIcon.sprite = consumable.ConsumableImage;
        PowerUpNameText.text = consumable.ConsumableName;
        DescriptionText.text = consumable.GetDescription();
        if (TypeText != null) 
        { 
            TypeText.text = "Consumable";
            RedLabel.SetActive(true);
            BlueLabel.SetActive(false);
        }
        if (CostText != null) CostText.text = _cost.ToString();

        SelectButton.interactable = true;
        if (SelectButtonText != null) SelectButtonText.text = _cost.ToString();
    }

    // "Purchased" rather than "Equipped" -- the same panel is used for Consumables, which go to
    // the held inventory rather than an equip slot.
    public void MarkSelected()
    {
        IsSold = true;
        SelectButton.interactable = false;
        if (SelectButtonText != null) SelectButtonText.text = "Purchased";
    }

    // Called when Coin Shop changes (reroll spend, purchase spend) so unsold panels reflect
    // whether they're still affordable -- doesn't touch already-sold panels.
    public void RefreshAffordability(int currentCoinShop)
    {
        if (IsSold) return;
        SelectButton.interactable = currentCoinShop >= _cost;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * HoverScale;
        OnHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        OnHoverExit?.Invoke();
    }
}
