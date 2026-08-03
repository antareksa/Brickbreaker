using UnityEngine;
using UnityEngine.Events;

public class BallControllerV2 : MonoBehaviour
{
    public string BallName;
    public Rarity Rarity;
    public Sprite BallIcon;
    public SpriteRenderer BallIconRenderer;
    public float Speed = 5f;
    public string BounceTag = "Bouncy";

    // Avoids the player being stuck watching one ball bounce forever: every RampInterval seconds
    // spent still moving, Speed gets multiplied by RampMultiplier. Reset back to the original
    // configured Speed each time this ball is (re)launched via Shoot().
    [Header("Speed Ramp")]
    public float RampInterval = 3f;
    public float RampMultiplier = 1.5f;

    [Header("Debug")]
    public bool DrawPath = true;
    public Color PathColor = Color.cyan;
    public float PathDuration = Mathf.Infinity;
    public bool LogBounces = true;

    // Below this Y, the ball is considered to have flown past the bottom of the field and is
    // returned (stopped) rather than left to fly forever -- there's no bottom wall to bounce off.
    // Set by whoever launches this ball (e.g. the launcher sets it to its own launch height).
    public float ReturnY;
    public UnityEvent<BallControllerV2> OnReturned = new UnityEvent<BallControllerV2>();

    private const int MaxBouncesPerFrame = 8;

    // Small gap kept between the ball and a surface it just touched. Without this, moving to
    // hit.distance exactly (i.e. touching) means the very next cast from that same spot can
    // immediately redetect the same collider at ~0 distance and get stuck bouncing in place.
    private const float SkinWidth = 0.01f;

    private static readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[8];

    public float WorldRadius => _worldRadius;

    private CircleCollider2D _collider;
    private BaseBallHitEffect[] _hitEffects;
    private float _worldRadius;
    private Vector2 _direction;
    private bool _isMoving;
    private float _baseSpeed;
    private float _rampTimer;

    // Per-shot state for PowerUp effect context -- all reset in Shoot(), read by
    // BaseBallHitEffect via CurrentHitContext whenever this ball hits a brick.
    private int _bouncesThisShot;
    private bool _hasHitAnyBrickThisShot;
    private int _fireIndexThisShot;
    private bool _pendingSideWallBonus;

    public BallHitContext CurrentHitContext { get; private set; }

