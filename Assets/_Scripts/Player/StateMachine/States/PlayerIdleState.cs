using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {

    }
    public override void EnterState()
    {
        gunAnimator = player.gunAnimator;
        gunAnimator.SetBool(player.playerInventory.activeSlot.activeItem.weaponClass.GUN_idleTrigger, true);
        armAnimator.SetBool(player.playerInventory.activeSlot.activeItem.weaponClass.ARM_idleTrigger, true);
    }
    public override void ExitState()
    {
        if (player.playerInventory.activeSlot == null)
        {
            Debug.LogWarning("Active slot is currently null");
            return;//-0.35, 40 pistol, -0.75 ,40 shotgun
        }
        gunAnimator = player.gunAnimator;
        gunAnimator.SetBool(player.playerInventory.activeSlot.activeItem.weaponClass.GUN_idleTrigger, false);
        armAnimator.SetBool(player.playerInventory.activeSlot.activeItem.weaponClass.ARM_idleTrigger, false);
    }
    public override void FrameUpdate()
    {
        //Debug.Log(player.playerInventory.activeSlot.activeItem.weaponClass.GUN_idleTrigger);
    }
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
}
