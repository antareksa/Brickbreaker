using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherController : MonoBehaviour
{
    public Transform LaunchPosition;
    public float LaunchDelay;
    public BallController BallControllerPrefab;

    private List<BallController> _balls = new List<BallController>();
    private Vector3 _direction;
    private bool _isPickingTarget;
    private int _startingBallTotal = 30;

    private void Start()
    {
        for (int i = 0; i < _startingBallTotal; i++)
        {
            BallController ball = Instantiate(BallControllerPrefab);
            ball.Stop();

            ball.OnDestroyBrick.AddListener(PromoteNextBallToLeader);

            _balls.Add(ball);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _isPickingTarget = true;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = transform.position.z;
            _direction = (worldPos - LaunchPosition.position).normalized;
        }
        if (Input.GetMouseButtonUp(0) && _isPickingTarget)
        {
            Launch();
            _isPickingTarget = false;
        }
    }

    public void Launch()
    {
        StartCoroutine(LaunchCoroutine());
    }

    IEnumerator LaunchCoroutine()
    {
        BallController leader = _balls[0];

        for (int i = 0; i < _balls.Count; i++)
        {
            _balls[i].Shoot(i, 0, LaunchPosition.position, _direction, leader);
            yield return new WaitForSeconds(LaunchDelay);
        }
    }

    // Called generically by ANY ball when it changes the world (destroys a box, etc.)
    public void PromoteNextBallToLeader(BallController ballThatChangedWorld)
    {
        int changedIndex = _balls.IndexOf(ballThatChangedWorld);
        if (changedIndex < 0 || changedIndex + 1 >= _balls.Count) return; // no next ball to promote

        BallController newLeader = _balls[changedIndex + 1];
        newLeader.PromoteToLeader();

        // Every ball after the new leader now follows it instead of the old leader
        for (int i = changedIndex + 2; i < _balls.Count; i++)
        {
            _balls[i].SwitchTrackingTarget(newLeader, newLeader.MyIndex);
        }
    }
}