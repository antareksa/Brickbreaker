using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Rows are instantiated from RowPrefab, one per TraitManager.Traits entry -- so the row count
// always matches however many traits exist without hand-placing a slot per trait.
public class UpgradeHUD : BaseHUD
{
    public GameObject PopupRoot;
    public TMP_Text TokenText;
    public TraitRowPanel RowPrefab;
    public Transform RowContainer;
    public Button CloseButton;
    public Button ResetButton;

    private readonly List<TraitRowPanel> _rows = new List<TraitRowPanel>();

    protected override void Start()
    {
        base.Start();

        CloseButton.onClick.AddListener(Close);
        ResetButton.onClick.AddListener(HandleResetClicked);

        PopupRoot.SetActive(false);
    }

    public void Open()
    {
        PopupRoot.SetActive(true);
        BuildRows();
    }

    public void Close()
    {
        PopupRoot.SetActive(false);
    }

    // Rebuilt from scratch every open -- simplest way to keep row count in sync with however
    // many traits TraitManager.Traits currently has.
    private void BuildRows()
    {
        foreach (TraitRowPanel row in _rows)
        {
            Destroy(row.gameObject);
        }
        _rows.Clear();

        List<TraitDefinition> traits = TraitManager.Instance.Traits;
        for (int i = 0; i < traits.Count; i++)
        {
            TraitRowPanel row = Instantiate(RowPrefab, RowContainer);
            int rowIndex = i;
            row.UpgradeButton.onClick.AddListener(() => HandleUpgradeClicked(rowIndex));
            _rows.Add(row);
        }

        RefreshAll();
    }

    private void HandleUpgradeClicked(int rowIndex)
    {
        TraitType type = TraitManager.Instance.Traits[rowIndex].Type;
        TraitManager.Instance.UpgradeTrait(type);
        RefreshAll();
    }

    private void HandleResetClicked()
    {
        TraitManager.Instance.ResetAllTraits();
        RefreshAll();
    }

    private void RefreshAll()
    {
        TokenText.text = TraitManager.Instance.GetToken().ToString();

        List<TraitDefinition> traits = TraitManager.Instance.Traits;
        for (int i = 0; i < _rows.Count; i++)
        {
            _rows[i].SetInfo(traits[i]);
        }
    }
}
