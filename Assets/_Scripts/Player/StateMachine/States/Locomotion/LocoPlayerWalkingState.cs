using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocoPlayerWalkingState : PlayerState
{
    InputHandler playerInputs;
    float moveSpeed;
    float horizontalSpeedMultiplier = 0.75f;
    float sprintMultiplier; 
    //gravity
    float gravityAcceleration;
    float maxGravityAmount;
    float deltaGravityAmount;
    //jump
    float maxJumpHeight;
    float initialJumpForce;
    float minJumpForce;//when reaching max height
    AnimationCurve jumpCurve;
    float deltaJumpPosition;
    public int jumpCounter;
    private Vector3 jumpDir;


    LayerMask groundLayer;



    #region grappling
    public bool isGrappling;
    private Transform grappleStart;
    private Vector3 grapplePoint;
    private LineRenderer grappleLine;

    private float pullStrength;
    Camera aimCamera;
    LayerMask grappleMask;

    private float grappleSpeed;
    float grappleRange = 5000;
    bool didHitGrapplePoint = false;
    float momentumDeceleration;
    float movementMultiplier;
    float moveInputInfluence;
    private float deltaGrapplePos;
    #endregion

    //momoentum
    private bool preserveMomentum;
    float secondsBeforeMomentumLoss = 0.1f;
    WaitForSeconds waitBeforeMomentumLoss;
    private Coroutine momentumCoroutine;

    private bool hasTouchedGroundAfterTouchingWall;
    private Vector3 velocity;
    public LocoPlayerWalkingState(
        Player player,
        PlayerStateMachine playerStateMachine,
        InputHandler playerInputs,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        this.playerInputs = playerInputs;
        moveSpeed = player.playerMovement.moveSpeed;
        horizontalSpeedMultiplier = player.playerMovement.horizontalSpeedMultiplier;
        sprintMultiplier = player.playerMovement.sprintMultiplier; 
        //gravity
        gravityAcceleration = player.playerMovement.gravityAcceleration;
        maxGravityAmount = player.playerMovement.maxGravityAmount;
        //jump
        maxJumpHeight = player.playerMovement.maxJumpHeight;
        initialJumpForce = player.playerMovement.initialJumpForce;
        minJumpForce = player.playerMovement.minJumpForce;//when reaching max height
        jumpCurve = player.playerMovement.jumpCurve;

        jumpCounter = PlayerStatus.Instance.maxJumps;
        groundLayer = LayerMask.GetMask("Ground");

        grappleSpeed = player.playerMovement.grappleSpeed;
        grappleStart = player.playerMovement.grappleStart.transform;
        grappleLine = player.playerMovement.grappleLineObj.GetComponent<LineRenderer>();
        pullStrength = player.playerMovement.pullStrength;
        momentumDeceleration = player.playerMovement.momentumDeceleration;
        movementMultiplier = player.playerMovement.movementMultiplier;
        moveInputInfluence = player.playerMovement.moveInputInfluence;
        waitBeforeMomentumLoss = new WaitForSeconds(secondsBeforeMomentumLoss);

    }
    public override void EnterState()
    {

    }
    public override void ExitState()
    {

    }
    public override void FrameUpdate()
    {
        jumpDir = Vector3.up;
        if (PlayerStatus.Instance.isGrounded)
        {
            if (momentumCoroutine == null)
            {
                momentumCoroutine = player.StartCoroutine(StartFrameCounterBeforeMomentumLoss());
            }
            deltaGravityAmount = 0;
            initialMomentum = 0;
            jumpCounter = PlayerStatus.Instance.maxJumps;
            hasTouchedGroundAfterTouchingWall = true;
        }
        
        if (PlayerStatus.Instance.isWithinWall)
        {
            jumpCounter = PlayerStatus.Instance.maxJumps;
            jumpDir = (jumpDir + PlayerStatus.Instance.wallNormal).normalized;
        }
        _DebuggingPurpose();
       // Debug.Log(deltaGravityAmount);
        _Move();
        _Gravity();
        _Jump();
        _DrawGrappleLine();
        _ApplyMovement();
        // Debug.Log(velocity.magnitude);
    }
    
    IEnumerator StartFrameCounterBeforeMomentumLoss()
    {
        yield return waitBeforeMomentumLoss;
        preserveMomentum = false;
        momentumCoroutine = null;
        //ADD THIS TO WALLJUMP TOO
        //walljump, make it a fix angle abve the normal of the wall, only maniupalte the forward and backeward
    }
    private void _DebuggingPurpose()
    {
        moveSpeed = player.playerMovement.moveSpeed;
        horizontalSpeedMultiplier = player.playerMovement.horizontalSpeedMultiplier;
        sprintMultiplier = player.playerMovement.sprintMultiplier;
        //gravity
        gravityAcceleration = player.playerMovement.gravityAcceleration;
        maxGravityAmount = player.playerMovement.maxGravityAmount;
        //jump
        maxJumpHeight = player.playerMovement.maxJumpHeight;
        initialJumpForce = player.playerMovement.initialJumpForce;
        minJumpForce = player.playerMovement.minJumpForce;//when reaching max height

        grappleSpeed = player.playerMovement.grappleSpeed;
        grappleStart = player.playerMovement.grappleStart.transform;
        grappleLine = player.playerMovement.grappleLineObj.GetComponent<LineRenderer>();
        pullStrength = player.playerMovement.pullStrength;
        momentumDeceleration = player.playerMovement.momentumDeceleration;
        movementMultiplier = player.playerMovement.movementMultiplier;
        moveInputInfluence = player.playerMovement.moveInputInfluence;

        aimCamera = Camera.main;
        grappleMask = LayerMask.GetMask("Ground", "Wall");
        grappleLine.positionCount = 2;
    }
    public void StartGrapple()
    {
        isGrappling = true;
        Vector3 endPoint;
        if (Physics.Raycast(aimCamera.transform.position, aimCamera.transform.forward, out RaycastHit hit, grappleRange, grappleMask))
        {
            endPoint = hit.point;
            didHitGrapplePoint = true;
            preserveMomentum = false;
            ResetJump();//preserve  the memontum when jumping
        }
        else
        {
            endPoint = aimCamera.transform.position + aimCamera.transform.forward * grappleRange;
            didHitGrapplePoint = false;
        }

        grapplePoint = endPoint;

        deltaGrapplePos = 0f; // reset so each new grapple starts from zero length

        grappleLine.SetPosition(0, grappleStart.position);
        grappleLine.SetPosition(1, grappleStart.position);
    }

    private void _DrawGrappleLine()
    {
        if (!isGrappling)
            return;

        Vector3 toPoint = grapplePoint - grappleStart.position;
        float maxDistance = toPoint.magnitude;
        Vector3 directionNormalized = toPoint.normalized;

        deltaGrapplePos += grappleSpeed * Time.deltaTime;
        deltaGrapplePos = Mathf.Min(deltaGrapplePos, maxDistance);

        grappleLine.SetPosition(0, grappleStart.position);
        grappleLine.SetPosition(1, grappleStart.position + directionNormalized * deltaGrapplePos);

        if (!grappleLine.enabled)
            grappleLine.enabled = true;

        if (didHitGrapplePoint && deltaGrapplePos >= maxDistance)
        {
            deltaGravityAmount = 0;
            Vector3 finalVel = (velocity.normalized + directionNormalized) * (pullStrength * Time.deltaTime);
            //Debug.Log($"_DrawGrappleLine(): Velocity before : {velocity.normalized}, Normalized:{directionNormalized}, Velocity after : {finalVel}");
            velocity = finalVel;
           // Debug.Log(velocity.magnitude);
            if ((player.transform.position - grapplePoint).sqrMagnitude < 4)
            {
                EndGrapple();
            }
        }
        else if (!didHitGrapplePoint && deltaGrapplePos >= maxDistance)
        {
            EndGrapple();
        }
    }


    public void EndGrapple()
    {
        isGrappling = false;
        deltaGrapplePos = 0f;
        grappleLine.enabled = false; // hide the rope once released — drop this if handled elsewhere
        preserveMomentum = true;//tama yan
        initialMomentum = velocity.magnitude;
        deltaGravityAmount = 0;
    }
    private void _Move()
    {
        if(preserveMomentum)
            return;
        if (playerInputs.moveDirection.magnitude < 0.01f)
        {
            //_Dash(transform.forward);
            if (!isGrappling && !preserveMomentum)
            {
                velocity.x = 0;
                velocity.z = 0;
            }
            player.playerDashingState.SetDirection(player.transform.forward);
            return;
        }
        Vector2 _2DMoveDir = playerInputs.moveDirection * moveSpeed * Time.deltaTime;
        Vector3 forward = player.transform.forward;
        Vector3 right = player.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        float velY = velocity.y;
        Vector3 movementDirectionWithSPeed = (player.transform.right * _2DMoveDir.x) + (player.transform.forward * _2DMoveDir.y);

        if (isGrappling || preserveMomentum)
        {
            Vector3 normalizedMoveDir = movementDirectionWithSPeed.normalized;
            Vector3 normalizedVelocity = velocity.normalized;

            Vector3 finalVelocity = normalizedMoveDir*moveInputInfluence + normalizedVelocity;
            //Debug.Log($"_Move() : Velocity : {normalizedVelocity}, MoveDir : {normalizedMoveDir}, Final: {finalVelocity}, normal : {finalVelocity.normalized}");
            finalVelocity.Normalize();
            velocity = new Vector3(finalVelocity.x, velocity.y, finalVelocity.z) * (velocity.magnitude);
            //Debug.Log("movevel: "+ velocity);
        }
        else
        {
            velocity = movementDirectionWithSPeed;
            velocity.y = velY;
        }
        //_Dash(horizontalVelocity.normalized);
        player.playerDashingState.SetDirection(velocity.normalized);
    }
    
    private void _Gravity()
    {
        Quaternion slopeOffset = Quaternion.identity;
       
        
        
        if (PlayerStatus.Instance.isJumping)
            return;
        if (preserveMomentum)
        {
            return;
            if (velocity.y <= 0)//if almost falling, start increaseing gravity
            {

            }
            else //else dont add gravity yet
            {
                return;
            }
        }
        if (deltaGravityAmount + gravityAcceleration * Time.deltaTime < maxGravityAmount)
            deltaGravityAmount += gravityAcceleration * Time.deltaTime;
        if (isGrappling)
            deltaGravityAmount /= 5;// if grappling, reduce influence of gravity
        
            
        //Vector3 finalVelocityOffset = slopeOffset *- Vector3.up;
        velocity.y = - deltaGravityAmount * Time.deltaTime;

    }
    float initialMomentum;

    private float GetDeltaGravityFromMomentum(float initialMomentum)
    {
        float momentumFactor = 1.0f / (initialMomentum * 90f);
        Debug.Log(momentumFactor);
        //float momentumFactor = .05f;
        return (deltaGravityAmount + gravityAcceleration * Time.deltaTime)*momentumFactor;
    }
    private void _ApplyMovement()
    {
        if (!PlayerStatus.Instance.isGrounded)
        {
            if(preserveMomentum)
            {
                //becomes negative
                /*if(velocity.magnitude  > 0) change every ve l version
                   velocity = velocity.normalized * (velocity.magnitude - momentumDeceleration*Time.deltaTime);*/
                if(velocity.magnitude  > 0) //change only y
                   velocity.y = velocity.y  - momentumDeceleration*Time.deltaTime;

                if (velocity.magnitude <= initialMomentum * 0.75f)
                {
                    float finalGrav = GetDeltaGravityFromMomentum(initialMomentum);
                    if (finalGrav < maxGravityAmount)
                        deltaGravityAmount = finalGrav;
                    //deltaGravityAmount = (deltaGravityAmount +gravityAcceleration * Time.deltaTime)*(.5f);//how quick grav is depends on the intiial memonth
                    velocity.y -= deltaGravityAmount;
                }
            }
            player.characterController.Move(velocity);
            return;
        }

        float speed = velocity.magnitude;
        Vector3 moveDir = velocity.normalized;
        Quaternion slopeOffset = Quaternion.identity;

        float castAheadDist = player.characterController.radius + 0.05f;
        Vector3 origin = player.transform.position
            + player.transform.TransformDirection(new Vector3(playerInputs.moveDirection.x, 0, playerInputs.moveDirection.y)).normalized * castAheadDist
            + Vector3.up * 0.5f;
        Debug.DrawRay(origin, Vector3.down, Color.red, 1);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1000, groundLayer))
        {
            slopeOffset = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }

        player.characterController.Move(slopeOffset * moveDir * speed);
    }
    private void _Jump()
    {
        if (!PlayerStatus.Instance.isJumping)
            return;
        if(isGrappling)
        {
            PlayerStatus.Instance.isJumping = false;
            deltaJumpPosition = 0;
            return;
        }
        if (momentumCoroutine != null)
        {
            player.StopCoroutine(momentumCoroutine);
            momentumCoroutine = null;
        }
        //preserveMomentum = true;

        deltaGravityAmount = 0;
        if(PlayerStatus.Instance.isWithinWall || PlayerStatus.Instance.isWallJumping)
        {
            WallJump();
            return;
        }
        float deltaY = initialJumpForce * Time.deltaTime;
        velocity.y = deltaY;
        float jumpMomentum_percent = 1 - (deltaJumpPosition / maxJumpHeight);
        float jumpMomentum = jumpCurve.Evaluate(jumpMomentum_percent);
        if (velocity.magnitude >= minJumpForce || jumpMomentum > 0)
        {
            velocity.y *= jumpMomentum;
        }

        deltaJumpPosition += deltaY;
        if (maxJumpHeight - deltaJumpPosition <= 0.2f || jumpMomentum < 0)
        {
            PlayerStatus.Instance.isJumping = false;
            deltaJumpPosition = 0;
        }
    }
    private void WallJump()//ADD LERTP TO INFLUENCE MOVEMENT INPUT
    {
        float moveInfluence = 1f;
        Vector3 _moveDir = new Vector3(playerInputs.moveDirection.x, 0, playerInputs.moveDirection.y);
        jumpDir = jumpDir + (_moveDir * moveInfluence);
        jumpDir.Normalize();
        preserveMomentum = false;
        initialMomentum = 0;
        PlayerStatus.Instance.isWallJumping = true;
        float deltaY = initialJumpForce * 2 * Time.deltaTime;
        velocity = jumpDir * deltaY;
        float jumpMomentum_percent = 1 - (deltaJumpPosition / (maxJumpHeight*2));
        float jumpMomentum = jumpCurve.Evaluate(jumpMomentum_percent);
        if (velocity.magnitude >= minJumpForce || jumpMomentum > 0)
        {
            velocity *= jumpMomentum;
        }

        deltaJumpPosition += deltaY;
        if (maxJumpHeight - deltaJumpPosition <= 0.2f || jumpMomentum < 0)
        {
            PlayerStatus.Instance.isJumping = false;
            PlayerStatus.Instance.isWallJumping = false;
            deltaJumpPosition = 0;
        }
    }
    public void ResetJump()
    {
        if (jumpCounter < PlayerStatus.Instance.maxJumps)//subsequent jump
        {
            deltaJumpPosition = 0;
        }
    }
    private void _SnapshotMomentum()
    {

    }
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
}
