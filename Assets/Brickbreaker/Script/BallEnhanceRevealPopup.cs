using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shared modal opened after buying ANY Ball Enhance pack -- shop open -> 2 sealed pack offers ->
// buy one -> this popup lists that pack's 4 revealed options -> pick up to MaxPicks -> close.
// Only one pack is ever bought/revealed at a time, so ShopHUD owns a single instance of this
// rather than one per pack offer.
public class BallEnhanceRevealPopup : MonoBehaviour
{
    public GameObject PopupRoot;
    public BallEnhancePanel PanelPrefab;
    public Transform PanelContainer;
    public Button CloseButton;
    public int MaxPicks = 2;

    private readonly List<BallEnhancePanel> _panels = new List<BallEnhancePanel>();
    private int _picksMade;

    private void Awake()
    {
        CloseButton.onClick.AddListener(Close);
        PopupRoot.SetActive(false);
    }

    public void Open(List<(BallEnhanceType Type, BallEnhanceAxis Axis)> options)
    {
        foreach (BallEnhancePanel panel in _panels)
        {
            Destroy(panel.gameObject);
        }
        _panels.Clear();
        _picksMade = 0;

        foreach ((BallEnhanceType type, BallEnhanceAxis axis) in options)
        {
            BallEnhancePanel panel = Instantiate(PanelPrefab, PanelContainer);
            panel.SetInfo(type, axis);
            panel.SelectButton.onClick.AddListener(() => HandlePicked(panel));
            _panels.Add(panel);
        }

        RefreshLocking();
        PopupRoot.SetActive(true);
    }

    private void HandlePicked(BallEnhancePanel panel)
    {
        if (_picksMade >= MaxPicks) return;
        if (!BallEnhanceManager.Instance.TryUpgrade(panel.Type, panel.Axis)) return;

        panel.MarkPicked();
        _picksMade++;

        RefreshLocking();
    }

    private void RefreshLocking()
    {
        bool full = _picksMade >= MaxPicks;
        foreach (BallEnhancePanel panel in _panels)
        {
            panel.SetLocked(full);
        }
    }

    public void Close()
    {
        PopupRoot.SetActive(false);
    }
}
