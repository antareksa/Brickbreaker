using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Active for the whole game session from Start -- not tied to a specific wave. The boss doesn't
// occupy the brick field; it exists in parallel to the normal brick/wave/coin gameplay, which
// this class never touches. Only reachable through SkillManager routing Skill damage here.
// Owns boss state (which boss, phase, HP, defeated) and decides when transitions happen; each
// BossController is just the presentation (phase GameObjects + animations) for one boss,
// driven directly from here. Defeating any boss shows its final scene and hands control to that
// boss's FinalBossController; nothing is auto-hidden, and the next boss is only swapped in once
// the player ends that scene (Continue). Only the last boss's defeat is a real "IsDefeated".
public class BossManager : MonoBehaviour
{
    public List<BossController> Bosses;

    // The single live tuning knob (per the design doc) -- bump for "too easy" across every
    // phase/boss in one edit, drop for "too hard". basePhaseHP and the per-boss scalar are meant
    // to be set once and rarely touched; this is the one meant to move during playtesting.
    public float GlobalDifficultyMultiplier = 1f;

    // +20% per repeat boss by default (bossNumber is 1-indexed: 1 + (bossNumber - 1) * 0.2).
    public float BossScalarGrowthPerBoss = 0.2f;

    public UnityEvent OnAttackPlayer = new UnityEvent();
    public UnityEvent<int> OnPhaseChanged = new UnityEvent<int>();
    public UnityEvent OnBossDamaged = new UnityEvent();
    public UnityEvent OnBossDefeated = new UnityEvent();

    // Fired when a defeated boss's end scene goes up and when the player ends it (resume whatever
    // comes next -- the next boss, or the run being over).
    public UnityEvent OnAwaitingContinue = new UnityEvent();
    public UnityEvent OnContinued = new UnityEvent();

    public bool IsDefeated { get; private set; }
    public int CurrentPhaseIndex { get; private set; }
    public float CurrentPhaseHP { get; private set; }
    public int CurrentBossIndex { get; private set; }

    public BossController BossController => Bosses[CurrentBossIndex];

    // Recomputed live (not cached) so it always reflects the current phase's true max, even
    // after CurrentPhaseHP has been partially depleted by DamageBoss.
    public float CurrentPhaseMaxHP => GetPhaseHP(CurrentBossIndex, CurrentPhaseIndex);

    // HP = basePhaseHP[phaseIndex] x bossScalar(bossNumber) x GlobalDifficultyMultiplier.
    public float GetPhaseHP(int bossIndex, int phaseIndex)
    {
        return Bosses[bossIndex].BasePhaseHP[phaseIndex] * GetBossScalar(bossIndex) * GlobalDifficultyMultiplier;
    }

    // bossNumber is 1-indexed (bossIndex 0 -> bossNumber 1) -- first boss has no growth applied.
    private float GetBossScalar(int bossIndex)
    {
        int bossNumber = bossIndex + 1;
        return 1f + (bossNumber - 1) * BossScalarGrowthPerBoss;
    }

    // True from the moment any boss dies until Continue() resolves its end scene -- blocks
    // DamageBoss from re-triggering PlayDefeat if another hit lands during that window (IsDefeated
    // itself stays false for a mid-run boss since a next boss is still coming). Exposed publicly
    // so callers queuing up multiple hits (BrickManager) can tell a transition just started and
    // stop feeding it more hits.
    public bool IsTransitioning { get; private set; }

    // True from the moment a defeated boss's end scene appears until Continue() ends it.
    public bool IsAwaitingContinue { get; private set; }

    // No longer auto-starts here -- BrickManager.RestartGame() already calls ResetBoss() as part
    // of the MainMenuHUD Start / GameOverHUD Restart flow, same as brick spawning.

    public void ResetBoss()
    {
        CurrentBossIndex = 0;

        foreach (BossController boss in Bosses)
        {
            boss.gameObject.SetActive(false);

            // Hide the end scene explicitly -- restarting while it was still up (Continue never
            // pressed) would otherwise leave its own active flag set, so it'd reappear the moment
            // that boss's root is reactivated.
            if (boss.FinalScene != null) boss.FinalScene.SetActive(false);
        }

        ActivateCurrentBoss();
    }

    private void ActivateCurrentBoss()
    {
        IsDefeated = false;
        IsAwaitingContinue = false;
        IsTransitioning = false;
        CurrentPhaseIndex = 0;
        CurrentPhaseHP = GetPhaseHP(CurrentBossIndex, CurrentPhaseIndex);

        BossController.gameObject.SetActive(true);
        BossController.SetBossPhase(0);

        // Fires on both a full restart (ResetBoss) and swapping to the next boss -- either way
        // listeners like BossHUD need to know the phase/HP just reset, not just on a mid-fight
        // phase advance.
        OnPhaseChanged?.Invoke(CurrentPhaseIndex);
    }

