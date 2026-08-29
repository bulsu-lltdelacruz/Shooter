using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLeftIdleState : PlayerState
{

    public PlayerLeftIdleState(
        Player player,
        PlayerStateMachine playerStateMachine,
        InputHandler playerInputs,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {


    }
    public override void EnterState()
    {
        //armAnimator.ResetTrigger("L_ArmThrow");
        //armAnimator.SetBool("L_ArmGrappleStart", false);
       // armAnimator.SetBool("L_ArmGrappleEnd", false);
    }
    public override void ExitState()
    {

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
