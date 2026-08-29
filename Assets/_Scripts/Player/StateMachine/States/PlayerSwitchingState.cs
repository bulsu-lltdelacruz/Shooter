using UnityEngine;

public class PlayerSwitchingState : PlayerState
{
    //float baseCooldown = .375f;
    //float deltaTime;

    private int weaponSlot;
    GameObject gunRotateParent;
    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 initialStartPosition;
    Quaternion initialStartRotation;
    float initialTime;
    float holdTime;

    Vector3 holdPosition;
    Quaternion holdRotation;
    private bool didSwitch = false;
    public PlayerSwitchingState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        //base.baseCooldown = .375f-.05f;
        base.baseCooldown = player.playerStats.weaponSwitchDuration;
        gunRotateParent = player.cameraHandler.gunRotateParent;
        holdPosition =  new Vector3(0, -0.14f, 0);
        holdRotation = Quaternion.Euler(new Vector3(45f, 0, 0));
        initialTime = baseCooldown / 10; //player.playerStats.init;
        holdTime = baseCooldown / 2; //player.playerStats.hold;
    }
    public override void EnterState()
    {
        initialStartPosition = gunRotateParent.transform.localPosition;
        initialStartRotation = gunRotateParent.transform.localRotation;
    }
    public override void ExitState()
    {
        gunRotateParent.transform.localPosition = Vector3.zero;
        gunRotateParent.transform.localRotation = Quaternion.identity;
        didSwitch = false;
    }
    public override void FrameUpdate()
    {
        deltaTime += Time.deltaTime;
        if (deltaTime < initialTime + holdTime)//if NOT halfway through, DONT start lerping dicksuckers
        {
            if(deltaTime < initialTime)
            {
                startPosition = gunRotateParent.transform.localPosition = Vector3.Slerp(initialStartPosition, holdPosition, deltaTime / initialTime);
                startRotation = gunRotateParent.transform.localRotation = Quaternion.Slerp(initialStartRotation, holdRotation, deltaTime / initialTime);
            }else
            {
                if(!didSwitch)
                {
                    player.playerInventory.TrySwitchWeapon(weaponSlot);
                    didSwitch = true;
                }
                startPosition = gunRotateParent.transform.localPosition = holdPosition;
                startRotation = gunRotateParent.transform.localRotation = holdRotation;
            }
                return;
        }
        gunRotateParent.transform.localPosition = Vector3.Slerp(startPosition, Vector3.zero, deltaTime/baseCooldown);
        gunRotateParent.transform.localRotation = Quaternion.Slerp(startRotation, Quaternion.identity, deltaTime / baseCooldown);

        if (deltaTime > baseCooldown)
        {
            gunRotateParent.transform.localPosition = Vector3.zero;
            gunRotateParent.transform.localRotation = Quaternion.identity;
            deltaTime = 0f;
            player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
        }
    }
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
    public void SetWeaponSlot(int slot)
    {
        this.weaponSlot = slot;
    }
}
