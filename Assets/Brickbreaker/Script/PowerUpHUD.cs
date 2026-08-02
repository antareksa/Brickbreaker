using System.Collections.Generic;
using UnityEngine;

// Panels are instantiated from PanelPrefab, one per equipped PowerUp -- same approach as
// UpgradeHUD's rows, rather than a pre-placed fixed list of slots.
public class PowerUpHUD : BaseHUD
{
    public PowerUpPanel PanelPrefab;
    public Transform PanelContainer;

    private readonly List<PowerUpPanel> _panels = new List<PowerUpPanel>();

    protected override void Start()
    {
        base.Start();

        PowerUpManager.Instance.OnPowerUpsChanged.AddListener(RefreshPanels);

        // Prime the display with whatever's already equipped -- a subscription only fires on
        // future changes, not the state that already existed before this HUD subscribed.
        RefreshPanels();
    }

    // Rebuilt from scratch every change -- simplest way to keep panel count in sync with
    // however many PowerUps are currently equipped.
    private void RefreshPanels()
    {
        foreach (PowerUpPanel panel in _panels)
        {
            Destroy(panel.gameObject);
        }
        _panels.Clear();

        foreach (BasePowerUp powerUp in PowerUpManager.Instance.GetEquipped())
        {
            PowerUpPanel panel = Instantiate(PanelPrefab, PanelContainer);
            panel.SetInfo(powerUp);
            _panels.Add(panel);
        }
    }
}
