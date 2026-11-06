using System;
using UnityEngine;

public class PlayerStateController
{
    PlayerStateMachine playerStateMachine;
    PlayerStateMachine locomotionStateMachine;
    PlayerStateMachine leftStateMachine;
    PlayerIdleState playerIdleState;
    PlayerSwitchingState playerSwitchingState;
    PlayerShootingState playerShootingState;
    PlayerReloadState playerReloadState;
    PlayerReload2State playerReload2State;
    PlayerMeleeState playerMeleeState;
    PlayerDashMeleeState playerDashMeleeState;
    PlayerSpellState playerSpellState;
    PlayerChargingAltFireState playerChargingAltFireState;

    LocoPlayerWalkingState playerWalkState;
    LocoPlayerDashingState playerDashState;
    LocoPlayerSlammingState playerSlamState;
    LocoPlayerGrappleState playerGrappleState;

    PlayerLeftIdleState playerLeftIdleState;
    PlayerLeftThrowState playerLeftThrowState;

    Player player;

    #region bools for continuous inputs
    public bool isAttacking;
    public bool isMeleeing;
    public bool isChargingAltFire;
    public bool allowAltFireCharging = true;
    #endregion bools for continuous inputs

    public string DEBUG_GET_CURRENT_STATE()
    {
        return playerStateMachine.currentState.ToString();
    }
    public PlayerStateController(PlayerStateMachine playerStateMachine,
        PlayerStateMachine locomotionStateMachine, Player player,
        PlayerIdleState playerIdleState, PlayerSwitchingState playerSwitchingState, PlayerShootingState playerShootingState,
        PlayerDashMeleeState playerDashMeleeState,
        PlayerChargingAltFireState playerChargingAltFireState,
        PlayerReloadState playerReloadState, PlayerMeleeState playerMeleeState,
        PlayerReload2State playerReload2State, LocoPlayerWalkingState playerWalkState,
        LocoPlayerDashingState playerDashState, LocoPlayerSlammingState playerSlamState,
        LocoPlayerGrappleState playerGrappleState, PlayerSpellState playerSpellState)
    {
        this.playerStateMachine = playerStateMachine;
        this.locomotionStateMachine = locomotionStateMachine;
        this.playerIdleState = playerIdleState;
        this.playerSwitchingState = playerSwitchingState;
        this.playerShootingState = playerShootingState;
        this.playerReloadState = playerReloadState;
        this.playerReload2State = playerReload2State;
        this.playerMeleeState = playerMeleeState;
        this.playerDashMeleeState = playerDashMeleeState;
        this.playerSpellState = playerSpellState;
        this.playerChargingAltFireState = playerChargingAltFireState;

        this.playerWalkState = playerWalkState;
        this.playerDashState = playerDashState;
        this.playerSlamState = playerSlamState;
        this.playerGrappleState = playerGrappleState;
        this.player = player;
    }
    public void InitializeLeftStates(PlayerStateMachine leftStateMachine,
        PlayerLeftIdleState playerLeftIdleState, PlayerLeftThrowState playerLeftThrowState)
    {
        this.leftStateMachine = leftStateMachine;
        this.playerLeftIdleState = playerLeftIdleState;
        this.playerLeftThrowState = playerLeftThrowState;
        deltaTime = reloadTime;
    }
    public bool TryChangeState(E_PlayerStates newState)
    {
        if (playerStateMachine == null)
            return false;
        switch (newState)
        {
            case E_PlayerStates.IDLE:
                return TryIdle();
            case E_PlayerStates.SHOOTING:
                return TryShoot();
            case E_PlayerStates.CHARGINGALTFIRE:
                return TryChargeAltFire();
            case E_PlayerStates.RELOAD:
                return TryReload();
            case E_PlayerStates.RELOAD2:
                return TryReload2();
            case E_PlayerStates.MELEE:
                return TryMelee();
            case E_PlayerStates.SWITCH:
                return TrySwitch();
            default:
                return false;
        }
    }
    private bool TryIdle()
    {
        if (playerStateMachine.currentState is PlayerIdleState)
            return true;
        playerStateMachine.ChangeState(playerIdleState);
        return true;
    }
    float deltaTime;
    public float reloadTime;
    private bool TryShoot()
    {
        if (playerStateMachine.currentState == player.playerChargingAltFireState)
        {
            if(PlayerStatus.Instance.spellCharge < PlayerStatus.Instance.maxSpellCharge)
                return false;
            playerStateMachine.ChangeState(playerSpellState);
            PlayerStatus.Instance.UseSpellCharge();
            return true;
        }
        if (playerStateMachine.currentState is PlayerSwitchingState || playerStateMachine.currentState is PlayerReloadState
            || !player.playerInventory.activeSlot.activeItem.weaponClass.canShoot || playerStateMachine.currentState is PlayerSpellState)
            return false;
        if (playerStateMachine.currentState is PlayerShootingState)
            return true;
        playerStateMachine.ChangeState(playerShootingState);
        return true;
    }
    private bool TryChargeAltFire()
    {
        if (playerStateMachine.currentState is PlayerChargingAltFireState)
            return true;
        if (playerStateMachine.currentState is PlayerShootingState || playerStateMachine.currentState is PlayerDashMeleeState || playerStateMachine.currentState is PlayerSpellState
            || playerStateMachine.currentState is PlayerReloadState)
            return false;
        playerStateMachine.ChangeState(playerChargingAltFireState);
        return true;
    }
    private bool TryReload()
    {
        if (playerStateMachine.currentState is PlayerSwitchingState)
            return false;
        if (player.playerWalkingState.isGrappling)
            return false;
        if (playerStateMachine.currentState is PlayerReloadState)
            return true;
        if (//playerStateMachine.currentState is PlayerShootingState ||
            playerStateMachine.currentState is PlayerReload2State ||
             playerStateMachine.currentState is PlayerMeleeState || playerStateMachine.currentState is PlayerDashMeleeState)
            return false;
        playerStateMachine.ChangeState(playerReloadState);
        return true;
    }
    private bool TryReload2()
    {
        if (playerStateMachine.currentState is PlayerSwitchingState)
            return false;
        if (player.playerWalkingState.isGrappling)
            return false;
        if (playerStateMachine.currentState is PlayerReload2State)
            return true;
        if (playerStateMachine.currentState is PlayerShootingState ||
            playerStateMachine.currentState is PlayerReloadState ||
             playerStateMachine.currentState is PlayerMeleeState || playerStateMachine.currentState is PlayerDashMeleeState)
            return false;
        playerStateMachine.ChangeState(playerReload2State);
        return true;
    }
    private bool TryMelee()
    {
        if (playerStateMachine.currentState == player.playerChargingAltFireState)
        {
            if (playerStateMachine.currentState is PlayerDashMeleeState)
                return true;
            if (playerStateMachine.currentState is PlayerShootingState)
                return false;
            playerStateMachine.ChangeState(playerDashMeleeState);
            return true;
        }
        else
        {
            if (playerStateMachine.currentState is PlayerMeleeState)
                return true;
            if (playerStateMachine.currentState is PlayerShootingState || playerStateMachine.currentState is PlayerDashMeleeState)
                return false;
            playerStateMachine.ChangeState(playerMeleeState);
            return true;
        }
        
    }
    private bool TrySwitch()
    {
        if (playerStateMachine.currentState is PlayerDashMeleeState || playerStateMachine.currentState is PlayerShootingState)
            return false;
        playerStateMachine.ChangeState(playerSwitchingState);
        return true;
    }
    public void CheckContinuousInputs()
    {
        //playerShootingState.
        if (isAttacking)
            TryShoot();
        if (isMeleeing)
            TryMelee(); 
        if (isChargingAltFire && allowAltFireCharging)
            TryChargeAltFire();
       // else
            //TryIdle();
    }

