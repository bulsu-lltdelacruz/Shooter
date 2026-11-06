using UnityEngine;

public class PlayerShootingState : PlayerState
{
    BaseWeaponClass weaponState;
    public PlayerShootingState(
        Player player, 
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) : 
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        base.baseCooldown = .05f;
    }
    public override void EnterState()
    {
        gunAnimator = player.gunAnimator;
        baseCooldown = player.weaponController.data.fireRate;
        weaponState = player.weaponController.weaponState;

        

        //gunAnimator.SetTrigger("GunFire");
        //armAnimator.SetTrigger("ArmFire");
        bool isSuccessful = player.weaponController.TryFirePressed();
        if (!isSuccessful)
            return;
        gunAnimator.SetTrigger(player.playerInventory.activeSlot.activeItem.weaponClass.GUN_shootTrigger);
        armAnimator.SetTrigger(player.playerInventory.activeSlot.activeItem.weaponClass.ARM_shootTrigger);
    }
    public override void ExitState()
    {
        if (weaponState.doesRequireReload)
        {
            //do crossbow.shotgun mechanics
        }
        gunAnimator = player.gunAnimator;
    }
    /*private float deltaReloadTime;
    private float reloadTime;
    public bool CheckWeaponCanAttack()
    {
        deltaReloadTime += Time.deltaTime;
        if (deltaReloadTime > reloadTime)
        {
            deltaReloadTime = 0f;
        }
    }*/
    public override void FrameUpdate()
    {
        //setscale back to 1
        deltaTime += Time.deltaTime;
        if (deltaTime > baseCooldown)
        {
            if(player.playerInventory.activeSlot.activeItem.weaponClass.doesRequireReload)
            {
                player.PlayerStateController.TryChangeState(E_PlayerStates.RELOAD);
            }else
            {
                player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
            }
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
