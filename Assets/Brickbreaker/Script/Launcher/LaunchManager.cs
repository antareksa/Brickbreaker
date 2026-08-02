using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BallGachaEntry
{
    public BallControllerV2 Prefab;
    public float Weight;
}

// Owns the roster of balls available to launch -- separate from LauncherControllerV2, which owns
// aiming/input/firing. Lets balls be added to the roster later (e.g. a pickup) without
// LauncherControllerV2 needing to know how/why the roster changed.
public class LaunchManager : MonoBehaviour
{
    public LauncherControllerV2 LauncherController;
    public BallControllerV2 BasicBallPrefab;
    public List<BallGachaEntry> GachaEntries;

    // Flat per-bundle costs, not a per-ball rate -- 6 balls (220) isn't just 6x the 1-ball
    // price (40), it's a discounted bundle.
    public int OneBallCost = 40;
    public int SixBallCost = 220;

    public UnityEvent<BallControllerV2> OnBallAdded = new UnityEvent<BallControllerV2>();
    public UnityEvent<List<BallControllerV2>> OnGachaRolled = new UnityEvent<List<BallControllerV2>>();

    private int _startingBallTotal = 1;
    private readonly List<BallControllerV2> _balls = new List<BallControllerV2>();

    public IReadOnlyList<BallControllerV2> Balls => _balls;

    private static readonly Color[] _debugColors =
    {
        Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan,
        new Color(1f, 0.5f, 0f) // orange
    };

    private void Start()
    {
        LauncherController.InitializeLauncher(this);
        ResetRoster();
    }

    // Destroys every currently-owned ball and rebuilds the starting roster from scratch --
    // used both at game start and on restart, so a game-over doesn't let extra balls bought
    // via gacha before losing carry over into the next run.
    public void ResetRoster()
    {
        foreach (BallControllerV2 ball in _balls)
        {
            if (ball != null) Destroy(ball.gameObject);
        }
        _balls.Clear();

        // The very first ball is always basic, not gacha-rolled -- the player shouldn't be able
        // to start the game without a ball just because of an unlucky roll (or empty GachaEntries).
        AddSpecificBall(BasicBallPrefab);

        for (int i = 1; i < _startingBallTotal; i++)
        {
            AddBall();
        }
    }

    public BallControllerV2 AddBall()
    {
        BallControllerV2 prefab = RollGachaPrefab();
        return AddSpecificBall(prefab);
    }

    private BallControllerV2 AddSpecificBall(BallControllerV2 prefab)
    {
        if (prefab == null) return null;

        BallControllerV2 ball = Instantiate(prefab);
        ball.Stop();
        ball.PathColor = _debugColors[Random.Range(0, _debugColors.Length)];
        ball.name = $"Ball_{_balls.Count}";

        _balls.Add(ball);
        OnBallAdded?.Invoke(ball);

        return ball;
    }

    // Standard weighted-random pick: sum all weights, roll a point in that range, walk the
    // entries subtracting each one's weight until the point lands inside it.
    private BallControllerV2 RollGachaPrefab()
    {
        float totalWeight = 0f;
        foreach (BallGachaEntry entry in GachaEntries)
        {
            totalWeight += entry.Weight;
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("[LaunchManager] No gacha entries with positive weight -- can't roll a ball prefab.");
            return null;
        }

        float roll = Random.Range(0f, totalWeight);

        foreach (BallGachaEntry entry in GachaEntries)
        {
            if (roll < entry.Weight) return entry.Prefab;
            roll -= entry.Weight;
        }

        return GachaEntries[GachaEntries.Count - 1].Prefab;
    }

    // Only 1-ball and 6-ball bundles have a defined price -- anything else falls back to the
    // 1-ball rate times quantity, since nothing in the UI currently buys any other amount.
    public int GetBallCost(int totalBall)
    {
        if (totalBall == 6) return SixBallCost;
        return OneBallCost * totalBall;
    }

    public void BuyBall(int totalBall)
    {
        if (!IsCanBuyBall(totalBall)) return;

        GameManager.Instance.SpendCoin(GetBallCost(totalBall));

        List<BallControllerV2> rolledBalls = new List<BallControllerV2>();
        for (int i = 0; i < totalBall; i++)
        {
            rolledBalls.Add(AddBall());
        }

        OnGachaRolled?.Invoke(rolledBalls);
    }

    public bool IsCanBuyBall(int totalBall)
    {
        int coin = GameManager.Instance.GetCoin();
        int cost = GetBallCost(totalBall);
        return coin >= cost;
    }
}
