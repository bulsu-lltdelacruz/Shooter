using System.Diagnostics;
using UnityEngine;

public class EnemyStateMachine
{
    public EnemyState currentState;
    public EnemyState previousState;

    public void Initialize(EnemyState startingState)
    {
        previousState = startingState;
        currentState = startingState;
        if (currentState != null)
            currentState.EnterState();
    }
    public void ChangeState(EnemyState newState)
    {
        if (newState == null)
            return;
        previousState = currentState;
        //UnityEngine.Debug.Log($"exit :  {currentState}");
        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();

    }
}