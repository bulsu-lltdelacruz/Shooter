using System;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class Player : MonoBehaviour, IDamageable
{
    InputHandler playerInputs;
    CameraHandler cameraHandler;
    PlayerMovement playerMovement;
    CharacterController characterController;
    #region ANIMATIONS
    [SerializeField]Animator armAnimator;
    [SerializeField]Animator gunAnimator;
    public AnimationEventBus animationEventBus;
    #endregion ANIMATIONs
    #region
    PlayerStateMachine playerStateMachine;
    public PlayerStateController PlayerStateController;

    PlayerShootingState playerShootingState;
    PlayerIdleState playerIdleState;
    PlayerReloadState playerReloadState;
    PlayerReload2State playerReload2State;
    PlayerMeleeState playerMeleeState;
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
       

        #region STATE MACHINES
        //states
        playerShootingState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerIdleState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerReloadState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerReload2State = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerMeleeState = new(this, playerStateMachine, armAnimator, gunAnimator);
        //states
        playerStateMachine = new();
        PlayerStateController = new(playerStateMachine, 
            playerIdleState, 
            playerShootingState,
            playerReloadState,
            playerMeleeState, 
            playerReload2State);
        playerStateMachine.Initialize(playerIdleState);
        #endregion STATE MACHINES

        #region SET INSTANCES
        cameraHandler.SetInstances(playerInputs, playerStateMachine);
        #endregion SET INSTANCES

    }
    void Update()
    {
        playerStateMachine.currentState.FrameUpdate();
        PlayerStateController.CheckContinuousInputs();
    }
    private void FixedUpdate()
    {
        playerStateMachine.currentState.PhysUpdate();
    }
    public bool AllowJump()
    {
        return characterController.isGrounded;
    }
    public void Damage()
    {
        
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        throw new NotImplementedException();
    }
}
