using UnityEngine;
using UnityEngine.UI;

public class GameOverHUD : BaseHUD
{
    public GameObject PopupRoot;
    public Button RestartButton;
    public Button ExitButton;
    public MainMenuHUD MainMenuHUD;

    protected override void Start()
    {
        base.Start();

        GameManager.Instance.BrickManager.OnGameOver.AddListener(HandleGameOver);
        RestartButton.onClick.AddListener(OnRestartButtonClicked);
        ExitButton.onClick.AddListener(OnExitButtonClicked);

        PopupRoot.SetActive(false);
    }

    private void HandleGameOver()
    {
        PopupRoot.SetActive(true);
    }

    private void OnRestartButtonClicked()
    {
        PopupRoot.SetActive(false);
        GameManager.Instance.BrickManager.RestartGame();
    }

    // Exit just opens the main menu for now -- not an application quit.
    private void OnExitButtonClicked()
    {
        PopupRoot.SetActive(false);
        MainMenuHUD.Open();
    }
}
