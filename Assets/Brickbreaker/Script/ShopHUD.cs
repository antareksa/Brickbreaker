using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Layout mirrors the Balatro-style reference: CardOffer (top, PowerUp picks -- 100% PowerUp for
// now, no Consumable pool yet) + PackOffer (bottom, Ball Enhance packs -- scaffolding only,
// not implemented yet) + Reroll + Next. BrickManager opens this after a boss phase/defeat
// change and waits on IsOpen before continuing to AdvanceWave.
public class ShopHUD : BaseHUD
{
    public GameObject PopupRoot;

    [Header("Card Offer (PowerUp picks)")]
    public CardOfferPanel CardOfferPanelPrefab;
    public Transform CardOfferContainer;
    public int CardOfferCount = 2;

    [Header("Pack Offer (Ball Enhance -- not implemented yet)")]
    public GameObject PackOfferSection;

    [Header("Buttons")]
    public Button RerollButton;
    public TMP_Text RerollButtonText;
    public int RerollCost = 2;
    public int RerollCostIncrement = 1;
    public Button NextButton;

    [Header("Currency")]
    public TMP_Text CoinShopText;

    public bool IsOpen { get; private set; }

    // Escalates each reroll within a visit (RerollCostIncrement per reroll), reset back to
    // RerollCost every time the shop opens.
    private int _currentRerollCost;

    private readonly List<CardOfferPanel> _cardOfferPanels = new List<CardOfferPanel>();

    protected override void Start()
    {
        base.Start();

        NextButton.onClick.AddListener(Close);
        RerollButton.onClick.AddListener(HandleRerollClicked);
        GameManager.Instance.OnCoinShopChanged.AddListener(HandleCoinShopChanged);

        PopupRoot.SetActive(false);
    }

    public void Open()
    {
        IsOpen = true;
        PopupRoot.SetActive(true);

        _currentRerollCost = RerollCost;
        GenerateCardOffers();
        HandleCoinShopChanged(GameManager.Instance.GetCoinShop());
    }

    public void Close()
    {
        IsOpen = false;
        PopupRoot.SetActive(false);
    }

    private void HandleRerollClicked()
    {
        if (!GameManager.Instance.TrySpendCoinShop(_currentRerollCost)) return;

        _currentRerollCost += RerollCostIncrement;
        GenerateCardOffers();
    }

    // Picks CardOfferCount unique random entries from the roster (excluding whatever's already
    // equipped, so a reroll can't offer a duplicate of something the player owns) and shows them
    // as selectable panels. Rebuilt from scratch on open and on every reroll.
    private void GenerateCardOffers()
    {
        foreach (CardOfferPanel panel in _cardOfferPanels)
        {
            Destroy(panel.gameObject);
        }
        _cardOfferPanels.Clear();

        IReadOnlyList<BasePowerUp> equipped = PowerUpManager.Instance.GetEquipped();
        List<BasePowerUp> pool = new List<BasePowerUp>();
        foreach (BasePowerUp powerUp in PowerUpManager.Instance.Roster)
        {
            bool isEquipped = false;
            for (int i = 0; i < equipped.Count; i++)
            {
                if (equipped[i] == powerUp)
                {
                    isEquipped = true;
                    break;
                }
            }

            if (!isEquipped)
            {
                pool.Add(powerUp);
            }
        }

        int coinShop = GameManager.Instance.GetCoinShop();

        for (int i = 0; i < CardOfferCount && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            BasePowerUp powerUp = pool[index];
            pool.RemoveAt(index);

            CardOfferPanel panel = Instantiate(CardOfferPanelPrefab, CardOfferContainer);
            panel.SetInfo(powerUp);
            panel.RefreshAffordability(coinShop);
            panel.SelectButton.onClick.AddListener(() => HandleCardSelected(panel));
            _cardOfferPanels.Add(panel);
        }

        RefreshRerollButton();
    }

    // Doesn't regenerate the other offers or close the shop -- just marks this slot taken, same
    // as Balatro leaving the rest of the shop as-is after one purchase.
    private void HandleCardSelected(CardOfferPanel panel)
    {
        BasePowerUp powerUp = panel.PowerUp;

        if (GameManager.Instance.GetCoinShop() < powerUp.BuyCost) return;
        if (PowerUpManager.Instance.IsFull) return;
        if (!PowerUpManager.Instance.TryEquip(powerUp)) return;

        GameManager.Instance.TrySpendCoinShop(powerUp.BuyCost);
        panel.MarkSelected();
    }

    // Fires on every Coin Shop change (reward, reroll spend, purchase spend) -- keeps the label
    // and every button's affordability state in sync without needing a manual refresh call at
    // each spend site.
    private void HandleCoinShopChanged(int amount)
    {
        if (CoinShopText != null) CoinShopText.text = amount.ToString();

        RefreshRerollButton();

        foreach (CardOfferPanel panel in _cardOfferPanels)
        {
            panel.RefreshAffordability(amount);
        }
    }

    private void RefreshRerollButton()
    {
        if (RerollButtonText != null) RerollButtonText.text = $"Reroll ({_currentRerollCost})";
        RerollButton.interactable = GameManager.Instance.GetCoinShop() >= _currentRerollCost;
    }
}
