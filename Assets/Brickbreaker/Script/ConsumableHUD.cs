using System.Collections.Generic;
using UnityEngine;

// Panels are instantiated from PanelPrefab, one per held Consumable -- same approach as
// PowerUpHUD's rows.
public class ConsumableHUD : BaseHUD
{
    public ConsumablePanel PanelPrefab;
    public Transform PanelContainer;

    private readonly List<ConsumablePanel> _panels = new List<ConsumablePanel>();

    protected override void Start()
    {
        base.Start();

        ConsumableManager.Instance.OnConsumablesChanged.AddListener(RefreshPanels);

        // Prime the display with whatever's already held -- a subscription only fires on future
        // changes, not the state that already existed before this HUD subscribed.
        RefreshPanels();
    }

    protected override void OnEnterState(GameState state)
    {
        foreach (ConsumablePanel panel in _panels)
        {
            panel.RefreshUsability(state);
        }
    }

    // Rebuilt from scratch every change -- simplest way to keep panel count in sync with however
    // many Consumables are currently held.
    private void RefreshPanels()
    {
        foreach (ConsumablePanel panel in _panels)
        {
            Destroy(panel.gameObject);
        }
        _panels.Clear();

        // Read live off the state machine rather than BaseHUD's own CurrentState mirror -- that
        // mirror only updates on the NEXT state change after Start, so it'd be stale for this very
        // first call.
        GameState currentState = GameManager.Instance.StateMachine.CurrentState;

        foreach (BaseConsumable consumable in ConsumableManager.Instance.GetHeld())
        {
            ConsumablePanel panel = Instantiate(PanelPrefab, PanelContainer);
            panel.SetInfo(consumable);
            panel.RefreshUsability(currentState);
            _panels.Add(panel);
        }
    }
}
