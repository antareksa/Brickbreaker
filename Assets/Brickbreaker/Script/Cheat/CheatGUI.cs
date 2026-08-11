using UnityEngine;

// Immediate-mode debug panel (OnGUI, not the Canvas-based HUD system) -- no prefab/Canvas wiring
// needed, just drop this on any GameObject with a CheatManager reference.
public class CheatGUI : MonoBehaviour
{
    public CheatManager CheatManager;

    // The panel covers the top-left of the screen, so it needs a way to get out of the way of
    // whatever it's sitting on top of. Collapsed to just the toggle button, never fully gone --
    // an OnGUI-only panel has no other affordance to bring it back.
    public bool StartVisible = true;

    public int GoldAmount = 100;
    public int CoinShopAmount = 20;

    // Queued hits, not raw damage -- each deals BrickManager.AttackPowerToBoss, same as a real
    // brick destruction would.
    public int BossAttackHitCount = 10;
    public BasePowerUp TestPowerUp;
    public BaseConsumable TestConsumable;
    public BallEnhanceType TestBallEnhanceType;
    public BallEnhanceAxis TestBallEnhanceAxis;

    // Kept as strings rather than Inspector ints so they stay editable mid-Play-mode.
    private string _waveInput = "50";
    private string _powerUpInput = "1";
    private string _consumableInput = "1";

    private bool _isVisible;

    private void Awake()
    {
        _isVisible = StartVisible;
    }

    private void OnGUI()
    {
        // Collapsed height is just the toggle -- the area has to shrink with it, or the invisible
        // rect keeps eating clicks meant for the game underneath.
        GUILayout.BeginArea(new Rect(10, 10, 240, _isVisible ? 520 : 30));

        if (GUILayout.Button(_isVisible ? "Hide Cheats" : "Show Cheats"))
        {
            _isVisible = !_isVisible;
        }

        if (!_isVisible)
        {
            GUILayout.EndArea();
            return;
        }

        if (GUILayout.Button($"Add Gold {GoldAmount}"))
        {
            CheatManager.AddGold(GoldAmount);
        }

        if (GUILayout.Button($"Add Coin Shop {CoinShopAmount}"))
        {
            CheatManager.AddCoinShop(CoinShopAmount);
        }

        if (GUILayout.Button("Activate Skill"))
        {
            CheatManager.ActivateSkill();
        }

        if (GUILayout.Button("Force Restart"))
        {
            CheatManager.ForceRestart();
        }

        GUILayout.Label("Wave");
        _waveInput = GUILayout.TextField(_waveInput);

        // Button call has to run every OnGUI pass for IMGUI layout to stay consistent, so it's
        // the left operand -- the parse only gates what happens after the click.
        if (GUILayout.Button("Jump To Wave") && int.TryParse(_waveInput, out int wave))
        {
            CheatManager.JumpToWave(wave);
        }

        if (GUILayout.Button($"Attack Boss x{BossAttackHitCount} hits"))
        {
            CheatManager.AttackBoss(BossAttackHitCount);
        }

        // Resolved name is shown on the button itself so you can see what a number maps to
        // before clicking -- 30 PowerUps is too many to remember by index.
        GUILayout.Label("PowerUp #");
        _powerUpInput = GUILayout.TextField(_powerUpInput);

        int.TryParse(_powerUpInput, out int powerUpNumber);
        BasePowerUp numberedPowerUp = CheatManager.GetPowerUpByNumber(powerUpNumber);
        if (GUILayout.Button($"Add: {(numberedPowerUp != null ? numberedPowerUp.PowerUpName : "-")}"))
        {
            CheatManager.AddPowerUpByNumber(powerUpNumber);
        }

        GUILayout.Label("Consumable #");
        _consumableInput = GUILayout.TextField(_consumableInput);

        int.TryParse(_consumableInput, out int consumableNumber);
        BaseConsumable numberedConsumable = CheatManager.GetConsumableByNumber(consumableNumber);
        string consumableName = numberedConsumable != null ? numberedConsumable.ConsumableName : "-";

        if (GUILayout.Button($"Add: {consumableName}"))
        {
            CheatManager.AddConsumableByNumber(consumableNumber);
        }

        if (GUILayout.Button($"Use: {consumableName}"))
        {
            CheatManager.UseConsumableByNumber(consumableNumber);
        }

        int currentLevel = BallEnhanceManager.Instance != null ? BallEnhanceManager.Instance.GetLevel(TestBallEnhanceType, TestBallEnhanceAxis) : 0;
        if (GUILayout.Button($"Upgrade Enhance: {TestBallEnhanceType}/{TestBallEnhanceAxis} (Lvl {currentLevel})"))
        {
            CheatManager.UpgradeBallEnhance(TestBallEnhanceType, TestBallEnhanceAxis);
        }

        GUILayout.EndArea();
    }
}
