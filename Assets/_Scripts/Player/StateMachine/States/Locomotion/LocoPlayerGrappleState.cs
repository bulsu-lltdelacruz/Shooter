using System.Collections;
using System.Net;
using UnityEngine;

public class LocoPlayerGrappleState : PlayerState
{

    private Transform grappleStart;
    private Vector3 grapplePoint;
    public LineRenderer grappleLine;
    Camera aimCamera;
    LayerMask grappleMask;

    private float grappleSpeed;
    float grappleRange = 10;
    bool didHitGrapplePoint = false;


    public LocoPlayerGrappleState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        grappleSpeed = player.playerMovement.grappleSpeed;
        grappleStart = player.playerMovement.grappleStart.transform;
        grappleLine = player.playerMovement.grappleLineObj.GetComponent<LineRenderer>();
        aimCamera = Camera.main;
        grappleMask = LayerMask.GetMask("Ground","Wall");
        grappleLine.positionCount = 2;
    }
    public override void EnterState()
    {
        
    }
    public override void ExitState()
    {
        didHitGrapplePoint = false;
    }
    public override void FrameUpdate()
    {
        Debug.Log($"no normzlize {grapplePoint - grappleStart.position}, noprmalized {(grapplePoint - grappleStart.position).normalized}");
        grappleLine.SetPosition(1, (grapplePoint-grappleStart.position).normalized* grappleSpeed*Time.deltaTime);

        grappleLine.enabled = true;

    }
    public override void PhysUpdate()
    {

    }
    //IEnumerator
    public override void AnimationTriggerEvent()
    {

    }
}
