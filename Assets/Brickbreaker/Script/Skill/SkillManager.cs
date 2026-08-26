using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillManager : MonoBehaviour
{
    public List<BaseSkillEffect> ListSkillChoice;

    public UnityEvent OnActivatedSkill = new UnityEvent();
    public UnityEvent<float> OnSkillPointChanged = new UnityEvent<float>();

    public GameObject SkillPopUp;
    public Animator SkillPopUpAnimator;

    private float _skillPoint;
    private int _skillIndex = 0;


    private void Awake()
    {
        if(PlayerPrefs.HasKey("PLAYER_SKILL_INDEX"))
        {
            _skillIndex = PlayerPrefs.GetInt("PLAYER_SKILL_INDEX");
        }
        else
        {
            _skillIndex = 0;
            ChangeSkill(_skillIndex);
        }
    }

    private BaseSkillEffect _activeSkill
    {
        get { return ListSkillChoice[_skillIndex]; }
    }

    public float GetCurrenSkillPoint()
    {
        return _skillPoint;
    }

    public int GetSkillIndex()
    {
        return _skillIndex;
    }

    // Read live off ActiveSkill rather than cached -- SkillPointNeeded depends on CurrentLevel,
    // which depends on the current wave, so it changes as waves advance rather than being fixed
    // once at Start.
    public float GetSkillPointNeeded()
    {
        return _activeSkill.SkillPointNeeded;
    }

    public int GetSkillLevel()
    {
        return _activeSkill.CurrentLevel;
    }

    public Sprite GetSkillIcon()
    {
        return _activeSkill.SkillIcon;
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
        return _skillPoint >= _activeSkill.SkillPointNeeded;
    }

    public void ResetSkillPoint()
    {
        _skillPoint = 0;
        OnSkillPointChanged?.Invoke(_skillPoint);
    }

    public void ChangeSkill(int index)
    {
        if(index >= 0 &&  index < ListSkillChoice.Count)
        {
            _skillIndex = index;
            Debug.Log("[SKILLMANAGER] PICK " + index);
        }
        else
        {
            _skillIndex = 0;
            Debug.Log("[SKILLMANAGER] overflow PICK");
        }

        PlayerPrefs.SetInt("PLAYER_SKILL_INDEX", _skillIndex);
    }

    // Call once the current shot has fully finished (before the next wave spawns) -- activates
    // the skill only if enough points have been banked, then resets the meter.
    public void TryActivateSkill()
    {
        if (!IsSkillPoint()) return;

        StartCoroutine(TryActivateSkillRoutine());
    }

    private IEnumerator TryActivateSkillRoutine()
    {
        SkillPopUp.gameObject.SetActive(true);
        SkillPopUpAnimator.SetTrigger("Show");

        yield return new WaitForSeconds(1.25f);

        SkillPopUp.gameObject.SetActive(false);

        // Normally resets to 0, but a PowerUp can bank a flat leftover instead (defaults to 0,
        // so behavior is unchanged with nothing equipped).
        _skillPoint = PowerUpManager.Instance != null ? PowerUpManager.Instance.GetTotalSkillChargeLeftover() : 0f;
        OnSkillPointChanged?.Invoke(_skillPoint);
        OnActivatedSkill?.Invoke();

        if (PowerUpManager.Instance != null)
        {
            int bonusHp = PowerUpManager.Instance.GetTotalBonusHpOnSkillTrigger();
            if (bonusHp > 0)
            {
                GameManager.Instance.SetPlayerChanceCount(GameManager.Instance.GetPlayerChanceCount() + bonusHp);
            }
        }

        // Skill activation no longer touches the boss -- boss damage now comes purely from
        // brick destruction (BrickManager.AttackPowerToBoss), so this just runs the skill's own
        // brick-clearing effect. Extra triggers from PowerUps just replay Activate() more times.
        int extraTriggers = PowerUpManager.Instance != null ? PowerUpManager.Instance.GetTotalBonusSkillTriggers() : 0;
        for (int i = 0; i < 1 + extraTriggers; i++)
        {
            _activeSkill?.Activate();
        }
    }
}
