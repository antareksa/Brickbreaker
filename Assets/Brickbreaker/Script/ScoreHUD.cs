using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHUD : BaseHUD
{
    public TMP_Text ScoreText;
    public TMP_Text WaveText;
    public Button SkipWaveButton;
    public Button SkipShotButton;

    protected override void Start()
    {
        base.Start();

        SkipWaveButton.onClick.AddListener(OnSkipWaveButtonClicked);
        SkipShotButton.onClick.AddListener(OnSkipShotButtonClicked);

        GameManager.Instance.OnScoreChanged.AddListener(HandleScoreChanged);
        GameManager.Instance.OnWaveChanged.AddListener(HandleWaveChanged);

        // Prime the display with the current values -- a subscription only fires on future
        // changes, not the value that already existed before this HUD subscribed.
        HandleScoreChanged(GameManager.Instance.GetScore());
        HandleWaveChanged(GameManager.Instance.GetWave());
    }

    private void HandleScoreChanged(int score)
    {
        ScoreText.text = score.ToString();
    }

    private void HandleWaveChanged(int wave)
    {
        WaveText.text = (wave + 1).ToString();
    }

    private void OnSkipWaveButtonClicked()
    {
        GameManager.Instance.LaunchManager.LauncherController.SkipWave();
    }

    private void OnSkipShotButtonClicked()
    {
        GameManager.Instance.LaunchManager.LauncherController.SkipShot();
    }
}
