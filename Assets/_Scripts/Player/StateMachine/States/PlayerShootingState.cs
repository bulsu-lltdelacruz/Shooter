using UnityEngine;

public class PlayerShootingState : PlayerState
{
    public PlayerShootingState(
        Player player, 
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) : 
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        base.baseCooldown = .25f;
    }
    public override void EnterState()
    {
        gunAnimator = player.gunAnimator;
        gunAnimator.SetTrigger(player.playerInventory.activeSlot.activeItem.weaponClass.GUN_shootTrigger);
        armAnimator.SetTrigger(player.playerInventory.activeSlot.activeItem.weaponClass.ARM_shootTrigger);
        //gunAnimator.SetTrigger("GunFire");
        //armAnimator.SetTrigger("ArmFire");
        player.weaponController.TryFirePressed();
    }
    public override void ExitState()
    {
        gunAnimator = player.gunAnimator;
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