    #region simple movement bools
    public bool TryChangeState(E_PlayerMovementStates newState)
    {
        if (locomotionStateMachine == null)
            return false;
        switch (newState)
        {
            case E_PlayerMovementStates.WALK:
                return TryWalk();
            case E_PlayerMovementStates.DASH:
                return TryDash();
            case E_PlayerMovementStates.SLAM:
                return TrySlam();
            case E_PlayerMovementStates.GRAPPLE:
                return TryGrapple();
            default:
                return false;
        }
    }
    private bool TryWalk()
    {
        if (playerStateMachine.currentState is LocoPlayerWalkingState)
            return true;
        if (playerStateMachine.currentState is LocoPlayerDashingState ||
            playerStateMachine.currentState is LocoPlayerSlammingState)
            return false;
        locomotionStateMachine.ChangeState(playerWalkState);
        return true;

    }
    private bool TryDash()
    {
        if (playerStateMachine.currentState is LocoPlayerDashingState)
            return true;
        if (playerStateMachine.currentState is LocoPlayerSlammingState)
            return false;
        locomotionStateMachine.ChangeState(playerDashState);
        return true;

    }
    private bool TrySlam()
    {
        if (playerStateMachine.currentState is LocoPlayerSlammingState)
            return true;
        if (playerStateMachine.currentState is LocoPlayerDashingState)
            return false;
        locomotionStateMachine.ChangeState(playerSlamState);
        return true;

    }
    #region LEFT ARM STATES
    public bool TryChangeState(E_PlayerLeftStates newState)
    {
        if (playerStateMachine == null)
            return false;
        switch (newState)
        {
            case E_PlayerLeftStates.IDLE:
                return TryLeftIdle();
            case E_PlayerLeftStates.THROW:
                return TryLeftThrow();
            default:
                return false;
        }
    }
    private bool TryLeftIdle()
    {
        if (leftStateMachine.currentState is PlayerLeftIdleState)
            return true;
        leftStateMachine.ChangeState(playerLeftIdleState);
        return true;

    }
    private bool TryLeftThrow()
    {
        if (leftStateMachine.currentState is PlayerLeftThrowState)
            return true;
        leftStateMachine.ChangeState(playerLeftThrowState);
        return true;

    }
    #endregion
    //force calls
    public bool ForceWalkState()
    {
        if (playerStateMachine.currentState is LocoPlayerWalkingState)
            return true;
        locomotionStateMachine.ChangeState(playerWalkState);
        return true;

    }
    public bool TryGrapple()
    {
        if (playerStateMachine.currentState is LocoPlayerGrappleState)
        {
            Debug.Log("already grappling");
            return true;
        }
            
        locomotionStateMachine.ChangeState(playerGrappleState);
        return true;
    }
    #endregion
    public PlayerState GetCurrentLocoState()
    {
        return locomotionStateMachine.currentState;
    }
    public PlayerState GetCurrentPlayerState()
    {
        return playerStateMachine.currentState;
    }
}

public enum E_PlayerMovementStates
{
    WALK,
    DASH,
    SLAM,
    GRAPPLE
}
public enum E_PlayerStates
{
    IDLE,
    SHOOTING,
    CHARGINGALTFIRE,
    RELOAD,
    RELOAD2,
    MELEE,
    SWITCH
}
public enum E_PlayerLeftStates
{
    IDLE,
    THROW
}
