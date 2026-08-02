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

    // Where spawned coins animate to before their value gets added.
    public Transform CollectPoint;

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
    }

    public void ResetWave()
    {
        _wave = 0;
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

    public int GetPlayerChanceCount() => _playerChanceCount;

    public void SetPlayerChanceCount(int count)
    {
        _playerChanceCount = count;
        OnPlayerChanceCountChanged?.Invoke(_playerChanceCount);
    }

    // HP-style: always takes the hit (decrements), then reports whether the player is still
    // alive afterward. The hit that brings it to 0 is a death blow on that same hit, not a save
    // that only fails next time.
    public bool TakePlayerHit()
    {
        SetPlayerChanceCount(_playerChanceCount - 1);
        return _playerChanceCount > 0;
    }
}
