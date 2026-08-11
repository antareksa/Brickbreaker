using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUniqueSkillHUD : BaseHUD
{
    public Slider SkillPointBar;
    public TMP_Text LevelSkillText;
    public TMP_Text SkillBarValueText;

    protected override void Start()
    {
        base.Start();

        RefreshSkillLevelDisplay();

        GameManager.Instance.SkillManager.OnSkillPointChanged.AddListener(HandleSkillPointChanged);
        GameManager.Instance.OnWaveChanged.AddListener(HandleWaveChanged);

        // Prime the display with the current values -- a subscription only fires on future
        // changes, not the value that already existed before this HUD subscribed.
        HandleSkillPointChanged(GameManager.Instance.SkillManager.GetCurrenSkillPoint());
    }

    private void HandleWaveChanged(int wave)
    {
        RefreshSkillLevelDisplay();
    }

    // Skill level (and the charge meter's max) is wave-derived now, so both need refreshing
    // whenever the wave advances -- not just once at Start.
    private void RefreshSkillLevelDisplay()
    {
        SkillPointBar.maxValue = GameManager.Instance.SkillManager.GetSkillPointNeeded();
        LevelSkillText.text = "Lvl." + GameManager.Instance.SkillManager.ActiveSkill.CurrentLevel;
    }

    private void HandleSkillPointChanged(float skillPoint)
    {
        SkillPointBar.value = skillPoint;

        float percentage = SkillPointBar.maxValue > 0f ? skillPoint / SkillPointBar.maxValue * 100f : 0f;
        SkillBarValueText.text = percentage.ToString("F2") + "%";
    }
}
