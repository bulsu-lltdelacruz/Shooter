using UnityEngine;

public class PlayerReload2State : PlayerState
{
    public PlayerReload2State(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        base.baseCooldown = .917f;
    }
    public override void EnterState()
    {
        gunAnimator.SetTrigger("GunReload2");
        armAnimator.SetTrigger("ArmReload2");
    }
    public override void ExitState()
    {

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
