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

        // Read live off the state machine (not BaseHUD's own CurrentState mirror) -- that mirror
        // only updates on the NEXT state change after Start, so it'd be stale for this priming call.
        RefreshSkipShotButton(GameManager.Instance.StateMachine.CurrentState);
        RefreshSkipWaveButton(GameManager.Instance.StateMachine.CurrentState);
    }

    // Only meaningful to skip while balls are actually in flight.
    protected override void OnEnterState(GameState state)
    {
        RefreshSkipShotButton(state);
        RefreshSkipWaveButton(state);
    }

    private void RefreshSkipShotButton(GameState state)
    {
        SkipShotButton.interactable = state == GameState.Shooting;
    }

    // SkipWave is only meant for skipping BEFORE a shot happens (see LauncherControllerV2.SkipWave).
    // Outside of Aiming -- e.g. during AdvanceWave's descend/bottom-row-clear delay -- clicking it
    // would fire a second OnShotFinished while HandleShotFinishedRoutine is still mid-flight,
    // shifting bricks down again before a pending bottom-row hit ever resolves.
    private void RefreshSkipWaveButton(GameState state)
    {
        SkipWaveButton.interactable = state == GameState.Aiming;
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
