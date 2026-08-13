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
        gunAnimator.SetTrigger("GunReload1");
        armAnimator.SetTrigger("ArmReload1");
    }
    public override void ExitState()
    {
        gunAnimator.ResetTrigger("GunReload1");
        armAnimator.ResetTrigger("ArmReload1");
    }
    public override void FrameUpdate()
    {
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
