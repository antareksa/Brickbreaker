using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

// Lives on the boss GameObject itself and owns everything about its presentation: the phase
// GameObjects (each pre-placed/scaled by hand in the Scene view) and the animations played on
// whichever one is currently active. BossManager drives this directly -- it decides *when* a hit
// or phase change happens, this just knows *how* to show it.
public class BossController : MonoBehaviour
{
    public List<GameObject> Phases;

    // HP = BasePhaseHP[phaseIndex] x bossScalar(bossNumber) x GlobalDifficultyMultiplier (see
    // BossManager.GetPhaseHP). Indexed by phase, so each phase can have its own HP shape instead
    // of one flat value for the whole boss.
    public List<float> BasePhaseHP;

    [Header("Animation Names")]
    public string IdleAnimationName;
    public string HitAnimationName;
    public string DefeatAnimationName;

    // Where boss-hit VFX should play (e.g. for brick-destroy attacks -- see
    // BrickManager.AttackPowerToBoss).
    public Transform BossHitPoint;

    // Shown once the boss is fully defeated -- hidden the rest of the time, same as any phase
    // that hasn't been reached yet. Never auto-hidden: FinalBossController owns it from the
    // moment PlayDefeat shows it until the player ends it.
    public GameObject FinalScene;

    // Every boss has one -- the end scene is always player-driven, never a timed beat.
    public FinalBossController FinalBossController;

    private int _currentIndexPhases;

    private SkeletonAnimation CurrentSkeleton => Phases[_currentIndexPhases].GetComponent<SkeletonAnimation>();

    private void Start()
    {
        foreach (GameObject p in Phases)
        {
            p.SetActive(false);
        }

        Phases[0].SetActive(true);
        PlayIdle();

        if (FinalScene != null)
        {
            FinalScene.SetActive(false);
        }
    }

    public void SetBossPhase(int phase)
    {
        Phases[_currentIndexPhases].SetActive(false);
        Phases[phase].SetActive(true);
        _currentIndexPhases = phase;

        PlayIdle();
    }

    public void PlayIdle()
    {
        CurrentSkeleton.AnimationState.SetAnimation(0, IdleAnimationName, true);
    }

    public void PlayHit()
    {
        CurrentSkeleton.AnimationState.SetAnimation(0, HitAnimationName, false);
        CurrentSkeleton.AnimationState.AddAnimation(0, IdleAnimationName, true, 0f);
    }

    public void PlayDefeat()
    {
        CurrentSkeleton.AnimationState.SetAnimation(0, DefeatAnimationName, false);

        if (FinalScene != null)
        {
            FinalScene.SetActive(true);
        }

        // Must come AFTER the SetActive above -- SkeletonGraphic.AnimationState is null while its
        // GameObject is still inactive, so starting the playlist first would throw.
        if (FinalBossController != null)
        {
            FinalBossController.StartFinalBoss();
        }
    }
}
