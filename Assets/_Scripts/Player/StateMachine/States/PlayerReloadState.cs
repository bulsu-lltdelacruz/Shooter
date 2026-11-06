using UnityEngine;

public class PlayerReloadState : PlayerState
{
    public PlayerReloadState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        base.baseCooldown = .875f;
    }
    public override void EnterState()
    {
        player.playerInventory.activeSlot.activeItem.weaponClass.canShoot = false;
        gunAnimator = player.gunAnimator;
        gunAnimator.SetTrigger(player.playerInventory.activeSlot.activeItem.weaponClass.GUN_reloadTrigger);
        armAnimator.SetTrigger(player.playerInventory.activeSlot.activeItem.weaponClass.ARM_reloadTrigger);
        //gunAnimator.SetTrigger("GunReload1");
        //armAnimator.SetTrigger("ArmReload1");
        baseCooldown = player.playerInventory.activeSlot.activeItem.data.reloadTime;
    }
    public override void ExitState()
    {
        deltaTime = 0;
        gunAnimator = player.gunAnimator;
        gunAnimator.ResetTrigger(player.playerInventory.activeSlot.activeItem.weaponClass.GUN_reloadTrigger);
        gunAnimator.SetBool("Idle", true);
        armAnimator.ResetTrigger(player.playerInventory.activeSlot.activeItem.weaponClass.ARM_reloadTrigger);
        //gunAnimator.ResetTrigger("GunReload1");
        //armAnimator.ResetTrigger("ArmReload1");
    }
    public override void FrameUpdate()
    {
        deltaTime += Time.deltaTime;
        if (deltaTime >= baseCooldown *.7f)
        {
            player.playerInventory.activeSlot.activeItem.weaponClass.canShoot = true;
        }
        if (deltaTime >= baseCooldown)
        {
            deltaTime = 0f; 
            player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
        }
        /*if (player.playerInventory.activeSlot.activeItem.weaponClass.canShoot)
        {
            player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
        }*/
    }
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
}
