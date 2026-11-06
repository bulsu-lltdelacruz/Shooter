using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStateMachine
{
    public PlayerState currentState;
    public PlayerState nextState;// can only be used on exit state since it becomes currentstatae anyways
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
        nextState = newState;
        previousState = currentState;
        //UnityEngine.Debug.Log($"exit :  {currentState}");
        currentState?.ExitState();
        currentState = nextState;
        currentState?.EnterState();

    }
}