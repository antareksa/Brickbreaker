using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One sealed Ball Enhance pack offer shown in the Shop -- rolls its own contents once (fixed for
// the visit, not rerollable), but doesn't reveal them inline. Buying fires OnBought with the
// rolled contents; ShopHUD wires that to the shared BallEnhanceRevealPopup, since only one popup
// instance exists no matter how many pack offers are on screen.
public class BallEnhancePackOfferPanel : MonoBehaviour
{
    public TMP_Text ThemeText;
    public TMP_Text CostText;
    public Button BuyButton;

    public Action<List<(BallEnhanceType Type, BallEnhanceAxis Axis)>> OnBought;

    private List<(BallEnhanceType Type, BallEnhanceAxis Axis)> _options;
    private bool _isBought;

    public void Generate()
    {
        _isBought = false;
        _options = BallEnhanceManager.Instance.GenerateThemedPack();

        // Slot 0 is always the themed pick (see GenerateThemedPack) -- reuse its type as the
        // pack's displayed theme rather than rolling a separate one here.
        BallEnhanceType theme = _options.Count > 0 ? _options[0].Type : BallEnhanceType.Basic;
        ThemeText.text = $"{theme} Pack";
        if (CostText != null) CostText.text = BallEnhanceManager.Instance.PackBuyCost.ToString();

        gameObject.SetActive(true);
        BuyButton.onClick.RemoveAllListeners();
        BuyButton.onClick.AddListener(HandleBuyClicked);
        RefreshAffordability(GameManager.Instance.GetCoinShop());
    }

    private void HandleBuyClicked()
    {
        if (_isBought) return;
        if (!BallEnhanceManager.Instance.TrySpendForPack()) return;

        _isBought = true;
        gameObject.SetActive(false); // consumed -- hide the sealed offer once bought

        OnBought?.Invoke(_options);
    }

    public void RefreshAffordability(int currentCoinShop)
    {
        if (_isBought) return;
        BuyButton.interactable = currentCoinShop >= BallEnhanceManager.Instance.PackBuyCost;
    }
}
