using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BrickController : MonoBehaviour
{
    public TMP_Text HitPointText;
    public Animator Animator;

    public Vector2Int GridPosition { get; private set; }
    public bool IsGold { get; private set; }

    // How many times THIS brick has been hit so far this shot -- for PowerUp effects like
    // "bonus damage on re-hitting the same brick". Reset via ResetShotState, called by
    // BrickManager when a new shot starts (not when the brick spawns).
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
