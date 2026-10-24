using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class Player : Entity
{
    InputHandler playerInputs;
    UI_InputHandler uiInputs;
    public CameraHandler cameraHandler;
    [NonSerialized]public PlayerStats playerStats;
    [NonSerialized] public CharacterController characterController;
    [SerializeField] public Transform grenadeOrigin;
    #region ANIMATIONS
    [SerializeField]public Animator armAnimator;
    [Header("just the thing first, gets replaced inscript, probably temporary")]
    [SerializeField]public Animator gunAnimator;
    public AnimationEventBus animationEventBus;
    #endregion ANIMATIONs
    #region
    public PlayerStateMachine playerStateMachine;
    public PlayerStateMachine playerLocomotionStateMachine;
    public PlayerStateMachine leftStateMachine;
    public PlayerStateController PlayerStateController;

    public PlayerShootingState playerShootingState;
    public PlayerIdleState playerIdleState;
    public PlayerSwitchingState playerSwitchingState;
    PlayerReloadState playerReloadState;
    PlayerReload2State playerReload2State;
    public PlayerMeleeState playerMeleeState;
    public PlayerChargingAltFireState playerChargingAltFireState;
    public PlayerDashMeleeState playerDashMeleeState;
    public float SPACEDFDF = 0;
    //locmotion states

    public LocoPlayerWalkingState playerWalkingState;
    public LocoPlayerDashingState playerDashingState;
    public LocoPlayerSlammingState playerSlamState;
    public LocoPlayerGrappleState playerGrappleState;
    //left hand state
    public PlayerLeftIdleState playerLeftIdleState;
    public PlayerLeftThrowState playerLeftThrowState;
    #endregion
    #region ATTACK RELATED STUFF
    [NonSerialized]public WeaponController weaponController; 
    [NonSerialized]public GrenadeController grenadeController;
    [SerializeField] public MeleeHitboxDetector meleeCollider;
    #endregion

    #region Inventory
    [Header("Inventory")]
    [NonSerialized]public PlayerInventory playerInventory;
    public UIHandler uiHandler;
    #endregion
    #region UI DOCS
    [SerializeField] private UIDocument HUDDoc;
    private VisualElement HPBar;
    #endregion
    private void Awake()
    {
        EntityManager.Instance.RegisterPlayer(gameObject, GetComponent<IDamageable>());
        playerInputs = new InputHandler(this);
        uiInputs = new(this, playerInputs.GetInputActions());
        playerInventory = GetComponent<PlayerInventory>();
        uiHandler = new(playerInventory.inventoryTablet, playerInputs, uiInputs, this);
        cameraHandler = GetComponent<CameraHandler>();
        playerStats = GetComponent<PlayerStats>();
        playerStats.SetInstances(playerInputs);
        characterController = GetComponent<CharacterController>();
        animationEventBus = GetComponent<AnimationEventBus>();
        weaponController = GetComponent<WeaponController>();
        grenadeController = GetComponent<GrenadeController>();
    }
    void Start()
    {
        //states
        playerStateMachine = new();
        playerLocomotionStateMachine = new();
        leftStateMachine = new();

        #region STATE MACHINES
        //states
        playerShootingState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerIdleState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerReloadState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerSwitchingState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerReload2State = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerMeleeState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerDashMeleeState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerChargingAltFireState = new(this, playerStateMachine, armAnimator, gunAnimator);
        //locomotion states
        playerWalkingState = new(this, playerStateMachine, playerInputs, armAnimator, gunAnimator);
        playerDashingState = new(this, playerStateMachine, playerInputs, armAnimator, gunAnimator);
        playerSlamState = new(this, playerStateMachine, armAnimator, gunAnimator);
        playerGrappleState = new(this, playerStateMachine, armAnimator, gunAnimator);
        //left state
        playerLeftThrowState = new(this, playerStateMachine, playerInputs, armAnimator, gunAnimator);
        playerLeftIdleState = new(this, playerStateMachine, playerInputs, armAnimator, gunAnimator);


        PlayerStateController = new PlayerStateController(playerStateMachine, playerLocomotionStateMachine, this,
            playerIdleState,
            playerSwitchingState,
            playerShootingState,
        playerDashMeleeState,
        playerChargingAltFireState,
        playerReloadState,
            playerMeleeState, 
            playerReload2State,
            playerWalkingState,
            playerDashingState,
            playerSlamState,
            playerGrappleState);
        PlayerStateController.InitializeLeftStates(leftStateMachine, playerLeftIdleState, playerLeftThrowState);

        playerInventory.Initialize();
        playerStateMachine.Initialize(playerIdleState);
        playerLocomotionStateMachine.Initialize(playerWalkingState);
        leftStateMachine.Initialize(playerLeftIdleState);
        grenadeController.Initialize(this);
        #endregion STATE MACHINES

        #region SET INSTANCES
        cameraHandler.SetInstances(playerInputs, playerStateMachine);
        #endregion SET INSTANCES
        currentHP = maxHP;
        //Init UI stuff
        HPBar = HUDDoc.rootVisualElement.Q<VisualElement>("hp-bar");
    }
    private void Update()
    {
        base.FrameUpdate();
        playerStateMachine.currentState.FrameUpdate();
        playerLocomotionStateMachine.currentState.FrameUpdate();
        leftStateMachine.currentState.FrameUpdate();
        //Debug.Log("" + playerStateMachine.currentState);
        PlayerStateController.CheckContinuousInputs();
    }
    private void FixedUpdate()
    {
        playerStateMachine.currentState.PhysUpdate();
        playerLocomotionStateMachine.currentState.PhysUpdate();
        leftStateMachine.currentState.PhysUpdate();
    }
    public bool AllowJump()
    {
        return playerWalkingState.jumpCounter > 0;
    }
    public void Damage()
    {
        
    }
    public override void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        //Debug.Log("Damaged with " + amount +" ugh ouchies");
        Debug.Log("ARAY, "+amount);
        currentHP = Mathf.Max(0, currentHP-=amount);
        float hpPercent = (currentHP / maxHP) * 100;
        SoundFXManager.instance.PlaySoundFXClip(PlayerSounds.Instance.hit_sound, transform, .3f, SoundType.SOUNDFX);
        HPBar.style.width = Length.Percent(hpPercent* 0.9583f);//kasi di mas maliit sa loob 
        if (currentHP <=0)
        {
            Debug.Log("Die");
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
    public override void Heal(float healAmount)
    {
        currentHP = Mathf.Clamp(currentHP + healAmount, 0, maxHP);
        Debug.Log($"{currentHP} from {healAmount} amount of healing");
        float hpPercent = (currentHP / maxHP) * 100;
        HPBar.style.width = Length.Percent(hpPercent* 0.9583f);//kasi di mas maliit sa loob 
    }
    /*public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        throw new NotImplementedException();
    }

    public void TakeDamageWithImpact(float amount, float impactAmount, Vector3 source)
    {

    }*/
}
