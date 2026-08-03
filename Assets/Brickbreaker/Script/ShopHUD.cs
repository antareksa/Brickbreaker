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

    [Header("Card Offer (PowerUp + Consumable picks)")]
    public CardOfferPanel CardOfferPanelPrefab;
    public Transform CardOfferContainer;
    public int CardOfferCount = 2;

    // Odds each offer slot rolls a PowerUp vs a Consumable -- weight ratio between the two pools
    // isn't formally decided per the design doc, so this is a tunable default rather than derived
    // from roster size.
    [Range(0f, 1f)] public float PowerUpOfferChance = 0.6f;

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

    // Picks CardOfferCount entries from a combined PowerUp + Consumable pool and shows them as
    // selectable panels. PowerUp side excludes whatever's already equipped (so a reroll can't
    // offer a duplicate of something the player owns) -- Consumable side has no such exclusion,
    // since holding/buying duplicates of the same Consumable is fine. Rebuilt from scratch on
    // open and on every reroll.
    private void GenerateCardOffers()
    {
        foreach (CardOfferPanel panel in _cardOfferPanels)
        {
            Destroy(panel.gameObject);
        }
        _cardOfferPanels.Clear();

        IReadOnlyList<BasePowerUp> equipped = PowerUpManager.Instance.GetEquipped();
        List<BasePowerUp> powerUpPool = new List<BasePowerUp>();
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
                powerUpPool.Add(powerUp);
            }
        }

        List<BaseConsumable> consumablePool = new List<BaseConsumable>(ConsumableManager.Instance.Roster);

        int coinShop = GameManager.Instance.GetCoinShop();

        for (int i = 0; i < CardOfferCount; i++)
        {
            if (powerUpPool.Count == 0 && consumablePool.Count == 0) break;

            // Coin-flip between the two pools, falling back to whichever still has entries left
            // this visit if the other's empty or already exhausted.
            bool offerPowerUp;
            if (powerUpPool.Count == 0) offerPowerUp = false;
            else if (consumablePool.Count == 0) offerPowerUp = true;
            else offerPowerUp = Random.value < PowerUpOfferChance;

            CardOfferPanel panel = Instantiate(CardOfferPanelPrefab, CardOfferContainer);

            if (offerPowerUp)
            {
                int index = Random.Range(0, powerUpPool.Count);
                BasePowerUp powerUp = powerUpPool[index];
                powerUpPool.RemoveAt(index);
                panel.SetInfo(powerUp);
            }
            else
            {
                int index = Random.Range(0, consumablePool.Count);
                BaseConsumable consumable = consumablePool[index];
                consumablePool.RemoveAt(index);
                panel.SetInfo(consumable);
            }

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
        if (panel.PowerUp != null)
        {
            BasePowerUp powerUp = panel.PowerUp;

            if (GameManager.Instance.GetCoinShop() < powerUp.BuyCost) return;
            if (PowerUpManager.Instance.IsFull) return;
            if (!PowerUpManager.Instance.TryEquip(powerUp)) return;

            GameManager.Instance.TrySpendCoinShop(powerUp.BuyCost);
            panel.MarkSelected();
        }
        else if (panel.Consumable != null)
        {
            BaseConsumable consumable = panel.Consumable;

            if (GameManager.Instance.GetCoinShop() < consumable.BuyCost) return;
            if (ConsumableManager.Instance.IsFull) return;
            if (!ConsumableManager.Instance.TryAdd(consumable)) return;

            GameManager.Instance.TrySpendCoinShop(consumable.BuyCost);
            panel.MarkSelected();
        }
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
