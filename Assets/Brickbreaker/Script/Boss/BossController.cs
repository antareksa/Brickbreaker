using System.Collections;
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
    public GameObject AttackingPhase;
    public string AttackingAnim;

    // Named Spine event authored on AttackingAnim's timeline -- shake fires when playback
    // actually crosses this keyframe, not when the animation starts, so it lines up with the
    // attack's real impact instead of an estimated delay.
    [Header("Attack Shake")]
    public string AttackShakeEventName = "breath_1";
    public float AttackShakeDuration = 0.25f;
    public float AttackShakeMagnitude = 0.15f;

    private bool _attackShakeEventSubscribed;

    // HP = BasePhaseHP[phaseIndex] x bossScalar(bossNumber) x GlobalDifficultyMultiplier (see
    // BossManager.GetPhaseHP). Indexed by phase, so each phase can have its own HP shape instead
    // of one flat value for the whole boss.
    public List<float> BasePhaseHP;

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
    private SkeletonAnimation AttackingSkeleton => AttackingPhase.GetComponent<SkeletonAnimation>();

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

    // Returns the routine itself (rather than firing-and-forgetting via StartCoroutine) so
    // callers can yield on it -- e.g. BrickManager waits for the attack to actually land before
    // applying the player's HP loss, instead of both happening the same frame.
    public IEnumerator PlayAttack()
    {
        if (_currentBossAnimation == null) yield break;
        if (string.IsNullOrEmpty(AttackingAnim) || AttackingPhase == null) yield break;

        Phases[_currentIndexPhases].SetActive(false);
        AttackingPhase.SetActive(true);

        // SkeletonAnimation only finishes initializing (AnimationState becomes usable) once its
        // GameObject has actually been active -- subscribe here, after SetActive(true), rather
        // than in Start(), and only once since AttackingSkeleton is the same instance every call.
        if (!_attackShakeEventSubscribed)
        {
            AttackingSkeleton.AnimationState.Event += HandleAttackAnimationEvent;
            _attackShakeEventSubscribed = true;
        }

        AttackingSkeleton.AnimationState.SetAnimation(0, AttackingAnim, false);

        // Wait for the actual attack animation length instead of swapping back the same frame --
        // reads straight off the Spine data so it stays in sync if AttackingAnim's clip changes.
        Spine.Animation animation = AttackingSkeleton.Skeleton.Data.FindAnimation(AttackingAnim);
        float duration = animation != null ? animation.Duration : 0f;
        yield return new WaitForSeconds(duration);

        Phases[_currentIndexPhases].SetActive(true);
        AttackingPhase.SetActive(false);
        CurrentSkeleton.AnimationState.SetAnimation(0, _currentBossAnimation.IdleAnimationName, true);
    }

    private void HandleAttackAnimationEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name != AttackShakeEventName) return;
        if (CameraShake.Instance == null) return;

        CameraShake.Instance.Shake(AttackShakeDuration, AttackShakeMagnitude);
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
