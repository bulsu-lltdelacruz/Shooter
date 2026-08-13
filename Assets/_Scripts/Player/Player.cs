using System;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class Player : MonoBehaviour, IDamageable
{
    InputHandler playerInputs;
    CameraHandler cameraHandler;
    public PlayerMovement playerMovement;
    public CharacterController characterController;
    #region ANIMATIONS
    [SerializeField]Animator armAnimator;
    [SerializeField]Animator gunAnimator;
    public AnimationEventBus animationEventBus;
    #endregion ANIMATIONs
    #region
    public PlayerStateMachine playerStateMachine;
    public PlayerStateMachine playerLocomotionStateMachine;
    public PlayerStateController PlayerStateController;

    PlayerShootingState playerShootingState;
    PlayerIdleState playerIdleState;
    PlayerReloadState playerReloadState;
    PlayerReload2State playerReload2State;
    PlayerMeleeState playerMeleeState;
    public float SPACEDFDF = 0;
    //locmotion states

    public LocoPlayerWalkingState playerWalkingState;
    public LocoPlayerDashingState playerDashingState;
    public LocoPlayerSlammingState playerSlamState;
    public LocoPlayerGrappleState playerGrappleState;
    #endregion
    #region ATTACK RELATED STUFF
    [NonSerialized]public WeaponController weaponController;
    #endregion

    private void Awake()
    {
        playerInputs = new InputHandler(this);
        cameraHandler = GetComponent<CameraHandler>();
        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.SetInstances(playerInputs);
        characterController = GetComponent<CharacterController>();
        animationEventBus = GetComponent<AnimationEventBus>();
        weaponController = GetComponent<WeaponController>();
    }
    void Start()
    {
        //states
        playerStateMachine = new();
        playerLocomotionStateMachine = new();

        #region STATE MACHINES
        //states
        playerShootingState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerIdleState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerReloadState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerReload2State = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerMeleeState = new(this, playerStateMachine, armAnimator, gunAnimator);
        //locomotion states
        playerWalkingState = new(this, playerStateMachine, playerInputs, armAnimator, gunAnimator);
        playerDashingState = new(this, playerStateMachine, playerInputs, armAnimator, gunAnimator);
        playerSlamState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerGrappleState = new(this, playerStateMachine, armAnimator, gunAnimator);



        PlayerStateController = new(playerStateMachine, playerLocomotionStateMachine,
            playerIdleState, 
            playerShootingState,
            playerReloadState,
            playerMeleeState, 
            playerReload2State,
            playerWalkingState,
            playerDashingState,
            playerSlamState,
            playerGrappleState);
        playerStateMachine.Initialize(playerIdleState);
        playerLocomotionStateMachine.Initialize(playerWalkingState);
        #endregion STATE MACHINES

        #region SET INSTANCES
        cameraHandler.SetInstances(playerInputs, playerStateMachine);
        #endregion SET INSTANCES

    }
    void Update()
    {
        playerStateMachine.currentState.FrameUpdate();
        playerLocomotionStateMachine.currentState.FrameUpdate();
        PlayerStateController.CheckContinuousInputs();
    }
    private void FixedUpdate()
    {
        playerStateMachine.currentState.PhysUpdate();
        playerLocomotionStateMachine.currentState.PhysUpdate();
    }
    public bool AllowJump()
    {
        return playerWalkingState.jumpCounter > 0;
    }
    public void Damage()
    {
        
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        throw new NotImplementedException();
    }
}
