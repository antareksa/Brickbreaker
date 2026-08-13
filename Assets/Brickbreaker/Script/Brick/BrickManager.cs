using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BrickManager : MonoBehaviour
{
    public BrickController BrickPrefab;
    public BrickController BigBrickPrefab;
    public LauncherControllerV2 Launcher;
    public ShopHUD ShopHUD;

    // World position of grid cell (0,0), and world distance between adjacent cells. Measured
    // directly off the actual placed bricks in the scene (not guessed) -- the scene's Grid
    // component isn't positioned to match the real brick layout, so we don't use it.
    [Header("Grid")]
    public Vector2 GridOrigin = new Vector2(-3.5f, -3.5f);
    public Vector2 CellSize = new Vector2(1f, 1f);
    public int SpawnRow = 6;
    public Vector2Int SpawnColumnRange = new Vector2Int(1, 6);
    public int BottomRow = 0;
    public Transform BigBrickSpawnPosition;

    // Pause between bricks reaching the bottom (boss attack lands) and the bottom rows actually
    // being cleared -- without it they vanish the same frame, with no chance to read what hit you.
    [Header("Descend")]
    public float BottomRowClearDelay = 0.5f;

    // Animator speed multiplier for bricks that actually landed on the bottom, applied for the
    // BottomRowClearDelay beat before they're cleared.
    public float BottomRowDangerAnimationSpeed = 2f;

    // Token awarded at game over, scaled by how far the player got -- the only source of the
    // meta-progression currency TraitManager spends on trait upgrades.
    [Header("Token Reward")]
    public float TokensPerWave = 1f;

    // Every brick destroyed chips at the boss (if one's alive) by this much -- this is the only
    // source of boss damage now, skill activation no longer touches it.
    [Header("Boss Attack")]
    public int AttackPowerToBoss = 1;
    public GameObject BossAttackVfx;
    public float BossAttackVfxRadius = 0.5f;
    public float BossAttackDelay = 0.15f;
    public float PhaseChangeDelay = 0.5f;
    public float FinalPhaseBrickHpMultiplier = 5f;
    public List<AudioClip> BossAttackedSFX;

    // Flat, same amount for every phase kill (including the Final Phase) -- no scaling by phase
    // or boss number, per the design doc's economy section.
    [Header("Shop")]
    public int CoinShopRewardPerPhaseKill = 5;

    public UnityEvent OnGameOver = new UnityEvent();

    private bool _isGameOver;
    private bool _isBossEndingPhase;
    private int _pendingBossHits;
    private bool _finalPhaseBrickDestroyed;
    private BossManager _finalPhaseBrickBossManager;

    // True from the moment SpawnBossFinalPhaseBrick spawns the brick until it's destroyed --
    // same idea as _isBossEndingPhase (freeze AdvanceWave/DescendBricks/SpawnWaveBricks) but
    // scoped to just this one phase instead of the rest of the run.
    private bool _isFinalPhaseBrickActive;
    private int _bricksDestroyedThisShot;
    private readonly Dictionary<Vector2Int, BrickController> _bricks = new Dictionary<Vector2Int, BrickController>();

    // How many bricks have been destroyed so far THIS shot -- for PowerUp effects like "bonus
    // damage for every brick already destroyed this shot". Reset on OnShotStarted, not per-wave.
    public int BricksDestroyedThisShot => _bricksDestroyedThisShot;

    public int GetBrickCount() => _bricks.Count;

    // Snapshot copy -- callers (Consumable effects hitting "every brick") destroy bricks while
    // iterating, which would break iterating the live dictionary's values directly.
    public List<BrickController> GetAllBricks() => new List<BrickController>(_bricks.Values);

    // One-shot flag consumed by the very next DescendBricks call -- set by the "bricks don't
    // descend this wave" Consumable effect. Refreshes danger immediately since using it means
    // nothing is actually going to reach the bottom this wave.
    private bool _skipNextDescend;

    public void SkipNextDescend()
    {
        _skipNextDescend = true;
        RefreshDangerState();
    }

    private void Start()
    {
        Launcher.OnShotFinished.AddListener(HandleShotFinished);
        Launcher.OnShotStarted.AddListener(HandleShotStarted);
        GameManager.Instance.BossManager.OnBossDefeated.AddListener(EnterBossEndingPhase);

        // No longer auto-starts -- MainMenuHUD's Start button calls RestartGame(), which spawns
        // the first wave itself. Nothing happens here until the player actually presses Start.
    }

    // Resets per-shot PowerUp-effect state (not wave/game state) right as a new shot begins.
    // Only _bricksDestroyedThisShot still matters -- the ResetShotState loop is OBSOLETE, since
    // the per-brick repeat-hit counter it clears is no longer read by anything (repeat-hit moved
    // to per-(ball, brick) tracking on BallControllerV2). Kept in step with BrickController's own
    // obsolete counter; both can be deleted together.
    private void HandleShotStarted()
    {
        _bricksDestroyedThisShot = 0;

        foreach (BrickController brick in _bricks.Values)
        {
            if (brick != null) brick.ResetShotState();
        }
    }

    // Boss fully defeated -- clears the board, spawns one big brick centered on the grid, and
    // suspends the loss condition for the rest of the run. Just a celebratory phase, not a
    // continuation of the normal difficulty ramp.
    private void EnterBossEndingPhase()
    {
        _isBossEndingPhase = true;

        foreach (BrickController brick in _bricks.Values)
        {
            if (brick != null) Destroy(brick.gameObject);
        }
        _bricks.Clear();

        Vector2Int centerPosition = new Vector2Int(
            (SpawnColumnRange.x + SpawnColumnRange.y) / 2,
            SpawnRow);

        BrickController bigBrick = Instantiate(BigBrickPrefab, BigBrickSpawnPosition); //Instantiate(BigBrickPrefab, transform);
        bigBrick.transform.position = Vector3.zero; //BigBrickSpawnPosition.transform.position; //GridToWorld(centerPosition);
        bigBrick.Spawn(BrickConfig.GetBaseHP(GameManager.Instance.GetWave()), Vector2Int.zero);
        bigBrick.OnDestroyed.AddListener(HandleBrickDestroyed);

        _bricks[centerPosition] = bigBrick;
    }

    // A boss's final phase spawns this big brick alongside its (still visible) skeleton, and
    // destroying it (normal ball collision, not the hit-count system) finishes that phase.
    // Called by BossManager.AdvancePhaseIfComplete() when entering a boss's last phase.
    public void SpawnBossFinalPhaseBrick(BossManager bossManager)
    {
        _isFinalPhaseBrickActive = true;

        foreach (BrickController brick in _bricks.Values)
        {
            if (brick != null) Destroy(brick.gameObject);
        }
        _bricks.Clear();

        Vector2Int centerPosition = new Vector2Int(
            (SpawnColumnRange.x + SpawnColumnRange.y) / 2,
            SpawnRow);

        BrickController bigBrick = Instantiate(BigBrickPrefab, BigBrickSpawnPosition);
        bigBrick.transform.localPosition = Vector3.zero; //GridToWorld(centerPosition);
        // CurrentPhaseHP is already the formula-computed HP for this (final) phase --
        // AdvancePhaseIfComplete sets it right before calling this.
        bigBrick.Spawn(Mathf.RoundToInt(bossManager.CurrentPhaseHP * FinalPhaseBrickHpMultiplier), centerPosition);
        bigBrick.OnDestroyed.AddListener(brick => HandleFinalPhaseBrickDestroyed(brick, bossManager));

        _bricks[centerPosition] = bigBrick;
    }

    // Fires mid-shot (same as any other brick destruction), so the actual "finish this boss"
    // step is deferred to CheckIfCanHitBoss, which runs after the shot fully resolves -- same
    // PhaseChangeDelay-then-Shop sequence a normal hit-count phase completion goes through.
    private void HandleFinalPhaseBrickDestroyed(BrickController brick, BossManager bossManager)
    {
        _bricks.Remove(brick.GridPosition);
        GameManager.Instance.CoinManager.SpawnCoin(brick.transform.position, brick.IsGold);

        _finalPhaseBrickDestroyed = true;
        _finalPhaseBrickBossManager = bossManager;
        _isFinalPhaseBrickActive = false;
    }

    // Clears every brick on the field, resets wave/game-over state, resets the launcher, and
    // spawns wave 0 again -- puts the whole game back to its just-started state.
    public void RestartGame()
    {
        foreach (BrickController brick in _bricks.Values)
        {
            if (brick != null) Destroy(brick.gameObject);
        }
        _bricks.Clear();

        GameManager.Instance.ResetWave();
        GameManager.Instance.ResetCoin();
        GrantStartingCoinBonus();
        GameManager.Instance.ResetCoinShop();
        GameManager.Instance.ResetScore();
        GameManager.Instance.CoinManager.ResetCoins();
        // Always starts at full HP -- the ExtraChance meta-trait raises GetMaxHp(), so a
        // purchased level directly grants more starting HP each run, not just a higher ceiling.
        GameManager.Instance.SetPlayerChanceCount(GameManager.Instance.GetMaxHp());
        GameManager.Instance.ResetWavesSinceLastHpLoss();
        GameManager.Instance.SkillManager.ResetSkillPoint();
        GameManager.Instance.BossManager.ResetBoss();
        PowerUpManager.Instance.ResetPowerUps();
        ConsumableManager.Instance.ResetConsumables();
        BallEnhanceManager.Instance.ResetEnhances();
        _isGameOver = false;
        _isBossEndingPhase = false;
        _pendingBossHits = 0;
        _finalPhaseBrickDestroyed = false;
        _finalPhaseBrickBossManager = null;
        _isFinalPhaseBrickActive = false;
        _skipNextDescend = false;

        Launcher.ResetLauncher();
        GameManager.Instance.LaunchManager.ResetRoster();

        SpawnWaveBricks();
        RefreshDangerState();
        GameManager.Instance.StateMachine.ChangeState(GameState.Aiming);
    }

    // Cheat/testing hook -- rebuilds the board at a given wave's difficulty without touching
    // anything else (PowerUps, Consumables, enhances, balls, Coin), unlike RestartGame. Lets a
    // test setup be assembled in any order without the wave jump wiping it.
    public void CheatJumpToWave(int wave)
    {
        foreach (BrickController brick in _bricks.Values)
        {
            if (brick != null) Destroy(brick.gameObject);
        }
        _bricks.Clear();

        GameManager.Instance.SetWave(wave);
        SpawnWaveBricks();
        RefreshDangerState();
    }

    private void HandleShotFinished()
    {
        if (_isGameOver) return;

        StartCoroutine(HandleShotFinishedRoutine());
    }

    // aiming -> shooting -> all balls back -> collect coins -> skill check -> advance wave and
    // so on. Coin collection has to run (and be waited on, since it's an animation) before the
    // skill check, which itself has to run before the next wave spawns.
    private IEnumerator HandleShotFinishedRoutine()
    {
        // Each phase change is what actually blocks player input -- LauncherControllerV2 only
        // accepts aim/shoot input while GameManager.StateMachine.CurrentState == Aiming, so the
        // player can't do anything from here until this routine sets it back to Aiming (or
        // GameOver, at which point Restart is the only way back in).
        GameManager.Instance.StateMachine.ChangeState(GameState.CheckCoin);
        yield return GameManager.Instance.CoinManager.CollectAllCoins();

        // Skill only activates here -- after shooting (and coin collection) is fully done, before
        // the next wave spawns -- never mid-shot, even if enough points were banked while balls
        // were still flying.
        GameManager.Instance.StateMachine.ChangeState(GameState.CheckSkill);
        GameManager.Instance.SkillManager.TryActivateSkill();

        // Bricks destroyed during the shot only queued up hits (HandleBrickDestroyed) rather
        // than attacking immediately -- playing them back here, one at a time with a small
        // delay, reads as a clean "boss takes N hits" sequence instead of everything firing at
        // once mid-chaos while balls are still bouncing around.
        GameManager.Instance.StateMachine.ChangeState(GameState.CheckBossAttack);
        yield return CheckIfCanHitBoss();

        // Once the boss's ending phase starts, the normal difficulty ramp (descend/lose
        // condition, spawning new waves) stops entirely -- it's just the big brick sitting there.
        // Same freeze while a boss's final-phase brick is alive but not destroyed yet -- no wave
        // advance, no new bricks spawning alongside/after it, until that brick is gone. Either
        // way the player can still shoot at whatever big brick is on the field, so this goes
        // back to Aiming rather than staying stuck mid-phase.
        if (_isBossEndingPhase || _isFinalPhaseBrickActive)
        {
            GameManager.Instance.StateMachine.ChangeState(GameState.Aiming);
            yield break;
        }

        GameManager.Instance.StateMachine.ChangeState(GameState.AdvanceWave);
        GameManager.Instance.AdvanceWave();
        GameManager.Instance.SoundManager.Play(SoundType.WaveClear);
        yield return DescendBricksRoutine();

        if (_isGameOver) yield break; // GameOver() already set GameState.GameOver

        GameManager.Instance.StateMachine.ChangeState(GameState.SpawnWave);
        SpawnWaveBricks();
        RefreshDangerState();

        GameManager.Instance.StateMachine.ChangeState(GameState.Aiming);
    }

    // Cheat/testing hook -- queues hitCount hits (same as hitCount bricks destroyed) and plays
    // them back through the normal delay/Shop flow, instead of calling BossManager directly and
    // skipping the Shop entirely.
    public void CheatAttackBoss(int hitCount)
    {
        _pendingBossHits += hitCount;
        StartCoroutine(CheatAttackBossRoutine());
    }

    // CheckIfCanHitBoss only returns to Aiming itself when called from the real shot-finished
    // flow (HandleShotFinishedRoutine handles that afterward) -- called standalone like this, a
    // phase kill would open the Shop, and closing it would leave the game stuck in GameState.Shop
    // forever with no further transition to re-enable input.
    private IEnumerator CheatAttackBossRoutine()
    {
        yield return CheckIfCanHitBoss();
        GameManager.Instance.StateMachine.ChangeState(GameState.Aiming);
    }

    // Plays back however many hits queued up from bricks destroyed this shot, one at a time
    // with BossAttackDelay between each. If a hit is the killing blow for the current phase (or
    // the whole boss), that hit still shows -- but any further queued hits are discarded rather
    // than carrying over into the next phase/boss. E.g. phase needs 3 hits, 5 are queued: hits
    // 1-3 play, the 3rd triggers the phase change, and hits 4-5 are ignored.
    //
    // A destroyed final-phase big brick takes priority over the normal hit-count queue -- it's
    // the same "phase finished" outcome, just reached via direct ball collision instead.
    private IEnumerator CheckIfCanHitBoss()
    {
        if (_finalPhaseBrickDestroyed)
        {
            BossManager finishedBossManager = _finalPhaseBrickBossManager;
            _finalPhaseBrickDestroyed = false;
            _finalPhaseBrickBossManager = null;
            _pendingBossHits = 0;

            yield return new WaitForSeconds(PhaseChangeDelay);

            finishedBossManager.FinishFinalPhase();

            GameManager.Instance.AddCoinShop(CoinShopRewardPerPhaseKill);
            GameManager.Instance.StateMachine.ChangeState(GameState.Shop);
            ShopHUD.Open();
            yield return new WaitUntil(() => !ShopHUD.IsOpen);

            yield break;
        }

        if (_pendingBossHits <= 0) yield break;

        BossManager bossManager = GameManager.Instance.BossManager;
        int hits = _pendingBossHits;
        _pendingBossHits = 0;

        if (bossManager == null) yield break;

        if (bossManager.IsDefeated || bossManager.IsTransitioning) yield break;

        bossManager.DamageBoss(AttackPowerToBoss*hits);

        Transform bossHitPoint = bossManager.BossController != null ? bossManager.BossController.BossHitPoint : null;
        if (bossHitPoint != null)
        {
            Vector3 randomOffset = (Vector3)(Random.insideUnitCircle * BossAttackVfxRadius);
            VFXManager.Instance.PlayVFX(BossAttackVfx, bossHitPoint.position + randomOffset);
        }

        if (bossManager.IsPhaseComplete)
        {
            // The hit landed and the phase's HP is empty, but the spine/phase swap
            // (AdvancePhaseIfComplete) is deliberately delayed so the hit reaction reads
            // first, rather than snapping straight to the new phase/defeat visual.
            yield return new WaitForSeconds(PhaseChangeDelay);

            bossManager.AdvancePhaseIfComplete();

            GameManager.Instance.AddCoinShop(CoinShopRewardPerPhaseKill);
            GameManager.Instance.StateMachine.ChangeState(GameState.Shop);
            ShopHUD.Open();
            yield return new WaitUntil(() => !ShopHUD.IsOpen);

            yield break;
        }

        //for (int i = 0; i < hits; i++)
        //{
        //    if (bossManager.IsDefeated || bossManager.IsTransitioning) yield break;

        //    bossManager.DamageBoss(AttackPowerToBoss);

        //    Transform bossHitPoint = bossManager.BossController != null ? bossManager.BossController.BossHitPoint : null;
        //    if (bossHitPoint != null)
        //    {
        //        Vector3 randomOffset = (Vector3)(Random.insideUnitCircle * BossAttackVfxRadius);
        //        VFXManager.Instance.PlayVFX(BossAttackVfx, bossHitPoint.position + randomOffset);
        //    }

        //    if (bossManager.IsPhaseComplete)
        //    {
        //        // The hit landed and the phase's HP is empty, but the spine/phase swap
        //        // (AdvancePhaseIfComplete) is deliberately delayed so the hit reaction reads
        //        // first, rather than snapping straight to the new phase/defeat visual.
        //        yield return new WaitForSeconds(PhaseChangeDelay);

        //        bossManager.AdvancePhaseIfComplete();

        //        GameManager.Instance.AddCoinShop(CoinShopRewardPerPhaseKill);
        //        GameManager.Instance.StateMachine.ChangeState(GameState.Shop);
        //        ShopHUD.Open();
        //        yield return new WaitUntil(() => !ShopHUD.IsOpen);

        //        yield break;
        //    }

        //    yield return new WaitForSeconds(BossAttackDelay);
        //}
    }

    // Every alive brick moves one row closer to the player. Rebuilds the lookup since the keys
    // (grid positions) are changing -- iterating and mutating the same dictionary isn't safe.
    private IEnumerator DescendBricksRoutine()
    {
        // Skips the reach-bottom check too, not just the shift loop below -- with nothing moving
        // down, nothing can newly reach the bottom threshold this wave either, so this only ever
        // makes the wave safer, never different in a way that needs its own HP-loss handling.
        if (_skipNextDescend)
        {
            _skipNextDescend = false;
            yield break;
        }

        // Shift FIRST, then react. The old order resolved the breach while the bricks were still
        // one row up, so the player never actually saw anything reach the bottom -- they just
        // vanished from where they already were.
        Dictionary<Vector2Int, BrickController> shifted = new Dictionary<Vector2Int, BrickController>();

        foreach (KeyValuePair<Vector2Int, BrickController> entry in _bricks)
        {
            BrickController brick = entry.Value;
            Vector2Int newPosition = new Vector2Int(entry.Key.x, entry.Key.y - 1);

            brick.SetGridPosition(newPosition);
            brick.transform.position = GridToWorld(newPosition);
            shifted[newPosition] = brick;
        }

        _bricks.Clear();
        foreach (KeyValuePair<Vector2Int, BrickController> entry in shifted)
        {
            _bricks[entry.Key] = entry.Value;
        }

        // Nothing landed on the bottom -- no attack and no delay, the wave just moved down.
        if (!AnyBrickAtBottom()) yield break;

        SpeedUpBottomRowAnimation();

        // Thematically the boss's own attack landing -- fires regardless of whether the player
        // actually has a chance left to survive it.
        GameManager.Instance.BossManager.AttackPlayer();

        // Beat with the bricks visibly sitting on the bottom before anything happens to them.
        // Before the branch so it covers all three outcomes (blocked, survived, dead).
        yield return new WaitForSeconds(BottomRowClearDelay);

        float blockChance = PowerUpManager.Instance != null ? PowerUpManager.Instance.GetTotalBlockHpLossChance() : 0f;
        bool blocked = blockChance > 0f && Random.value < blockChance;

        if (blocked)
        {
            ClearBottomRows(3);
            yield break;
        }

        bool survived = GameManager.Instance.TakePlayerHit();

        GameManager.Instance.BossManager.BossAttack();

        // Only on an actual loss -- a blocked hit above never reaches this, so it never grants
        // this bonus either.
        if (PowerUpManager.Instance != null)
        {
            float bonusCharge = PowerUpManager.Instance.GetTotalBonusSkillChargeOnHpLoss();
            if (bonusCharge > 0f)
            {
                GameManager.Instance.SkillManager.AddSkillPoint(bonusCharge);
            }
        }

        if (survived)
        {
            ClearBottomRows(3);
        }
        else
        {
            GameOver();
        }
    }

    // Flags every brick that will breach on the NEXT descend, so the warning is up while the
    // player aims -- hence the predictive y - 1, unlike AnyBrickAtBottom which runs after the
    // shift has already happened.
    private void RefreshDangerState()
    {
        foreach (KeyValuePair<Vector2Int, BrickController> entry in _bricks)
        {
            if (entry.Value == null) continue;

            bool inDanger = !_skipNextDescend && entry.Key.y - 1 <= BottomRow;
            entry.Value.SetDanger(inDanger);
        }
    }

    // Only the bricks that actually landed on the bottom -- the rows above keep the normal-speed
    // danger loop (or none at all).
    private void SpeedUpBottomRowAnimation()
    {
        foreach (KeyValuePair<Vector2Int, BrickController> entry in _bricks)
        {
            if (entry.Value == null) continue;
            if (entry.Key.y > BottomRow) continue;

            entry.Value.SetAnimatorSpeed(BottomRowDangerAnimationSpeed);
        }
    }

    // Post-shift check -- bricks have already moved, so this tests where they ARE rather than
    // where they're about to be.
    private bool AnyBrickAtBottom()
    {
        foreach (KeyValuePair<Vector2Int, BrickController> entry in _bricks)
        {
            if (entry.Key.y <= BottomRow) return true;
        }
        return false;
    }

    // Directly destroys every brick in the bottom rowCount rows -- no coin/score reward, since
    // this is an emergency save, not a normal kill. Starts AT BottomRow (not one above it) since
    // the shift has already run by the time this is called, so breaching bricks sit on it.
    private void ClearBottomRows(int rowCount)
    {
        for (int i = 0; i < rowCount; i++)
        {
            int row = BottomRow + i;
            List<BrickController> bricks = GetBricksInRow(row);

            foreach (BrickController brick in bricks)
            {
                _bricks.Remove(brick.GridPosition);
                if (brick != null) Destroy(brick.gameObject);
            }
        }
    }

    private void SpawnWaveBricks()
    {
        int wave = GameManager.Instance.GetWave();
        int spawnCount = BrickConfig.GetBricksSpawned(wave);
        int baseHp = BrickConfig.GetBaseHP(wave);
        int tankCount = BrickConfig.GetTankCount(wave);
        int tankHp = BrickConfig.GetTankHP(wave);
        bool isGoldWave = BrickConfig.IsGoldWave(wave);

        for (int i = 0; i < spawnCount; i++)
        {
            if (!TryPickFreeSpawnPosition(out Vector2Int gridPosition)) continue;

            // The first tankCount bricks spawned this wave are tanks (2x HP) -- there's no
            // separate tank prefab yet, so they use the same BrickPrefab, just tougher.
            int hp = i < tankCount ? tankHp : baseHp;

            BrickController brick = Instantiate(BrickPrefab, transform);
            brick.transform.position = GridToWorld(gridPosition);
            brick.Spawn(hp, gridPosition);
            brick.OnDestroyed.AddListener(HandleBrickDestroyed);

            // Just one gold brick per gold wave -- the first one spawned. A PowerUp can also grant
            // a flat extra chance for any OTHER brick to independently roll Gold too.
            bool isGold = isGoldWave && i == 0;
            if (!isGold && PowerUpManager.Instance != null)
            {
                float bonusGoldChance = PowerUpManager.Instance.GetTotalBonusGoldChance();
                if (bonusGoldChance > 0f && Random.value < bonusGoldChance)
                {
                    isGold = true;
                }
            }
            brick.SetGold(isGold);

            _bricks[gridPosition] = brick;
        }
    }

    private bool TryPickFreeSpawnPosition(out Vector2Int gridPosition)
    {
        const int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int column = Random.Range(SpawnColumnRange.x, SpawnColumnRange.y + 1);
            Vector2Int candidate = new Vector2Int(column, SpawnRow);

            if (!_bricks.ContainsKey(candidate))
            {
                gridPosition = candidate;
                return true;
            }
        }

        gridPosition = default;
        return false;
    }

    private static readonly Vector2Int[] SideOffsets =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(1, 0),
    };

    private static readonly Vector2Int[] DiagonalOffsets =
    {
        new Vector2Int(1, 1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1),
    };

    private static readonly Vector2Int[] AllOffsets =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(1, 0),
        new Vector2Int(1, 1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1),
    };

    // reach = how many steps to walk along each direction, not just the tile at exactly that
    // distance -- e.g. reach 2 hits both the 1-tile and 2-tile bricks along each direction, for
    // Ball Enhance's Range axis (Fire/Lightning). Defaults to 1 (immediate neighbors only) for
    // every other caller.
    public List<BrickController> GetSideNeighbors(BrickController brick, int reach = 1) => GetNeighborsInReach(brick.GridPosition, SideOffsets, reach);
    public List<BrickController> GetDiagonalNeighbors(BrickController brick, int reach = 1) => GetNeighborsInReach(brick.GridPosition, DiagonalOffsets, reach);

    // Side + diagonal combined, immediate neighbors only -- used by Bomb's Range axis to pick
    // random neighboring bricks from, regardless of direction.
    public List<BrickController> GetAllNeighbors(BrickController brick) => GetNeighborsInReach(brick.GridPosition, AllOffsets, 1);

    public List<BrickController> GetBricksInRow(int row)
    {
        List<BrickController> bricksInRow = new List<BrickController>();
        foreach (KeyValuePair<Vector2Int, BrickController> entry in _bricks)
        {
            if (entry.Key.y == row) bricksInRow.Add(entry.Value);
        }
        return bricksInRow;
    }

    // Fixed left edge of the grid at that row -- not the leftmost occupied brick, so it stays
    // consistent even if that brick has already been destroyed. Assumes the VFX itself is
    // pivoted/designed to extend rightward from this point across the row.
    public Vector3 GetRowLeftWorldPosition(int row)
    {
        return GridToWorld(new Vector2Int(SpawnColumnRange.x, row));
    }

    public List<BrickController> GetBricksInColumn(int column)
    {
        List<BrickController> bricksInColumn = new List<BrickController>();
        foreach (KeyValuePair<Vector2Int, BrickController> entry in _bricks)
        {
            if (entry.Key.x == column) bricksInColumn.Add(entry.Value);
        }
        return bricksInColumn;
    }

    // Mirrors GetRowLeftWorldPosition: fixed bottom edge of the grid at that column, so the VFX
    // is assumed to extend upward from this point across the column.
    public Vector3 GetColumnBottomWorldPosition(int column)
    {
        return GridToWorld(new Vector2Int(column, BottomRow));
    }

    private List<BrickController> GetNeighborsInReach(Vector2Int gridPosition, Vector2Int[] directions, int reach)
    {
        List<BrickController> neighbors = new List<BrickController>();
        foreach (Vector2Int direction in directions)
        {
            for (int step = 1; step <= reach; step++)
            {
                if (_bricks.TryGetValue(gridPosition + direction * step, out BrickController neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }

    private void HandleBrickDestroyed(BrickController brick)
    {
        _bricks.Remove(brick.GridPosition);
        _bricksDestroyedThisShot++;

        GameManager.Instance.CoinManager.SpawnCoin(brick.transform.position, brick.IsGold);

        // Queued rather than applied immediately -- CheckIfCanHitBoss plays these back one at a
        // time, with a delay, once the shot has fully finished.
        BossManager bossManager = GameManager.Instance.BossManager;
        if (bossManager != null && !bossManager.IsDefeated)
        {
            _pendingBossHits++;
        }
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(
            GridOrigin.x + gridPosition.x * CellSize.x,
            GridOrigin.y + gridPosition.y * CellSize.y,
            0f);
    }

    private void GameOver()
    {
        _isGameOver = true;
        AwardTokens();
        GameManager.Instance.StateMachine.ChangeState(GameState.GameOver);
        OnGameOver?.Invoke();
    }

    // Awarded before OnGameOver fires so the GameOver screen (and the Upgrade menu reached from
    // it) already reflects the new token total.
    private void AwardTokens()
    {
        if (TraitManager.Instance == null) return;

        int tokens = Mathf.FloorToInt(GameManager.Instance.GetWave() * TokensPerWave);
        if (tokens > 0) TraitManager.Instance.AddToken(tokens);
    }

    // Flat Coin the StartingCoinBonus meta-trait grants at the start of every run -- applied
    // right after ResetCoin zeroes the previous run's balance.
    private void GrantStartingCoinBonus()
    {
        if (TraitManager.Instance == null) return;

        int bonus = Mathf.FloorToInt(TraitManager.Instance.GetTraitValue(TraitType.StartingCoinBonus));
        if (bonus > 0) GameManager.Instance.AddCoin(bonus);
    }
}
