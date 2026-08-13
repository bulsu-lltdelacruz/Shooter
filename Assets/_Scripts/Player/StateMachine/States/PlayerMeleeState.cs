using UnityEngine;

public class PlayerMeleeState : PlayerState
{
    //float baseCooldown = .375f;
    //float deltaTime;
    public PlayerMeleeState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        //base.baseCooldown = .375f-.05f;
        base.baseCooldown = .375f - .1f;
    }
    string val;
    public override void EnterState()
    {
        val = Random.Range(1,3).ToString();
        gunAnimator.SetTrigger($"GunMelee{val}");
        armAnimator.SetTrigger($"ArmMelee{val}");
    }
    public override void ExitState()
    {
        //reset stretch, the other one is in armanimcontroller
        player.animationEventBus.armEvents.evtRevertArmMelee?.Invoke();
        gunAnimator.ResetTrigger($"GunMelee{val}");
        armAnimator.ResetTrigger($"ArmMelee{val}");
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
