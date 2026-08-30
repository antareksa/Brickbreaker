using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LauncherControllerV2 : MonoBehaviour
{
    public Transform LaunchPosition;
    public TMP_Text BallCountText;
    public float LaunchDelay;

    // Large ball counts fire in groups instead of one full LaunchDelay per ball -- balls within
    // the same group only wait GroupedLaunchDelay (small) between each other, and the full
    // LaunchDelay only happens once between groups. Group size starts at 1 (every ball is its
    // own group -- identical to the old one-delay-per-ball behavior) and grows by 1 for every
    // GroupSizeStep balls owned, so a big roster still launches in a reasonable amount of time.
    [Header("Launch Grouping (large ball counts)")]
    public float GroupedLaunchDelay = 0.1f;
    public int GroupSizeStep = 50;

    [Header("Aim Line")]
    public LineRenderer AimLineRenderer;
    public SpriteRenderer AimEndMarker;
    public float AimLineMaxDistance = 20f;

    // Defaults match the actual wall bounds (x: -3 to 3, top wall y ~4) confirmed earlier, with a
    // bit of headroom below the launcher -- tune directly in the Inspector, not in code.
    [Header("Input restriction")]
    public Vector2 InputZoneMin = new Vector2(-3f, -6f);
    public Vector2 InputZoneMax = new Vector2(3f, 4f);
    public float MaxAimAngle = 75f;

    public UnityEvent OnShotFinished = new UnityEvent();
    public UnityEvent OnShotStarted = new UnityEvent();

    private Vector2 _direction;
    private bool _isPickingTarget;
    private bool _isAimValid;

    private int _activeBallCount;
    private bool _allLaunchedThisShot;
    private bool _hasFirstReturnX;
    private float _firstReturnX;
    private Vector3 _initialLaunchPosition;

    private LaunchManager _launchManager;

    // Subscribing in Awake (not Start) guarantees this runs before LaunchManager.Start spawns the
    // initial balls, since Unity calls Awake on every object before Start on any of them -- so no
    // ball, whenever it's added, is ever missed regardless of script execution order.

    private void Start()
    {
        _initialLaunchPosition = LaunchPosition.position;
        HideAimLine();

        GameManager.Instance.BrickManager.OnGameOver.AddListener(HandleGameOver);

        // No game has started yet -- MainMenuHUD's Start button calls RestartGame(), which
        // calls ResetLauncher() and re-enables this.
        enabled = false;
    }

    // Stops accepting input once the game is over -- ResetLauncher (called on restart, from
    // either the GameOverHUD or MainMenuHUD flow) re-enables this.
    private void HandleGameOver()
    {
        enabled = false;
    }

    public void InitializeLauncher(LaunchManager launchManager)
    {
        _launchManager = launchManager;
        _launchManager.OnBallAdded.AddListener(HandleBallAdded);
    }

    private void HandleBallAdded(BallControllerV2 ball)
    {
        ball.ReturnY = LaunchPosition.position.y;
        ball.OnReturned.AddListener(HandleBallReturned);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool overUI = IsPointerOverUI();
            bool withinZone = IsWithinInputZone();
            bool isAiming = GameManager.Instance.StateMachine.CurrentState == GameState.Aiming;
            _isPickingTarget = !overUI && withinZone && isAiming;

            //Debug.Log($"[Launcher] mouseDown overUI={overUI} withinZone={withinZone} isAiming={isAiming} -> isPickingTarget={_isPickingTarget}");
        }

        if (_isPickingTarget && Input.GetMouseButton(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = LaunchPosition.position.z;
            Vector2 rawDirection = ((Vector2)worldPos - (Vector2)LaunchPosition.position).normalized;

            float angle = Vector2.SignedAngle(Vector2.up, rawDirection);
            _isAimValid = angle >= -MaxAimAngle && angle <= MaxAimAngle;

            //Debug.Log($"[Launcher] aim angle = {angle:F1} degrees from straight up, valid={_isAimValid}");

            // Out of range disables aiming entirely (no clamp-to-edge) -- direction/line just
            // freeze at the last valid value until the drag comes back within MaxAimAngle.
            if (_isAimValid)
            {
                _direction = rawDirection;
                UpdateAimLine();
            }
            else
            {
                HideAimLine();
            }
        }

        if (Input.GetMouseButtonUp(0) && _isPickingTarget)
        {
            if (_isAimValid)
            {
                Launch();
            }

            _isPickingTarget = false;
            HideAimLine();
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // Aiming can only START inside this rectangle (roughly the playable field) -- keeps clicks
    // way outside the field from counting as aim input.
    private bool IsWithinInputZone()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        bool withinZone = worldPos.x >= InputZoneMin.x && worldPos.x <= InputZoneMax.x
            && worldPos.y >= InputZoneMin.y && worldPos.y <= InputZoneMax.y;

        //Debug.Log($"[Launcher] worldPos={worldPos} zoneMin={InputZoneMin} zoneMax={InputZoneMax} withinZone={withinZone}");

        return withinZone;
    }

    // Shows where the ball will actually hit -- not just a straight line to wherever the finger
    // currently is. Sweeps the same radius the real ball's own collision detection uses (not a
    // zero-width ray), otherwise the preview can show a near-miss on a brick that the ball's
    // actual physical width still clips once launched.
    private static readonly RaycastHit2D[] _aimLineHitBuffer = new RaycastHit2D[8];

    private void UpdateAimLine()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = false;

        float ballRadius = _launchManager.Balls.Count > 0 ? _launchManager.Balls[0].WorldRadius : 0f;

        int count = Physics2D.CircleCast(LaunchPosition.position, ballRadius, _direction, filter, _aimLineHitBuffer, AimLineMaxDistance);

        Vector3 endPointWorld = (Vector3)((Vector2)LaunchPosition.position + _direction * AimLineMaxDistance);
        float closestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (_aimLineHitBuffer[i].distance < closestDistance)
            {
                closestDistance = _aimLineHitBuffer[i].distance;
                endPointWorld = _aimLineHitBuffer[i].point;
            }
        }

        // AimLineRenderer is a child of LaunchPosition with useWorldSpace off, so its own
        // transform origin already sits at the launch point -- point 0 is just local zero, and
        // the world-space hit point needs converting into that same local space.
        AimLineRenderer.positionCount = 2;
        AimLineRenderer.SetPosition(0, Vector3.zero);
        AimLineRenderer.SetPosition(1, AimLineRenderer.transform.InverseTransformPoint(endPointWorld));

        // transform.position always sets world position regardless of parenting, so this works
        // whatever the marker's own hierarchy is.
        AimEndMarker.enabled = true;
        AimEndMarker.transform.position = endPointWorld;
    }

    private void HideAimLine()
    {
        AimLineRenderer.positionCount = 0;
        AimEndMarker.enabled = false;
    }

    public void Launch()
    {
        GameManager.Instance.StateMachine.ChangeState(GameState.Shooting);
        OnShotStarted?.Invoke();
        BallCountText.gameObject.SetActive(true);
        GameManager.Instance.SoundManager.Play(SoundType.Shoot);
        StartCoroutine(LaunchCoroutine());
    }

    // Stops any balls still in flight and puts shot-tracking / the launch position back to how
    // they were before the first shot -- used when restarting the game.
    public void ResetLauncher()
    {
        enabled = true;

        StopAllCoroutines();

        foreach (BallControllerV2 ball in _launchManager.Balls)
        {
            ball.Stop();
        }

        _activeBallCount = 0;
        _allLaunchedThisShot = false;
        _hasFirstReturnX = false;
        _isPickingTarget = false;
        LaunchPosition.position = _initialLaunchPosition;
        HideAimLine();
    }

    // Every ball just shoots from the same start position in the same direction, offset
    // in time by LaunchDelay. No leader/follower bookkeeping needed: since BallControllerV2's
    // bounce is a deterministic function of (direction, wall), each ball independently
    // retraces the same path on its own -- it doesn't need to be told the path by another ball.
    IEnumerator LaunchCoroutine()
    {
        _activeBallCount = 0;
        _allLaunchedThisShot = false;
        _hasFirstReturnX = false;

        int totalBalls = _launchManager.Balls.Count;

        // groupSize 1 (totalBalls < GroupSizeStep) makes every ball complete its own group,
        // which is exactly the original always-use-LaunchDelay behavior.
        int groupSize = 1 + (totalBalls / GroupSizeStep);

        for (int i = 0; i < totalBalls; i++)
        {
            _launchManager.Balls[i].Shoot(LaunchPosition.position, _direction, i);
            _activeBallCount++;

            BallCountText.text = "x" + (totalBalls - _activeBallCount);

            bool completedGroup = (i + 1) % groupSize == 0;
            yield return new WaitForSeconds(completedGroup ? LaunchDelay : GroupedLaunchDelay);
        }
        BallCountText.gameObject.SetActive(false);
        _allLaunchedThisShot = true;
        CheckShotFinished();
    }

    // Lets the player skip waiting for every ball to naturally bounce back down. Since the next
    // launch position is normally taken from whichever ball touches bottom first, we approximate
    // that here by using whichever still-flying ball is currently lowest (closest to returning on
    // its own) -- then force-stops everything and finishes the shot immediately. Meant to be wired
    // to a UI button.
    public void SkipShot()
    {
        StopAllCoroutines();

        if (!_hasFirstReturnX)
        {
            BallControllerV2 lowestBall = FindLowestActiveBall();
            if (lowestBall != null)
            {
                _hasFirstReturnX = true;
                _firstReturnX = lowestBall.transform.position.x;
            }
        }

        foreach (BallControllerV2 ball in _launchManager.Balls)
        {
            ball.Stop();
        }

        BallCountText.gameObject.SetActive(false);
        _activeBallCount = 0;
        _allLaunchedThisShot = true;
        CheckShotFinished();
    }

    // Different from SkipShot: this skips the wave BEFORE any shot happens at all -- only valid
    // while the player is still picking a target (no balls in flight yet). Just fires the same
    // "shot finished" trigger a completed shot normally would, without touching the launch
    // position (no ball ever flew, so there's nothing to base a new position on).
    public void SkipWave()
    {
        if (_activeBallCount > 0) return;

        OnShotFinished?.Invoke();
    }

    private BallControllerV2 FindLowestActiveBall()
    {
        BallControllerV2 lowest = null;
        float lowestY = float.MaxValue;

        foreach (BallControllerV2 ball in _launchManager.Balls)
        {
            if (!ball.gameObject.activeSelf) continue;

            if (ball.transform.position.y < lowestY)
            {
                lowestY = ball.transform.position.y;
                lowest = ball;
            }
        }

        return lowest;
    }

    private void HandleBallReturned(BallControllerV2 ball)
    {
        if (!_hasFirstReturnX)
        {
            _hasFirstReturnX = true;
            _firstReturnX = ball.transform.position.x;
        }

        _activeBallCount--;
        CheckShotFinished();
    }

    private void CheckShotFinished()
    {
        if (_allLaunchedThisShot && _activeBallCount <= 0)
        {
            if (_hasFirstReturnX)
            {
                Vector3 launchPos = LaunchPosition.position;
                launchPos.x = _firstReturnX;
                LaunchPosition.position = launchPos;
            }

            OnShotFinished?.Invoke();
        }
    }
}
