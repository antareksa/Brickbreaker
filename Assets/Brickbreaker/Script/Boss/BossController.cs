using System.Collections.Generic;
using NUnit.Framework;
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
    public string AttackAnimationName;
    public string DefeatAnimationName;

    [Header("Audio")]
    public AudioSource SFXSource;
    public List<AudioClip> BossAttackedSFX;

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

    private List<AudioClip> _bossAttackedPlaylistSFX;

    private int _playlistIndex;

    private BossAnimation _currentBossAnimation;

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

        _currentBossAnimation = Phases[_currentIndexPhases].GetComponent<BossAnimation>();

        PlayIdle();
    }

    public void PlayIdle()
    {
        if (_currentBossAnimation == null) return;
        CurrentSkeleton.AnimationState.SetAnimation(0, _currentBossAnimation.IdleAnimationName, true);
    }

    public void PlayHit()
    {
        if (_currentBossAnimation == null) return;
        CurrentSkeleton.AnimationState.SetAnimation(0, _currentBossAnimation.HitAnimationName, false);
        CurrentSkeleton.AnimationState.AddAnimation(0, _currentBossAnimation.IdleAnimationName, true, 0f);
    }

    public void PlayAttack()
    {
        if (_currentBossAnimation == null) return;
        CurrentSkeleton.AnimationState.SetAnimation(0, _currentBossAnimation.AttackAnimationName, false);
        CurrentSkeleton.AnimationState.AddAnimation(0, _currentBossAnimation.IdleAnimationName, true, 0f);
    }

    public void PlayDefeat()
    {
        if (_currentBossAnimation != null) CurrentSkeleton.AnimationState.SetAnimation(0, _currentBossAnimation.DefeatAnimationName, false);

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

    public void PlayAttackedSFX()
    {
        // If playlist is empty or exhausted, reshuffle
        if (_bossAttackedPlaylistSFX == null || _bossAttackedPlaylistSFX.Count == 0
            || _playlistIndex >= _bossAttackedPlaylistSFX.Count)
        {
            RandomizeAttackedSFX();
        }

        AudioClip clip = _bossAttackedPlaylistSFX[_playlistIndex];
        _playlistIndex++;

        if (clip != null)
            SFXSource.PlayOneShot(clip);
    }

    private void RandomizeAttackedSFX()
    {
        // Copy the source list so we don't mutate the original
        _bossAttackedPlaylistSFX = new List<AudioClip>(BossAttackedSFX);

        // Fisher-Yates shuffle
        for (int i = _bossAttackedPlaylistSFX.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_bossAttackedPlaylistSFX[i], _bossAttackedPlaylistSFX[j]) =
                (_bossAttackedPlaylistSFX[j], _bossAttackedPlaylistSFX[i]);
        }

        _playlistIndex = 0;
    }
}
