using UnityEngine;

public class PlayerIdleState : PlayerState
{
    // Remember exactly what EnterState switched on. ExitState clears *those* triggers,
    // no matter what the inventory's active item has become in the meantime.
    // (Previously ExitState read activeItem at exit time, so if the active item had
    // already been swapped, the NEW weapon's trigger names were cleared and the OLD
    // weapon's idle bools stayed true -> two idle bools on at once.)
    private Animator enteredGunAnimator;
    private BaseWeaponClass enteredWeapon;
    private bool isIdle;

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
        // Never stack idle on top of idle.
        ClearIdle();

        Slot slot = player.playerInventory.activeSlot;
        if (slot == null || slot.activeItem == null)
        {
            Debug.LogWarning("Idle: no active weapon to play idle for");
            return;
        }

        gunAnimator = player.gunAnimator;
        enteredGunAnimator = gunAnimator;
        enteredWeapon = slot.activeItem.weaponClass;

        if (enteredGunAnimator != null)
            enteredGunAnimator.SetBool(enteredWeapon.GUN_idleTrigger, true);
        if (armAnimator != null)
            armAnimator.SetBool(enteredWeapon.ARM_idleTrigger, true);

        isIdle = true;

        if (!player.playerInventory.activeSlot.activeItem.weaponClass.canShoot)
        {
            player.PlayerStateController.TryChangeState(E_PlayerStates.RELOAD);
        }
    }

    public override void ExitState()
    {
        ClearIdle();
    }

    private void ClearIdle()
    {
        if (!isIdle) return;

        // Unity-null checks: the old gun object may already be destroyed.
        if (enteredWeapon != null)
        {
            if (enteredGunAnimator != null)
                enteredGunAnimator.SetBool(enteredWeapon.GUN_idleTrigger, false);
            if (armAnimator != null)
                armAnimator.SetBool(enteredWeapon.ARM_idleTrigger, false);
        }

        enteredGunAnimator = null;
        enteredWeapon = null;
        isIdle = false;
    }

    public override void FrameUpdate()
    {

    }

    public override void PhysUpdate()
    {

    }

    public override void AnimationTriggerEvent()
    {

    }
}