    // Applies damage and the hit reaction only -- does NOT advance phase, start a defeat, or
    // swap the spine even if this empties the phase's HP. Call AdvancePhaseIfComplete() to
    // actually apply that outcome, whenever the caller wants it to visually happen (immediately,
    // or after its own delay).
    public void DamageBoss(int amount)
    {
        if (IsDefeated || IsTransitioning) return;

        CurrentPhaseHP -= amount;
        BossController.PlayHit();
        OnBossDamaged?.Invoke();
    }

    // True once the current phase's HP has been brought to 0 by DamageBoss but the resulting
    // phase/boss transition hasn't been applied yet.
    public bool IsPhaseComplete => CurrentPhaseHP <= 0;

    // Actually applies whatever DamageBoss's last hit triggered: advances to the next phase, or
    // -- if that next phase is the boss's LAST one -- hands off to BrickManager to spawn the
    // final-phase big brick instead of showing a skeleton. Kept separate from DamageBoss so a
    // delay can sit between the hit landing and the phase visually changing.
    public void AdvancePhaseIfComplete()
    {
        if (IsDefeated || IsTransitioning || !IsPhaseComplete) return;

        int lastPhaseIndex = BossController.Phases.Count - 1;
        if (CurrentPhaseIndex >= lastPhaseIndex) return;

        CurrentPhaseIndex++;

        // Reset BEFORE firing the event -- listeners (BossHUD) read CurrentPhaseHP/BossController
        // synchronously off this event, so it needs to already reflect the new phase.
        CurrentPhaseHP = GetPhaseHP(CurrentBossIndex, CurrentPhaseIndex);
        BossController.SetBossPhase(CurrentPhaseIndex);

        if (CurrentPhaseIndex == lastPhaseIndex)
        {
            // The boss's final phase shows its own skeleton exactly like any other phase --
            // the big brick is an additional thing spawned on top of that, not a replacement.
            // Destroying the brick (normal ball collision, not hit-count damage) is what
            // finishes this boss.
            GameManager.Instance.BrickManager.SpawnBossFinalPhaseBrick(this);
        }
        OnPhaseChanged?.Invoke(CurrentPhaseIndex);
    }

    // Called by BrickManager once the final-phase big brick is destroyed. Applies the same
    // defeat/transition outcome a normal phase's hit-count depletion would -- the final phase's
    // skeleton is still on screen (SpawnBossFinalPhaseBrick adds the brick on top of it, doesn't
    // replace it), so PlayDefeat animates it same as any other phase.
    public void FinishFinalPhase()
    {
        if (IsDefeated || IsTransitioning) return;

        BossController.PlayDefeat();

        // Every boss's end scene is player-driven (FinalBossController), so the wait is the same
        // whether or not another boss is coming -- nothing is on a timer. IsTransitioning stays
        // set for the whole window so queued hits can't land on an already-defeated boss.
        IsTransitioning = true;
        IsAwaitingContinue = true;

        bool hasNextBoss = CurrentBossIndex < Bosses.Count - 1;
        if (!hasNextBoss)
        {
            IsDefeated = true;
            OnBossDefeated?.Invoke();
        }

        OnAwaitingContinue?.Invoke();
    }

    // Called by FinalBossController.EndFinalBoss when the player finishes the end scene. Also
    // usable straight from a Button's OnClick -- it takes no arguments, so no extra script is
    // needed between the button and here.
    public void Continue()
    {
        if (!IsAwaitingContinue) return;

        IsAwaitingContinue = false;

        BossController finishedBoss = BossController;

        // Reset FinalScene's own active state as well as the boss root -- deactivating just the
        // root isn't enough if that same root gets reactivated for "the next boss" (e.g. Bosses
        // reusing the same GameObject), since FinalScene's own flag would still read active.
        if (finishedBoss.FinalScene != null)
        {
            finishedBoss.FinalScene.SetActive(false);
        }

        bool hasNextBoss = CurrentBossIndex < Bosses.Count - 1;
        if (hasNextBoss)
        {
            finishedBoss.gameObject.SetActive(false);
            CurrentBossIndex++;
            ActivateCurrentBoss();
        }

        IsTransitioning = false;
        OnContinued?.Invoke();
    }

    // Boss attacking the player isn't built yet -- just the event hook other systems can listen
    // for once that behavior exists.
    public void AttackPlayer()
    {
        OnAttackPlayer?.Invoke();
    }
}
