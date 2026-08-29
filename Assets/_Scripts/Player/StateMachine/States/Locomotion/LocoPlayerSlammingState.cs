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
        slamSpeed = player.playerStats.slamSpeed;
        maxSlamSpeed = player.playerStats.maxSlamSpeed;
        slamAcceleration = player.playerStats.slamAcceleration;
        slamCollider = player.playerStats.slamColliderObject;
    }
    public override void EnterState()
    {

    }
    public override void ExitState()
    {
        slamSpeed = player.playerStats.slamSpeed;
        deltaAccel = 0;
    }
    public override void FrameUpdate()
    {
        slamSpeed = player.playerStats.slamSpeed;
        slamAcceleration = player.playerStats.slamAcceleration;
        maxSlamSpeed = player.playerStats.maxSlamSpeed;//remove both, just ford ebug
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
