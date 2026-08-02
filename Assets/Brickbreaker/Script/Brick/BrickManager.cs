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

    // How many times the player can be saved from a brick reaching the bottom before it's
    // actually Game Over -- was a one-shot bool, now a count since multiple saves are possible.
    public int StartingPlayerChanceCount = 1;

    // Every brick destroyed chips at the boss (if one's alive) by this much -- this is the only
    // source of boss damage now, skill activation no longer touches it.
    [Header("Boss Attack")]
    public int AttackPowerToBoss = 1;
    public GameObject BossAttackVfx;
    public float BossAttackVfxRadius = 0.5f;
    public float BossAttackDelay = 0.15f;
    public float PhaseChangeDelay = 0.5f;
    public float FinalPhaseBrickHpMultiplier = 5f;

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

    private void Start()
    {
        Launcher.OnShotFinished.AddListener(HandleShotFinished);
        Launcher.OnShotStarted.AddListener(HandleShotStarted);
        GameManager.Instance.BossManager.OnBossDefeated.AddListener(EnterBossEndingPhase);

        // No longer auto-starts -- MainMenuHUD's Start button calls RestartGame(), which spawns
        // the first wave itself. Nothing happens here until the player actually presses Start.
    }

    // Resets per-shot PowerUp-effect state (not wave/game state) right as a new shot begins --
    // every brick's own repeat-hit counter and the shot-wide destroyed-count both start at 0.
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

        BrickController bigBrick = Instantiate(BigBrickPrefab, transform);
        bigBrick.transform.position = GridToWorld(centerPosition);
        bigBrick.Spawn(BrickConfig.GetBaseHP(GameManager.Instance.GetWave()), centerPosition);
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

        BrickController bigBrick = Instantiate(BigBrickPrefab, transform);
        bigBrick.transform.position = GridToWorld(centerPosition);
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
        GameManager.Instance.ResetCoinShop();
        GameManager.Instance.ResetScore();
        GameManager.Instance.CoinManager.ResetCoins();
        GameManager.Instance.SetPlayerChanceCount(StartingPlayerChanceCount);
        GameManager.Instance.SkillManager.ResetSkillPoint();
        GameManager.Instance.BossManager.ResetBoss();
        PowerUpManager.Instance.ResetPowerUps();
        _isGameOver = false;
        _isBossEndingPhase = false;
        _pendingBossHits = 0;
        _finalPhaseBrickDestroyed = false;
        _finalPhaseBrickBossManager = null;
        _isFinalPhaseBrickActive = false;

        Launcher.ResetLauncher();
        GameManager.Instance.LaunchManager.ResetRoster();

        SpawnWaveBricks();
        GameManager.Instance.StateMachine.ChangeState(GameState.Aiming);
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
        DescendBricks();

        if (_isGameOver) yield break; // GameOver() already set GameState.GameOver

        GameManager.Instance.StateMachine.ChangeState(GameState.SpawnWave);
        SpawnWaveBricks();

        GameManager.Instance.StateMachine.ChangeState(GameState.Aiming);
    }

    // Cheat/testing hook -- queues hitCount hits (same as hitCount bricks destroyed) and plays
    // them back through the normal delay/Shop flow, instead of calling BossManager directly and
    // skipping the Shop entirely.
    public void CheatAttackBoss(int hitCount)
    {
        _pendingBossHits += hitCount;
        StartCoroutine(CheckIfCanHitBoss());
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

        for (int i = 0; i < hits; i++)
        {
            if (bossManager.IsDefeated || bossManager.IsTransitioning) yield break;

            bossManager.DamageBoss(AttackPowerToBoss);

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

            yield return new WaitForSeconds(BossAttackDelay);
        }
    }

    // Every alive brick moves one row closer to the player. Rebuilds the lookup since the keys
    // (grid positions) are changing -- iterating and mutating the same dictionary isn't safe.
    private void DescendBricks()
    {
        if (WouldAnyBrickReachBottom())
        {
            // Thematically the boss's own attack landing -- fires regardless of whether the
            // player actually has a chance left to survive it.
            GameManager.Instance.BossManager.AttackPlayer();

            if (GameManager.Instance.TakePlayerHit())
            {
                ClearBottomRows(3);
            }
            else
            {
                GameOver();
                return;
            }
        }

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
    }

    private bool WouldAnyBrickReachBottom()
    {
        foreach (KeyValuePair<Vector2Int, BrickController> entry in _bricks)
        {
            if (entry.Key.y - 1 <= BottomRow) return true;
        }
        return false;
    }

    // Directly destroys every brick in the bottom rowCount rows using their current (pre-shift)
    // positions -- no coin/score reward, since this is an emergency save, not a normal kill.
    private void ClearBottomRows(int rowCount)
    {
        for (int i = 1; i <= rowCount; i++)
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

            // Just one gold brick per gold wave -- the first one spawned. No reward/visual
            // behavior wired up yet, just the marker on BrickController for now.
            if (isGoldWave && i == 0)
            {
                brick.SetGold(true);
            }

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

    public List<BrickController> GetSideNeighbors(BrickController brick) => GetNeighbors(brick.GridPosition, SideOffsets);
    public List<BrickController> GetDiagonalNeighbors(BrickController brick) => GetNeighbors(brick.GridPosition, DiagonalOffsets);

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

    private List<BrickController> GetNeighbors(Vector2Int gridPosition, Vector2Int[] offsets)
    {
        List<BrickController> neighbors = new List<BrickController>();
        foreach (Vector2Int offset in offsets)
        {
            if (_bricks.TryGetValue(gridPosition + offset, out BrickController neighbor))
                neighbors.Add(neighbor);
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
        GameManager.Instance.StateMachine.ChangeState(GameState.GameOver);
        OnGameOver?.Invoke();
    }
}
