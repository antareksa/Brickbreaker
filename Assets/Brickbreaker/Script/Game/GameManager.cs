using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public LaunchManager LaunchManager;
    public BrickManager BrickManager;
    public SkillManager SkillManager;
    public GameStateMachine StateMachine;
    public CoinManager CoinManager;
    public SoundManager SoundManager;
    public BossManager BossManager;
    public ConsumableManager ConsumableManager;
    public BallEnhanceManager BallEnhanceManager;

    // Where spawned coins animate to before their value gets added.
    public Transform CollectPoint;

    [Header("Display")]
    public int TargetWidth = 1366;
    public int TargetHeight = 768;

    // Base HP cap before the ExtraChance meta-trait -- see GetMaxHp for the actual effective cap
    // SetPlayerChanceCount clamps against and RestartGame starts the player at.
    [Header("HP")]
    public int MaxHP = 1;

    [Header("Score")]
    public float BaseScoreHit = 1f;
    public float BaseScoreDestroy = 5f;

    public UnityEvent<int> OnCoinChanged = new UnityEvent<int>();
    public UnityEvent<int> OnCoinShopChanged = new UnityEvent<int>();
    public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();
    public UnityEvent<int> OnWaveChanged = new UnityEvent<int>();
    public UnityEvent<int> OnPlayerChanceCountChanged = new UnityEvent<int>();

    private int _coin;
    private int _coinShop;
    private int _wave;
    private int _score;
    private int _playerChanceCount;
    private float _runStartTime;
    private int _totalBricksDestroyed;

    // Reset to 0 whenever TakePlayerHit is actually called (not when a PowerUp blocks the hit
    // entirely) -- lets a PowerUp award bonus HP for stringing together HP-loss-free waves.
    private int _wavesSinceLastHpLoss;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Locked so the game renders at a fixed size regardless of the player's monitor -- on a
        // 2K/4K display the OnGUI debug panels (which work in raw pixels and don't scale like the
        // Canvas UI does) would otherwise shrink to be unreadable. Passing Screen.fullScreen keeps
        // whichever window mode the player is already in.
        Screen.SetResolution(TargetWidth, TargetHeight, Screen.fullScreen);
        Screen.fullScreen = true;
    }

    public int GetCoin() => _coin;

    public int AddCoin(int coin)
    {
        _coin += coin;
        OnCoinChanged?.Invoke(_coin);
        return coin;
    }

    public int SpendCoin(int coin)
    {
        _coin -= coin;
        OnCoinChanged?.Invoke(_coin);
        return coin;
    }

    public bool IsCoinEnough(int coin)
    {
        return _coin <= coin;
    }

    public void ResetCoin()
    {
        _coin = 0;
        OnCoinChanged?.Invoke(_coin);
    }

    public int GetCoinShop() => _coinShop;

    public void AddCoinShop(int amount)
    {
        _coinShop += amount;
        OnCoinShopChanged?.Invoke(_coinShop);
    }

    // Checks funds first and floors at 0 (per design doc, no debt system) -- reports success so
    // callers (reroll, purchase) know whether to actually go through.
    public bool TrySpendCoinShop(int amount)
    {
        if (_coinShop < amount) return false;

        _coinShop -= amount;
        OnCoinShopChanged?.Invoke(_coinShop);
        return true;
    }

    public void ResetCoinShop()
    {
        _coinShop = 0;
        OnCoinShopChanged?.Invoke(_coinShop);
    }

    public int GetWave() => _wave;

    public void AdvanceWave()
    {
        _wave++;
        OnWaveChanged?.Invoke(_wave);

        _wavesSinceLastHpLoss++;
        if (PowerUpManager.Instance != null)
        {
            int bonusHp = PowerUpManager.Instance.GetTotalBonusHpForWavesSurvived(_wavesSinceLastHpLoss);
            if (bonusHp > 0)
            {
                SetPlayerChanceCount(_playerChanceCount + bonusHp);
            }
        }
    }

    public void ResetWave()
    {
        _wave = 0;
        OnWaveChanged?.Invoke(_wave);
    }

    // Cheat/testing hook -- jumps straight to a wave so brick HP scaling can be tested without
    // playing up to it. Board isn't respawned here; BrickManager.CheatJumpToWave does that part.
    public void SetWave(int wave)
    {
        _wave = wave;
        OnWaveChanged?.Invoke(_wave);
    }

    public int GetScore() => _score;

    public void AddHitScore(int damage, int brickType = 1)
    {
        // _wave + 1: _wave is 0 during the first wave, which would otherwise zero out all score
        // from it entirely.
        _score += Mathf.RoundToInt(damage * (_wave + 1) * BaseScoreHit * brickType);
        OnScoreChanged?.Invoke(_score);
    }

    public void AddDestroyScore(int damage, int brickType = 1)
    {
        _score += Mathf.RoundToInt(damage * (_wave + 1) * BaseScoreDestroy * brickType);
        OnScoreChanged?.Invoke(_score);
    }

    public void ResetScore()
    {
        _score = 0;
        OnScoreChanged?.Invoke(_score);
    }

    public void ResetPlayDuration()
    {
        _runStartTime = Time.time;
    }

    public int GetTotalBricksDestroyed() => _totalBricksDestroyed;

    public void AddBrickDestroyed()
    {
        _totalBricksDestroyed++;
    }

    public void ResetBricksDestroyed()
    {
        _totalBricksDestroyed = 0;
    }

    // Live elapsed time since the current run started -- not frozen at GameOver, but nothing
    // currently keeps ticking on the GameOver screen after it's read once, so that's fine.
    public float GetPlayDuration() => Time.time - _runStartTime;

    public int GetPlayerChanceCount() => _playerChanceCount;

    // Base MaxHP plus the persistent ExtraChance meta-trait (bought with Token, independent of
    // the current run) -- this is the real cap used at runtime, not the raw MaxHP field.
    public int GetMaxHp()
    {
        float traitBonus = TraitManager.Instance != null
            ? TraitManager.Instance.GetTraitValue(TraitType.ExtraChance)
            : 0f;
        return MaxHP + Mathf.RoundToInt(traitBonus);
    }

    public void SetPlayerChanceCount(int count)
    {
        _playerChanceCount = Mathf.Min(count, GetMaxHp());
        OnPlayerChanceCountChanged?.Invoke(_playerChanceCount);
    }

    // Re-fires OnPlayerChanceCountChanged with the current count unchanged -- just forces
    // listeners (like HealthHUD) to re-read GetMaxHp() immediately. Needed because GetMaxHp()
    // can change mid-session (buying/resetting the ExtraChance trait) without any actual HP loss
    // or gain to naturally trigger a refresh.
    public void RefreshMaxHp()
    {
        OnPlayerChanceCountChanged?.Invoke(_playerChanceCount);
    }

    // HP-style: always takes the hit (decrements), then reports whether the player is still
    // alive afterward. The hit that brings it to 0 is a death blow on that same hit, not a save
    // that only fails next time.
    public bool TakePlayerHit()
    {
        _wavesSinceLastHpLoss = 0;
        SetPlayerChanceCount(_playerChanceCount - 1);
        return _playerChanceCount > 0;
    }

    public void ResetWavesSinceLastHpLoss()
    {
        _wavesSinceLastHpLoss = 0;
    }
}