    // Persists across frames (not just within one Update's bounce loop): a phantom re-hit against
    // the wall we just bounced off can happen on the very next frame's first cast just as easily as
    // later in the same frame, since each ball's frames land at a different real-time phase relative
    // to that boundary. Only cleared once a cast confirms open space ahead.
    private Collider2D _lastHitCollider;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _hitEffects = GetComponents<BaseBallHitEffect>();
        _worldRadius = _collider.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        _baseSpeed = Speed;
    }

    private void Start()
    {
        BallIconRenderer.sprite = BallIcon;
    }

    public void Shoot(Vector3 startPosition, Vector2 direction, int fireIndexThisShot = 0)
    {
        gameObject.SetActive(true);
        transform.position = startPosition;
        _direction = direction.normalized;
        _isMoving = true;
        _lastHitCollider = null;
        Speed = _baseSpeed;
        _rampTimer = 0f;

        _bouncesThisShot = 0;
        _hasHitAnyBrickThisShot = false;
        _fireIndexThisShot = fireIndexThisShot;
        _pendingSideWallBonus = false;
    }

    public void Stop()
    {
        _isMoving = false;
        gameObject.SetActive(false);
    }

    // Instead of moving blindly and waiting for the physics engine's own timestep to report an
    // overlap (which happens on a different clock than Update, so the exact overshoot before it's
    // noticed varies frame to frame / ball to ball), we ask up front "how far can I go before I'd
    // touch something" and stop exactly there. That makes the bounce point a pure function of
    // position + direction + geometry -- identical for every ball, no physics-step timing involved.
    void Update()
    {
        if (!_isMoving) return;

        _rampTimer += Time.deltaTime;
        if (_rampTimer >= RampInterval)
        {
            _rampTimer -= RampInterval;
            Speed *= RampMultiplier;
        }

        float remainingDistance = Speed * Time.deltaTime;
        int bounces = 0;

        while (remainingDistance > 0f && bounces < MaxBouncesPerFrame)
        {
            RaycastHit2D hit = CastIgnoringSelf(transform.position, _direction, remainingDistance, _lastHitCollider);

            if (!hit)
            {
                MoveAndDraw(_direction * remainingDistance);
                remainingDistance = 0f;
                _lastHitCollider = null;
                break;
            }

            float moveDistance = Mathf.Max(hit.distance - SkinWidth, 0f);
            MoveAndDraw(_direction * moveDistance);
            remainingDistance -= hit.distance;

            // CircleCast's raw normal is exact for a flat face, but right at a collider's corner it
            // blends continuously based on the precise sub-pixel contact angle -- and that precise
            // angle is exactly what varies by a hair between balls (each samples a different slice
            // of Unity's naturally jittery per-frame Time.deltaTime on its way there). Snapping to
            // whichever of the collider's own face directions is closest collapses any such jitter
            // back onto the same discrete result for every ball.
            Vector2 normal = SnapNormalToNearestFace(hit.collider, hit.normal);

            // Stopping short of the wall we just hit isn't enough near a corner, where a second
            // collider can already be touching the ball too. Push clear of THIS collider using its
            // own (snapped) normal so, after both corner surfaces get their turn in this loop, the
            // ball ends up separated from both instead of settling back into one of them.
            transform.position += (Vector3)(normal * SkinWidth);

            bool handled = false;

            if (hit.collider.TryGetComponent<BrickController>(out BrickController brickController))
            {
                handled = true;
                OnHitBrick(brickController);
            }

            if (hit.collider.CompareTag(BounceTag))
            {
                handled = true;
                Vector2 oldDirection = _direction;
                _direction = Reflect(_direction, normal);
                _bouncesThisShot++;

                // Side wall = horizontal normal (left/right), as opposed to top/bottom. Sets a
                // one-shot flag consumed by the next brick hit, not a running count -- a later
                // side bounce before that hit just keeps it true, doesn't stack.
                if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
                {
                    _pendingSideWallBonus = true;
                }

                GameManager.Instance.SoundManager.Play(SoundType.WallBounce);

                if (LogBounces)
                {
                    Debug.Log($"[{name}] bounce #{bounces} vs '{hit.collider.name}' " +
                        $"pos={transform.position} hitDist={hit.distance:F5} rawNormal={hit.normal} snappedNormal={normal} " +
                        $"dirIn={oldDirection} dirOut={_direction}", this);
                }
            }

            if (!handled) break;

            _lastHitCollider = hit.collider;
            bounces++;
        }

        if (_isMoving && transform.position.y <= ReturnY)
        {
            Stop();
            GameManager.Instance.SoundManager.Play(SoundType.BallReturn);
            OnReturned?.Invoke(this);
        }
    }

    private void MoveAndDraw(Vector2 step)
    {
        if (DrawPath)
            Debug.DrawRay(transform.position, step, PathColor, PathDuration);

        transform.position += (Vector3)step;
    }

    // Sweeps the ball's own radius forward so the reported hit distance is where its edge would
    // touch, not its center -- and skips its own collider, any other ball, and (within this
    // frame's bounce loop) whatever collider it just bounced off. That last exclusion is what
    // actually guarantees no phantom re-hit: a distance/skin threshold can flake right at Unity's
    // own contact offset, but "not the same collider object" can't.
    private RaycastHit2D CastIgnoringSelf(Vector2 origin, Vector2 direction, float distance, Collider2D excludeCollider)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = false;

        int count = Physics2D.CircleCast(origin, _worldRadius, direction, filter, _hitBuffer, distance);

        RaycastHit2D closest = default;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            RaycastHit2D hit = _hitBuffer[i];

            if (hit.collider.gameObject == gameObject) continue;
            if (hit.collider == excludeCollider) continue;
            if (hit.collider.GetComponent<BallControllerV2>() != null) continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closest = hit;
            }
        }

        return closest;
    }

    // Picks whichever of the collider's own 4 box-face directions is closest to the raw contact
    // normal. For a genuine flat-face hit this is a no-op (rawNormal already equals one of them
    // almost exactly). Near a corner it discretizes an otherwise continuously-varying value.
    private static Vector2 SnapNormalToNearestFace(Collider2D collider, Vector2 rawNormal)
    {
        Transform t = collider.transform;
        Vector2 right = t.right;
        Vector2 up = t.up;

        Vector2[] faceNormals = { right, -right, up, -up };

        Vector2 best = faceNormals[0];
        float bestAlignment = float.MinValue;

        foreach (Vector2 candidate in faceNormals)
        {
            float alignment = Vector2.Dot(rawNormal, candidate);
            if (alignment > bestAlignment)
            {
                bestAlignment = alignment;
                best = candidate;
            }
        }

        return best;
    }

    // X = D - 2 * (D . A) * A
    private static Vector2 Reflect(Vector2 direction, Vector2 normal)
    {
        return direction - 2f * Vector2.Dot(direction, normal) * normal;
    }

    private void OnHitBrick(BrickController brickController)
    {
        if (brickController == null) return;

        BrickManager brickManager = GameManager.Instance.BrickManager;

        // Captured before flipping _hasHitAnyBrickThisShot -- HitWallBeforeAnyBrick and
        // IsFirstHitThisShot are only ever true on the first brick hit of the shot, never on
        // later hits.
        CurrentHitContext = new BallHitContext
        {
            BouncesThisShot = _bouncesThisShot,
            FireIndexThisShot = _fireIndexThisShot,
            HitWallBeforeAnyBrick = _bouncesThisShot > 0 && !_hasHitAnyBrickThisShot,
            SideWallBounceSinceLastHit = _pendingSideWallBonus,
            IsFirstHitThisShot = !_hasHitAnyBrickThisShot,
            BricksRemainingOnField = brickManager != null ? brickManager.GetBrickCount() : 0,
            BricksDestroyedThisShot = brickManager != null ? brickManager.BricksDestroyedThisShot : 0,
            HitBrick = brickController,
        };
        _hasHitAnyBrickThisShot = true;
        _pendingSideWallBonus = false;

        foreach (BaseBallHitEffect hitEffect in _hitEffects)
        {
            hitEffect.OnHitBrick(brickController);
        }
    }
}
