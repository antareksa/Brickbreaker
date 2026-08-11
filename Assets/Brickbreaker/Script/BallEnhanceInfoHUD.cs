using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Popup listing every Ball Enhance the player currently owns, opened from its own button --
// purely informational, unlike the Shop's pack flow where enhances are actually bought/picked.
public class BallEnhanceInfoHUD : BaseHUD
{
    public GameObject PopupRoot;
    public Button OpenButton;
    public Button CloseButton;
    public BallEnhanceInfoPanel PanelPrefab;
    public Transform PanelContainer;

    private readonly List<BallEnhanceInfoPanel> _panels = new List<BallEnhanceInfoPanel>();

    protected override void Start()
    {
        base.Start();

        OpenButton.onClick.AddListener(Open);
        CloseButton.onClick.AddListener(Close);

        PopupRoot.SetActive(false);
    }

    // Rebuilt on every open rather than kept in sync -- enhances can only change at the Shop, so
    // there's no need to listen for changes while this is closed.
    public void Open()
    {
        BuildPanels();
        PopupRoot.SetActive(true);
    }

    public void Close()
    {
        PopupRoot.SetActive(false);
    }

    private void BuildPanels()
    {
        foreach (BallEnhanceInfoPanel panel in _panels)
        {
            Destroy(panel.gameObject);
        }
        _panels.Clear();

        foreach ((BallEnhanceType type, BallEnhanceAxis axis) in BallEnhanceManager.Instance.GetOwned())
        {
            BallEnhanceInfoPanel panel = Instantiate(PanelPrefab, PanelContainer);
            panel.SetInfo(type, axis);
            _panels.Add(panel);
        }
    }
}
