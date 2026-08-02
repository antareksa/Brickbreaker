using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallController : MonoBehaviour
{
    public struct PathPoint
    {
        public float distance;
        public Vector3 position;
    }

    public float Speed;
    public string BounceTag = "Bouncy";
    public int MyIndex { get; private set; }
    public LauncherController Launcher;

    public UnityEvent<BallController> OnDestroyBrick = new UnityEvent<BallController>();

    private bool _isMoving = false;
    private int _leaderIndex;
    private Vector3 _targetDirection;

    private List<PathPoint> _myPath = new List<PathPoint>();
    private BallController _trackingTarget;
    private float _traveledDistance;

    public List<PathPoint> Path => _myPath;

    public void Shoot(int myIndex, int leaderIndex, Vector3 startPosition, Vector3 direction, BallController trackingTarget)
    {
        gameObject.SetActive(true);
        transform.position = startPosition;
        _isMoving = true;

        MyIndex = myIndex;
        _leaderIndex = leaderIndex;
        _traveledDistance = 0f;

        if (MyIndex == _leaderIndex)
        {
            _targetDirection = direction.normalized;
            _myPath.Clear();
            _myPath.Add(new PathPoint { distance = 0f, position = startPosition });
        }
        else
        {
            _trackingTarget = trackingTarget;
        }
    }

    public void SwitchTrackingTarget(BallController newTarget, int newLeaderIndex)
    {
        _trackingTarget = newTarget;
        _leaderIndex = newLeaderIndex;
    }

    public void PromoteToLeader()
    {
        _leaderIndex = MyIndex;

        _targetDirection = _trackingTarget != null
            ? GetCurrentDirectionFromPath()
            : transform.right;

        _myPath.Clear();
        _myPath.Add(new PathPoint { distance = _traveledDistance, position = transform.position });
    }

    private Vector3 GetCurrentDirectionFromPath()
    {
        List<PathPoint> path = _trackingTarget.Path;
        if (path.Count < 2) return transform.right;

        Vector3 a = path[path.Count - 2].position;
        Vector3 b = path[path.Count - 1].position;
        return (b - a).normalized;
    }

    void Update()
    {
        if (!_isMoving) return;

        _traveledDistance += Speed * Time.deltaTime;

        if (MyIndex == _leaderIndex) MoveAsLeader();
        else MoveAsFollower();
    }

    void MoveAsLeader()
    {
        transform.position += _targetDirection * Speed * Time.deltaTime;
        _myPath.Add(new PathPoint { distance = _traveledDistance, position = transform.position });
    }

    void MoveAsFollower()
    {
        if (_trackingTarget == null) return;
        List<PathPoint> path = _trackingTarget.Path;
        if (path == null || path.Count == 0) return;

        float d = _traveledDistance;
        if (d >= path[path.Count - 1].distance)
        {
            transform.position = path[path.Count - 1].position;
            return;
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            if (d >= path[i].distance && d <= path[i + 1].distance)
            {
                float t = Mathf.InverseLerp(path[i].distance, path[i + 1].distance, d);
                transform.position = Vector3.Lerp(path[i].position, path[i + 1].position, t);
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (MyIndex != _leaderIndex) return;

        if (other.TryGetComponent<BrickController>(out BrickController brickController))
        {
            Debug.Log("brick hitted!");
            //if(brickController.OnGetHit(1))
            //{
            //    OnDestroyBrick?.Invoke(this);
            //}
        }

        if (!other.CompareTag(BounceTag)) return;

        Vector2 closestPoint = other.ClosestPoint(transform.position);
        Vector3 normal = ((Vector3)closestPoint - transform.position).normalized * -1f;

        _targetDirection = Vector3.Reflect(_targetDirection, normal).normalized;
    }

    public void Stop()
    {
        gameObject.SetActive(false);
        _isMoving = false;
    }
}