using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BrickController : MonoBehaviour
{
    public TMP_Text HitPointText;
    public Animator Animator;

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
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }

    public void SetGold(bool isGold)
    {
        IsGold = isGold;
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
