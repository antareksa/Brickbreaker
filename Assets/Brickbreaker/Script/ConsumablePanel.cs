using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Held-inventory display for one Consumable -- unlike CardOfferPanel (Shop, buy), this is the
// gameplay HUD for something already owned, with a button to trigger it instead of select it.
public class ConsumablePanel : MonoBehaviour
{
    public Image ConsumableIcon;
    public TMP_Text ConsumableNameText;
    public TMP_Text DescriptionText;
    public Button UseButton;

    private BaseConsumable _consumable;

    public void SetInfo(BaseConsumable consumable)
    {
        _consumable = consumable;

        ConsumableIcon.sprite = consumable.ConsumableImage;
        ConsumableNameText.text = consumable.ConsumableName;
        DescriptionText.text = consumable.Description;

        UseButton.onClick.RemoveAllListeners();
        UseButton.onClick.AddListener(HandleUseClicked);
    }

    // Mirrors ConsumableManager.TryUse's own Aiming-only gate, so the button reads as disabled
    // instead of silently doing nothing when clicked mid-shot.
    public void RefreshUsability(GameState state)
    {
        UseButton.interactable = state == GameState.Aiming;
    }

    private void HandleUseClicked()
    {
        ConsumableManager.Instance.TryUse(_consumable);
    }
}
