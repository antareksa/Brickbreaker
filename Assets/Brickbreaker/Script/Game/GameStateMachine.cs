using UnityEngine;
using UnityEngine.Events;

public class GameStateMachine : MonoBehaviour
{
    public GameState CurrentState { get; private set; } = GameState.Aiming;

    public UnityEvent<GameState> OnStateChanged = new UnityEvent<GameState>();

    public void ChangeState(GameState newState)
    {
        if (newState == CurrentState) return;

        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}
