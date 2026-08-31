using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverHUD : BaseHUD
{
    public GameObject PopupRoot;
    public Button RestartButton;
    public Button ExitButton;
    public MainMenuHUD MainMenuHUD;

    [Header("Summary")]
    public TMP_Text TotalScoreText;
    public TMP_Text TotalWaveText;
    public TMP_Text DurationText;
    public TMP_Text TotalBricksDestroyedText;

    [Header("Power Up List")]
    public PowerUpPanel PowerUpPanelPrefab;
    public Transform PowerUpPanelContainer;

    private readonly List<PowerUpPanel> _powerUpPanels = new List<PowerUpPanel>();

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

        TotalScoreText.text = GameManager.Instance.GetScore().ToString();
        // +1 to match ScoreHUD's own wave display -- GetWave() is 0-indexed internally.
        TotalWaveText.text = (GameManager.Instance.GetWave() + 1).ToString();
        DurationText.text = FormatDuration(GameManager.Instance.GetPlayDuration());
        TotalBricksDestroyedText.text = GameManager.Instance.GetTotalBricksDestroyed().ToString();

        RefreshPowerUpPanels();
    }

    private static string FormatDuration(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    // Rebuilt from scratch each time the screen opens -- same approach as PowerUpHUD's own list,
    // just without a sell button since there's no Shop to sell into here.
    private void RefreshPowerUpPanels()
    {
        foreach (PowerUpPanel panel in _powerUpPanels)
        {
            Destroy(panel.gameObject);
        }
        _powerUpPanels.Clear();

        foreach (BasePowerUp powerUp in PowerUpManager.Instance.GetEquipped())
        {
            PowerUpPanel panel = Instantiate(PowerUpPanelPrefab, PowerUpPanelContainer);
            panel.SetInfo(powerUp);
            panel.SetSellButtonVisible(false);
            _powerUpPanels.Add(panel);
        }
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
