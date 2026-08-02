using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Slider that fills UP as the boss's current phase takes damage -- reverse of a normal health
// bar (which drains down). Value = damage dealt so far this phase, Max = that phase's total HP,
// so a full bar means the phase is defeated. On a phase change it animates back down to empty
// over ResetDuration instead of snapping instantly.
public class BossHUD : BaseHUD
{
    public Slider ProgressBar;
    public float ResetDuration = 0.3f;

    private Coroutine _resetRoutine;

    protected override void Start()
    {
        base.Start();

        BossManager bossManager = GameManager.Instance.BossManager;
        bossManager.OnBossDamaged.AddListener(RefreshProgress);
        bossManager.OnPhaseChanged.AddListener(HandlePhaseChanged);

        RefreshProgress();
    }

    private void HandlePhaseChanged(int phaseIndex)
    {
        BossManager bossManager = GameManager.Instance.BossManager;
        ProgressBar.maxValue = bossManager.CurrentPhaseMaxHP;

        if (_resetRoutine != null) StopCoroutine(_resetRoutine);
        _resetRoutine = StartCoroutine(AnimateResetToZero());
    }

    private IEnumerator AnimateResetToZero()
    {
        float startValue = ProgressBar.value;
        float elapsed = 0f;

        while (elapsed < ResetDuration)
        {
            elapsed += Time.deltaTime;
            ProgressBar.value = Mathf.Lerp(startValue, 0f, elapsed / ResetDuration);
            yield return null;
        }

        ProgressBar.value = 0f;
    }

    private void RefreshProgress()
    {
        BossManager bossManager = GameManager.Instance.BossManager;
        float maxHp = bossManager.CurrentPhaseMaxHP;

        ProgressBar.maxValue = maxHp;
        ProgressBar.value = maxHp - bossManager.CurrentPhaseHP;
    }
}
