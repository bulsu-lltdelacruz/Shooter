using UnityEngine;

public class PlayerStateController
{
    PlayerStateMachine playerStateMachine; 
    PlayerIdleState playerIdleState; 
    PlayerShootingState playerShootingState;
    PlayerReloadState playerReloadState;
    PlayerReload2State playerReload2State;
    PlayerMeleeState playerMeleeState;

    #region bools for continuous inputs
    public bool isAttacking;
    public bool isMeleeing;
    #endregion bools for continuous inputs

    public string DEBUG_GET_CURRENT_STATE()
    {
        return playerStateMachine.currentState.ToString();
    }
    public PlayerStateController(PlayerStateMachine playerStateMachine,
        PlayerIdleState playerIdleState, PlayerShootingState playerShootingState,
        PlayerReloadState playerReloadState, PlayerMeleeState playerMeleeState,
        PlayerReload2State playerReload2State) 
    { 
        this.playerStateMachine = playerStateMachine; 
        this.playerIdleState = playerIdleState;
        this.playerShootingState = playerShootingState;
        this.playerReloadState = playerReloadState;
        this.playerReload2State = playerReload2State;
        this.playerMeleeState = playerMeleeState;
    }
    public bool TryChangeState(E_PlayerStates newState)
    {
        if(playerStateMachine == null)
            return false;
        switch (newState)
        {
            case E_PlayerStates.IDLE:
                return TryIdle();
            case E_PlayerStates.SHOOTING:
                return TryShoot();
            case E_PlayerStates.RELOAD:
                return TryReload();
            case E_PlayerStates.RELOAD2:
                return TryReload2();
            case E_PlayerStates.MELEE:
                return TryMelee();
            default:
                return false;
        }
    }
    private bool TryIdle()
    {
        if(playerStateMachine.currentState is PlayerIdleState)
            return true;
        playerStateMachine.ChangeState(playerIdleState);
        return true;
    }
    private bool TryShoot()
    {
        if (playerStateMachine.currentState is PlayerShootingState)
            return true;
        playerStateMachine.ChangeState(playerShootingState);
        return true;
    }
    private bool TryReload()
    {
        if (playerStateMachine.currentState is PlayerReloadState)
            return true;
        if (playerStateMachine.currentState is PlayerShootingState||
            playerStateMachine.currentState is PlayerReload2State||
             playerStateMachine.currentState is PlayerMeleeState)
            return false;
        playerStateMachine.ChangeState(playerReloadState);
        return true;
    }
    private bool TryReload2()
    {
        if (playerStateMachine.currentState is PlayerReload2State)
            return true;
        if (playerStateMachine.currentState is PlayerShootingState ||
            playerStateMachine.currentState is PlayerReloadState ||
             playerStateMachine.currentState is PlayerMeleeState)
            return false;
        playerStateMachine.ChangeState(playerReload2State);
        return true;
    }
    private bool TryMelee()
    {
        if (playerStateMachine.currentState is PlayerMeleeState)
            return true;
        if (playerStateMachine.currentState is PlayerShootingState)
            return false;
        playerStateMachine.ChangeState(playerMeleeState);
        return true;
    }
    public void CheckContinuousInputs()
    {
        if(isAttacking)
            TryShoot(); 
        if (isMeleeing)
            TryMelee();
    }
}
public enum E_PlayerStates
{
    IDLE,
    SHOOTING,
    RELOAD,
    RELOAD2,
    MELEE
}
