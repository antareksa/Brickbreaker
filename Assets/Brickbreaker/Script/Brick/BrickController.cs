using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BrickController : MonoBehaviour
{
    public TMP_Text HitPointText;
    public Animator Animator;
    public SpriteRenderer SpriteRenderer;

    public Color GoldColor = new Color(1f, 0.84f, 0.1f);

    // Curated so consecutive hues stay clearly distinct against each other and the background --
    // one entry per HpColorStep worth of HP. Cycles by hue every Palette.Length * HpColorStep HP;
    // ShadeLevels then darkens each full lap so it takes ShadeLevels laps (Palette.Length *
    // HpColorStep * ShadeLevels HP -- 1000 HP with the defaults below) before a tier's color is
    // an exact repeat of an earlier one.
    private static readonly Color[] HpColorPalette =
    {
        new Color(0.20f, 0.70f, 0.30f), // green
        new Color(0.20f, 0.60f, 0.70f), // teal
        new Color(0.25f, 0.45f, 0.85f), // blue
        new Color(0.55f, 0.35f, 0.85f), // purple
        new Color(0.80f, 0.30f, 0.75f), // magenta
        new Color(0.85f, 0.30f, 0.40f), // rose
        new Color(0.85f, 0.45f, 0.20f), // orange
        new Color(0.80f, 0.65f, 0.15f), // amber
        new Color(0.55f, 0.55f, 0.55f), // grey
        new Color(0.35f, 0.20f, 0.20f), // deep red
    };

    private const int HpColorStep = 10;
    private const int ShadeLevels = 10;

    // How long the spawn scale-in takes -- eases from 0 up to the prefab's own scale instead of
    // just appearing at full size.
    public float SpawnScaleDuration = 0.3f;

    private Coroutine _moveRoutine;
    private Coroutine _spawnScaleRoutine;
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    // Animator bool parameter driven by SetDanger -- true while this brick will breach the bottom
    // next wave. A bool rather than a trigger since it's a persistent state that also has to turn
    // back OFF (e.g. the "bricks don't descend this wave" consumable clears the warning).
    private const string DangerAnimatorBool = "isDanger";

    public Vector2Int GridPosition { get; private set; }
    public bool IsGold { get; private set; }

    // OBSOLETE -- nothing reads this anymore. It used to back PowerUp #8 ("bonus damage on
    // re-hitting the same brick"), but as a per-BRICK counter incremented inside DamageBrick it
    // counted hits from every source (other balls, Fire/Row/Column spread, skill and consumable
    // damage), which made that PowerUp wildly overtuned -- one Column ball splashing 10 bricks
    // bumped all 10 counters. Repeat-hit is now tracked per (ball, brick) on BallControllerV2
    // and surfaced via BallHitContext.RepeatHitsOnBrick, so only the same ball actually striking
    // the same brick again earns it. Still incremented below purely to keep this consistent if
    // anything picks it back up; safe to delete along with ResetShotState.
    public int HitsThisShot { get; private set; }

    public UnityEvent<BrickController> OnDestroyed = new UnityEvent<BrickController>();

    private int _hitPoint;
    private bool _isSpawned;

    // Scene-placed bricks that BrickManager doesn't own (e.g. hand-placed test bricks) still get
    // a default HP so existing setups keep working. Bricks spawned via BrickManager call Spawn()
    // themselves before Start() runs, so this fallback is skipped for those.
    private void Start()
    {
        if (!_isSpawned)
            Spawn(5, GridPosition);
    }

    public void Spawn(int hitPoint, Vector2Int gridPosition)
    {
        _hitPoint = hitPoint;
        GridPosition = gridPosition;
        _isSpawned = true;
        HitPointText.text = _hitPoint.ToString();

        PlaySpawnAnimation();
        RefreshColor();
    }

    // Gold overrides the HP-tier color entirely; otherwise the color steps every HpColorStep HP
    // so a large HP change reads as a visibly different brick, not just a smaller number.
    private void RefreshColor()
    {
        if (SpriteRenderer == null) return;

        SpriteRenderer.color = IsGold ? GoldColor : GetHpColor(_hitPoint);
    }

    private static Color GetHpColor(int hitPoint)
    {
        int tier = Mathf.Max(0, hitPoint) / HpColorStep;
        int hueIndex = tier % HpColorPalette.Length;
        int shadeIndex = (tier / HpColorPalette.Length) % ShadeLevels;

        Color.RGBToHSV(HpColorPalette[hueIndex], out float h, out float s, out float v);
        v *= 1f - (shadeIndex / (float)ShadeLevels) * 0.5f;
        return Color.HSVToRGB(h, s, Mathf.Clamp01(v));
    }

    private void PlaySpawnAnimation()
    {
        if (_spawnScaleRoutine != null) StopCoroutine(_spawnScaleRoutine);
        _spawnScaleRoutine = StartCoroutine(SpawnScaleRoutine());
    }

    private IEnumerator SpawnScaleRoutine()
    {
        transform.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < SpawnScaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Easing.EaseOutBack(Mathf.Clamp01(elapsed / SpawnScaleDuration));
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, _baseScale, t);
            yield return null;
        }

        transform.localScale = _baseScale;
        _spawnScaleRoutine = null;
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }

    // Visually eases toward targetPosition instead of snapping -- GridPosition/logical state is
    // already updated by the caller before this runs, so this is purely cosmetic. Restarting an
    // in-flight move (rather than stacking coroutines) keeps a brick from fighting itself if it's
    // told to descend again before the previous move finished.
    public void MoveTo(Vector3 targetPosition, float duration)
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveToRoutine(targetPosition, duration));
    }

    private IEnumerator MoveToRoutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Easing.EaseOutBack(Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
        _moveRoutine = null;
    }

    public void SetGold(bool isGold)
    {
        IsGold = isGold;
        RefreshColor();
    }

    public void SetDanger(bool isInDanger)
    {
        if (Animator == null) return;

        // Back to normal pace -- a brick only ever runs fast during the beat where it's actually
        // sitting on the bottom, and this is the point where that's no longer true.
        Animator.speed = 1f;
        Animator.SetBool(DangerAnimatorBool, isInDanger);
    }

    // Bumped while a brick is sitting on the bottom about to be cleared, so the same danger loop
    // reads as more urgent than the one-wave-out warning.
    public void SetAnimatorSpeed(float speed)
    {
        if (Animator == null) return;
        Animator.speed = speed;
    }

    // OBSOLETE -- only resets HitsThisShot, which nothing reads anymore (see above).
    public void ResetShotState()
    {
        HitsThisShot = 0;
    }

    public void DamageBrick(int damage)
    {
        HitsThisShot++;

        _hitPoint -= damage;
        HitPointText.text = _hitPoint.ToString();
        RefreshColor();

        GameManager.Instance.AddHitScore(damage);
        GameManager.Instance.SoundManager.Play(SoundType.HitBlock);

        if (_hitPoint <= 0 )
        {
            OnDestroyed?.Invoke(this);
            GameManager.Instance.AddDestroyScore(damage);
            GameManager.Instance.SoundManager.Play(SoundType.Destroyed);
            Destroy(gameObject);
        }

        Animator.SetTrigger("Hit");
    }
}
