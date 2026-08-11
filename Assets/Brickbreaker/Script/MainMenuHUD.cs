using UnityEngine;
using UnityEngine.UI;

public class MainMenuHUD : BaseHUD
{
    public GameObject PopupRoot;
    public Button StartButton;
    public Button UpgradeButton;
    public UpgradeHUD UpgradeHUD;

    protected override void Start()
    {
        base.Start();

        StartButton.onClick.AddListener(HandleStartClicked);
        UpgradeButton.onClick.AddListener(HandleUpgradeClicked);

        Open();
    }

    public void Open()
    {
        PopupRoot.SetActive(true);
    }

    public void Close()
    {
        PopupRoot.SetActive(false);
    }

    private void HandleStartClicked()
    {
        Close();
        GameManager.Instance.BrickManager.RestartGame();
    }

    private void HandleUpgradeClicked()
    {
        UpgradeHUD.Open();
    }
}
