using UnityEngine;

public class LocoPlayerSlammingState : PlayerState
{
    float slamSpeed;
    float slamAcceleration;
    float maxSlamSpeed;
    GameObject slamCollider;
    float deltaAccel;
    public LocoPlayerSlammingState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        slamSpeed = player.playerMovement.slamSpeed;
        maxSlamSpeed = player.playerMovement.maxSlamSpeed;
        slamAcceleration = player.playerMovement.slamAcceleration;
        slamCollider = player.playerMovement.slamColliderObject;
    }
    public override void EnterState()
    {

    }
    public override void ExitState()
    {
        slamSpeed = player.playerMovement.slamSpeed;
        deltaAccel = 0;
    }
    public override void FrameUpdate()
    {
        slamSpeed = player.playerMovement.slamSpeed;
        slamAcceleration = player.playerMovement.slamAcceleration;
        maxSlamSpeed = player.playerMovement.maxSlamSpeed;//remove both, just ford ebug
        //add collider bigger, and other logic heere when i apply the cool shit
        deltaAccel += (deltaAccel+slamSpeed < maxSlamSpeed) ? deltaAccel * slamAcceleration * Time.deltaTime + 1f : 0;
        player.characterController.Move(-Vector3.up*(slamSpeed+ deltaAccel)* Time.deltaTime);
        //Debug.Log(slamSpeed + deltaAccel);//50 is usual with double jump, 35 with single and yeah
        if(PlayerStatus.Instance.isGrounded)
            player.PlayerStateController.ForceWalkState();
    }
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
}
