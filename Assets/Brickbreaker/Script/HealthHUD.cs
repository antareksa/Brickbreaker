using System.Collections.Generic;
using UnityEngine;

// One HealthPlayerPanel per effective max HP point (GameManager.MaxHP plus the ExtraChance
// trait bonus) -- always that many panels total (not just current HP), each individually shown/
// hidden based on whether the player currently has that many HP. Panel count is re-synced to
// GetMaxHp() on every refresh (not just once at Start) so buying/resetting the ExtraChance trait
// mid-session grows/shrinks the bar immediately instead of only after a scene reload.
public class HealthHUD : BaseHUD
{
    public HealthPlayerPanel PanelPrefab;
    public Transform PanelContainer;

    private readonly List<HealthPlayerPanel> _panels = new List<HealthPlayerPanel>();

    protected override void Start()
    {
        base.Start();

        GameManager.Instance.OnPlayerChanceCountChanged.AddListener(RefreshPanels);

        // Prime the display with whatever HP already exists -- a subscription only fires on
        // future changes, not the state that already existed before this HUD subscribed.
        RefreshPanels(GameManager.Instance.GetPlayerChanceCount());
    }

    private void RefreshPanels(int currentHp)
    {
        int maxHp = GameManager.Instance.GetMaxHp();

        while (_panels.Count < maxHp)
        {
            _panels.Add(Instantiate(PanelPrefab, PanelContainer));
        }

        while (_panels.Count > maxHp)
        {
            HealthPlayerPanel last = _panels[_panels.Count - 1];
            _panels.RemoveAt(_panels.Count - 1);
            Destroy(last.gameObject);
        }

        for (int i = 0; i < _panels.Count; i++)
        {
            _panels[i].SetActive(i < currentHp);
        }
    }
}
