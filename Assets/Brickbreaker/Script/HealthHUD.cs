using System.Collections.Generic;
using UnityEngine;

// One HealthPlayerPanel per MaxHP point -- always MaxHP panels total (not just current HP), each
// individually shown/hidden based on whether the player currently has that many HP. Panels are
// spawned once at Start rather than rebuilt every HP change, since nothing in the run currently
// changes MaxHP itself.
public class HealthHUD : BaseHUD
{
    public HealthPlayerPanel PanelPrefab;
    public Transform PanelContainer;

    private readonly List<HealthPlayerPanel> _panels = new List<HealthPlayerPanel>();

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < GameManager.Instance.MaxHP; i++)
        {
            HealthPlayerPanel panel = Instantiate(PanelPrefab, PanelContainer);
            _panels.Add(panel);
        }

        GameManager.Instance.OnPlayerChanceCountChanged.AddListener(RefreshPanels);

        // Prime the display with whatever HP already exists -- a subscription only fires on
        // future changes, not the state that already existed before this HUD subscribed.
        RefreshPanels(GameManager.Instance.GetPlayerChanceCount());
    }

    private void RefreshPanels(int currentHp)
    {
        for (int i = 0; i < _panels.Count; i++)
        {
            _panels[i].SetActive(i < currentHp);
        }
    }
}
