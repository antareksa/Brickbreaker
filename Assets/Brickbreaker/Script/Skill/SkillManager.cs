using UnityEngine;
using UnityEngine.Events;

public class SkillManager : MonoBehaviour
{
    public BaseSkillEffect ActiveSkill;

    public UnityEvent OnActivatedSkill = new UnityEvent();
    public UnityEvent<float> OnSkillPointChanged = new UnityEvent<float>();

    private float _skillPoint;

    public float GetCurrenSkillPoint()
    {
        return _skillPoint;
    }

    // Read live off ActiveSkill rather than cached -- SkillPointNeeded depends on CurrentLevel,
    // which depends on the current wave, so it changes as waves advance rather than being fixed
    // once at Start.
    public float GetSkillPointNeeded()
    {
        return ActiveSkill.SkillPointNeeded;
    }

    // Only accumulates now -- no longer activates immediately on reaching the threshold. A ball
    // hit lands mid-shot (Shooting state), and the skill should only actually go off once the
    // player is back to Aiming, so activation is a separate step callers trigger explicitly.
    public void AddSkillPoint(float skillPoint)
    {
        _skillPoint += skillPoint;
        OnSkillPointChanged?.Invoke(_skillPoint);
    }

    public bool IsSkillPoint()
    {
        return _skillPoint >= ActiveSkill.SkillPointNeeded;
    }

    public void ResetSkillPoint()
    {
        _skillPoint = 0;
        OnSkillPointChanged?.Invoke(_skillPoint);
    }

    // Call once the current shot has fully finished (before the next wave spawns) -- activates
    // the skill only if enough points have been banked, then resets the meter.
    public void TryActivateSkill()
    {
        if (!IsSkillPoint()) return;

        _skillPoint = 0;
        OnSkillPointChanged?.Invoke(_skillPoint);
        OnActivatedSkill?.Invoke();

        // Skill activation no longer touches the boss -- boss damage now comes purely from
        // brick destruction (BrickManager.AttackPowerToBoss), so this just runs the skill's own
        // brick-clearing effect.
        ActiveSkill?.Activate();
    }
}
