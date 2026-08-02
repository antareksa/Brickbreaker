using UnityEngine;

// Subscribes to the game state machine so any HUD can react to state changes without each one
// wiring up the subscription itself. Override OnEnterState/OnExitState in a subclass to do
// something on a specific transition -- both are no-ops by default.
public abstract class BaseHUD : MonoBehaviour
{
    protected GameState CurrentState { get; private set; }

    // Start (not OnEnable/Awake) -- Start is the only lifecycle method Unity guarantees runs
    // after every object's Awake has already completed, so GameManager.Instance is safe to use
    // here regardless of GameObject processing order.
    protected virtual void Start()
    {
        GameManager.Instance.StateMachine.OnStateChanged.AddListener(HandleStateChanged);
    }

    protected virtual void OnDisable()
    {
        GameManager.Instance.StateMachine.OnStateChanged.RemoveListener(HandleStateChanged);
    }

    private void HandleStateChanged(GameState newState)
    {
        OnExitState(CurrentState);
        CurrentState = newState;
        OnEnterState(newState);
    }

    protected virtual void OnEnterState(GameState state) { }
    protected virtual void OnExitState(GameState state) { }
}
