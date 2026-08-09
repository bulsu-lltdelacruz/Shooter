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
        gunAnimator.SetBool("GunIsIdle", true);
        armAnimator.SetBool("ArmIsIdle", true);
    }
    public override void ExitState()
    {
        gunAnimator.SetBool("GunIsIdle", false);
        armAnimator.SetBool("ArmIsIdle", false);
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
