using System.Diagnostics;
using UnityEngine;

public class PlayerStateMachine
{
    public PlayerState currentState;
    public PlayerState previousState;

    public void Initialize(PlayerState startingState)
    {
        previousState = startingState;
        currentState = startingState;
        if (currentState != null)
            currentState.EnterState();
    }
    public void ChangeState(PlayerState newState)
    {
        if (newState == null)
            return;
        previousState = currentState;

        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();

    }
}