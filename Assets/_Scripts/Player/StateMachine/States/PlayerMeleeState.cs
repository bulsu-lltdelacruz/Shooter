using UnityEngine;

public class PlayerMeleeState : PlayerState
{
    //float baseCooldown = .375f;
    //float deltaTime;
    Vector3 gunCamOrigPosition;
    public PlayerMeleeState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        //base.baseCooldown = .375f-.05f;
        base.baseCooldown = .375f-.07f; //- .1f;
    }
    string val;
    public override void EnterState()
    {
        gunCamOrigPosition = player.cameraHandler.gunCamera.transform.localPosition;
        player.cameraHandler.gunCamera.transform.localPosition = new Vector3(gunCamOrigPosition.x, gunCamOrigPosition.y, -0.35f);
        player.playerInventory.HideWeaponGameobject(true);
        val = Random.Range(1,3).ToString();
        //gunAnimator.SetTrigger($"GunMelee{1}");
        armAnimator.SetTrigger($"ArmMelee{1}");
    }
    public override void ExitState()
    {
        player.cameraHandler.gunCamera.transform.localPosition = gunCamOrigPosition;
        player.playerInventory.HideWeaponGameobject(false);
        //reset stretch, the other one is in armanimcontroller
        player.animationEventBus.armEvents.evtRevertArmMelee?.Invoke();
        //gunAnimator.ResetTrigger($"GunMelee{1}");
        armAnimator.ResetTrigger($"ArmMelee{1}");
    }
    public override void FrameUpdate()
    {
        //setscale back to 1
        deltaTime += Time.deltaTime;
        if (deltaTime > baseCooldown)
        {
            player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
            deltaTime = 0f;
        }
    }
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
}
