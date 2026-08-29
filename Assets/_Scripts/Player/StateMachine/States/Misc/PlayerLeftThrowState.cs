using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLeftThrowState : PlayerState
{
    private string slot;
    public PlayerLeftThrowState(
        Player player,
        PlayerStateMachine playerStateMachine,
        InputHandler playerInputs,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        baseCooldown = 0.292f + .05f + .03f;
        //baseCooldown = 0.5f;

    }
    public override void EnterState()
    {
        armAnimator.SetLayerWeight(leftArmLayerIndex, 1f);
        armAnimator.SetTrigger("L_ArmThrow");
        player.grenadeController.ThrowGrenade(slot);
    }
    public override void ExitState()
    {
        //armAnimator.SetLayerWeight(leftArmLayerIndex, 0f);
        slot = null;
        armAnimator.ResetTrigger("L_ArmThrow");
    }
    public void SetSlot(string slot)
    {
        this.slot = slot;
    }
    public override void FrameUpdate()
    {
        deltaTime += Time.deltaTime;
        if (deltaTime > baseCooldown)
        {
            player.PlayerStateController.TryChangeState(E_PlayerLeftStates.IDLE);
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
