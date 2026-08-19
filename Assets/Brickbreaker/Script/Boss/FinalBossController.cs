using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalBossController : MonoBehaviour
{
    public SkeletonGraphic SkeletonGraphic;
    public List<FinalBossSceneAnimationPlaylist> Animations;
    public float MaxProgress;
    public float MinProgressToTriggerFinalShot;
    public bool AutoTriggerFinalShot;
    
    [Header("UI")]
    public Slider MaxProgressSlider;
    public Button FinalShotButton;
    public Button FastButton;
    public Button MediumButton;
    public Button SlowButton;
    public Button PlayAgainButton;
    public Button EndButton;
    public Transform BottomButtonContainer;

    [Header("Audio")]
    public AudioSource FinalBossAudioSource;
    public List<AudioClip> LoopAudioClips;
    public List<AudioClip> FinalShotAudioClips;

    private FinalBossSceneAnimationPlaylist _currentAnimations;
    private float _currentSpeed;
    private float _currentProgress;
    private int _currentPlaylistIndex = -1;
    private bool _isPlaying;

    private void Start()
    {
        FinalShotButton.onClick.AddListener(() => StartFinalShot());

        FastButton.onClick.AddListener(() => SetSpeed(1.5f));
        MediumButton.onClick.AddListener(() => SetSpeed(1f));
        SlowButton.onClick.AddListener(() => SetSpeed(0.5f));

        PlayAgainButton.onClick.AddListener(() => RestartFinalBoss());
        EndButton.onClick.AddListener(() => EndFinalBoss());
    }

    private void Update()
    {
        if(_isPlaying)
        {
            _currentProgress += Time.deltaTime * _currentSpeed;
            MaxProgressSlider.value = _currentProgress;
            if(_currentProgress > MaxProgress)
            {
                if(AutoTriggerFinalShot) StartFinalShot();
            }
        }

        FinalShotButton.gameObject.SetActive(_isPlaying && (_currentProgress >= MinProgressToTriggerFinalShot));
    }

    public void StartFinalBoss()
    {
        _currentAnimations = GetNextAnimations();
        if (_currentAnimations == null)
        {
            EndFinalBoss();
            return;
        }

        SkeletonGraphic.AnimationState.SetAnimation(0, _currentAnimations.LoopAnimation, true);
        SetSpeed(0.5f);

        _currentProgress = 0;
        _isPlaying = true;
        MaxProgressSlider.maxValue = MaxProgress;

        BottomButtonContainer.gameObject.SetActive(false);

        FinalBossAudioSource.clip = LoopAudioClips[UnityEngine.Random.Range(0, LoopAudioClips.Count)];
        FinalBossAudioSource.loop = true;
        FinalBossAudioSource.Play();
    }

    public void RestartFinalBoss()
    {
        if (_currentAnimations == null)
        {
            EndFinalBoss();
            return;
        }

        SkeletonGraphic.AnimationState.SetAnimation(0, _currentAnimations.LoopAnimation, true);
        SetSpeed(1);

        _currentProgress = 0;
        _isPlaying = true;
        MaxProgressSlider.maxValue = MaxProgress;

        BottomButtonContainer.gameObject.SetActive(false);

        FinalBossAudioSource.clip = LoopAudioClips[UnityEngine.Random.Range(0, LoopAudioClips.Count)];
        FinalBossAudioSource.loop = true;
        FinalBossAudioSource.Play();
    }

    public void SetSpeed(float speed)
    {
        SkeletonGraphic.timeScale = speed;
        _currentSpeed = speed;
    }

    private FinalBossSceneAnimationPlaylist GetNextAnimations()
    {
        if (Animations == null || Animations.Count == 0) return null;

        _currentPlaylistIndex = (_currentPlaylistIndex + 1) % Animations.Count;
        return Animations[_currentPlaylistIndex];
    }

    private void StartFinalShot()
    {
        _isPlaying = false;
        TrackEntry finalShotEntry = SkeletonGraphic.AnimationState.SetAnimation(0, _currentAnimations.FinalShotAnimation, false);

        FinalBossAudioSource.clip = FinalShotAudioClips[UnityEngine.Random.Range(0, FinalShotAudioClips.Count)];
        FinalBossAudioSource.loop = false;
        FinalBossAudioSource.Play();

        finalShotEntry.Complete += HandleFinalShotComplete;
    }

    private void HandleFinalShotComplete(TrackEntry trackEntry)
    {
        trackEntry.Complete -= HandleFinalShotComplete;
        SkeletonGraphic.AnimationState.SetAnimation(0, _currentAnimations.AfterFinalShotLoopAnimation, true);
        BottomButtonContainer.gameObject.SetActive(true);
    }

    public void EndFinalBoss()
    {
        _isPlaying = false;
        SkeletonGraphic.AnimationState.ClearTracks();
        SkeletonGraphic.Skeleton.SetToSetupPose();

        gameObject.SetActive(false);

        GameManager.Instance.BossManager.Continue();
    }
}

[Serializable]
public class FinalBossSceneAnimationPlaylist
{
    public string LoopAnimation;
    public string FinalShotAnimation;
    public string AfterFinalShotLoopAnimation;
}
