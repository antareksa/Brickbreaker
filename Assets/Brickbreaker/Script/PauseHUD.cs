using UnityEngine;
using UnityEngine.UI;

public class PauseHUD : BaseHUD
{
    public GameObject PopupRoot;
    public Button ResumeButton;
    public Button RestartButton;
    public Button ExitButton;
    public MainMenuHUD MainMenuHUD;

    protected override void Start()
    {
        base.Start();

        ResumeButton.onClick.AddListener(() => PopupRoot.SetActive(false));
        RestartButton.onClick.AddListener(OnRestartButtonClicked);
        ExitButton.onClick.AddListener(OnExitButtonClicked);

        PopupRoot.SetActive(false);
    }

    private void HandlePause()
    {
        PopupRoot.SetActive(true);
    }

    private void OnRestartButtonClicked()
    {
        PopupRoot.SetActive(false);
        GameManager.Instance.BrickManager.RestartGame();
    }

    private void OnExitButtonClicked()
    {
        PopupRoot.SetActive(false);
        MainMenuHUD.Open();
    }
}
