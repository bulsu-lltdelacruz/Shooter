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

    public bool allowMovement;

    #region grappling
    public bool isGrappling;
    private Transform grappleStart;
    private Vector3 grapplePoint;
    private LineRenderer grappleLine;

    private float pullStrength;
    Camera aimCamera;
    LayerMask grappleMask;

    private float grappleSpeed;
    float grappleRange = 100;
    public bool didHitGrapplePoint = false;
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
    public Vector3 currentVelocity;

    //fall damage
        private float airTime;
    private float minAirTimeForFallDamage = .6f;
    private float fallDamagePerSecond = 20f;
    private float fallDamageBase = 12f;
    private float fallDamageGrowth = 500000f;

    

    //int leftArmLayerIndex = 0;
    private float footstepInterval = .4f;
    private float deltaStepInterval;
    public LocoPlayerWalkingState(
        Player player,
        PlayerStateMachine playerStateMachine,
        InputHandler playerInputs,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {

        deltaStepInterval = footstepInterval;
        allowMovement = true;
        this.playerInputs = playerInputs;
        moveSpeed = player.playerStats.moveSpeed;
        horizontalSpeedMultiplier = player.playerStats.horizontalSpeedMultiplier;
        sprintMultiplier = player.playerStats.sprintMultiplier;
        currentVelocity = Vector3.zero;
        //gravity
        gravityAcceleration = player.playerStats.gravityAcceleration;
        maxGravityAmount = player.playerStats.maxGravityAmount;
        //jump
        maxJumpHeight = player.playerStats.maxJumpHeight;
        initialJumpForce = player.playerStats.initialJumpForce;
        minJumpForce = player.playerStats.minJumpForce;//when reaching max height
        jumpCurve = player.playerStats.jumpCurve;

        jumpCounter = PlayerStatus.Instance.maxJumps;
        groundLayer = LayerMask.GetMask("Ground");

        grappleSpeed = player.playerStats.grappleSpeed;
        grappleStart = player.playerStats.grappleStart.transform;
        grappleLine = player.playerStats.grappleLineObj.GetComponent<LineRenderer>();
        pullStrength = player.playerStats.pullStrength;
        momentumDeceleration = player.playerStats.momentumDeceleration;
        movementMultiplier = player.playerStats.movementMultiplier;
        moveInputInfluence = player.playerStats.moveInputInfluence;
        waitBeforeMomentumLoss = new WaitForSeconds(secondsBeforeMomentumLoss);
        //leftArmLayerIndex = armAnimator.GetLayerIndex("Left Arm");
        footstepSound = PlayerSounds.Instance.footsteps_default;
    }
    public override void EnterState()
    {

    }
    public override void ExitState()
    {

    }
    public bool isWalking = false;
    public void ResetWalkSound()
    {
        deltaStepInterval = footstepInterval;
        isWalking = false;
    }
    AudioClip footstepSound;
    public void ChangeFootStepType(FootStepType footStepType)
    {
        switch (footStepType)
        {
            case FootStepType.DEFAULT:
                footstepSound = PlayerSounds.Instance.footsteps_default;
                break;
            case FootStepType.WATER:
                footstepSound = PlayerSounds.Instance.footsteps_water;
                break;
        }
    }
    private void TakeFallDamage(float airTimeSeconds)
    {
        if (airTimeSeconds <= minAirTimeForFallDamage)
            return;

        float excessTime = airTimeSeconds - minAirTimeForFallDamage;
        float damageAmount = fallDamageBase * (Mathf.Pow(fallDamageGrowth, excessTime) - 1f);

        player.TakeDamage(damageAmount, Vector3.zero, Vector3.zero);
      //  player.TakeDamageWithImpact(Vector3.zero, DamageType.FALL, Vector3.zero, Quaternion.identity);
}
    private void _TrackAirTime()
    {
       /* if (!PlayerStatus.Instance.isGrounded)
        {
            // only count time spent falling, not rising
            if (velocity.y < 0f)
            {
                airTime += Time.deltaTime;
            }
        }
        else
        {
            if (airTime > 0f)
            {
                TakeFallDamage(airTime);
            }
            airTime = 0f;
        }*/
    }
    public override void FrameUpdate()
    {

        if (!allowMovement)
        {
            return;
        }
        _TrackAirTime();
        if (isWalking)
        {
            deltaStepInterval += Time.deltaTime;
            if (deltaStepInterval >= footstepInterval && PlayerStatus.Instance.isGrounded)
            {
                deltaStepInterval = 0;
                SoundFXManager.instance.PlaySoundFXClip(footstepSound, player.transform, .6f, SoundType.SOUNDFX);
            }
        }

        jumpDir = Vector3.up;
        if (PlayerStatus.Instance.isGrounded && !PlayerStatus.Instance.isJumping)
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
            //jumpCounter = PlayerStatus.Instance.maxJumps;
            //jumpDir = (jumpDir + PlayerStatus.Instance.wallNormal).normalized;
        }
        _DebuggingPurpose();
        if (_Climb()) 
            return;
        // Debug.Log(deltaGravityAmount);
        _Move();
        _Gravity();
        _Jump();
        _DrawGrappleLine();
        _ApplyMovement();
        Vector3 toPoint = grapplePoint - grappleStart.position;
        Vector3 directionNormalized = toPoint.normalized;
        grappleLine.SetPosition(0, grappleStart.position);
        grappleLine.SetPosition(1, grappleStart.position + directionNormalized * deltaGrapplePos);
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
        moveSpeed = player.playerStats.moveSpeed;
        horizontalSpeedMultiplier = player.playerStats.horizontalSpeedMultiplier;
        sprintMultiplier = player.playerStats.sprintMultiplier;
        //gravity
        gravityAcceleration = player.playerStats.gravityAcceleration;
        maxGravityAmount = player.playerStats.maxGravityAmount;
        //jump
        maxJumpHeight = player.playerStats.maxJumpHeight;
        initialJumpForce = player.playerStats.initialJumpForce;
        minJumpForce = player.playerStats.minJumpForce;//when reaching max height

        grappleSpeed = player.playerStats.grappleSpeed;
        grappleStart = player.playerStats.grappleStart.transform;
        grappleLine = player.playerStats.grappleLineObj.GetComponent<LineRenderer>();
        pullStrength = player.playerStats.pullStrength;
        momentumDeceleration = player.playerStats.momentumDeceleration;
        movementMultiplier = player.playerStats.movementMultiplier;
        moveInputInfluence = player.playerStats.moveInputInfluence;

        aimCamera = Camera.main;
        grappleMask = LayerMask.GetMask("Ground", "Wall");
        grappleLine.positionCount = 2;
    }

    private float initialGrappleDistance;
    private bool isInitialGrappleThrow;
    public void StartGrapple()
    {
        armAnimator.SetLayerWeight(leftArmLayerIndex, 1f);
        armAnimator.SetBool("L_ArmGrappleStart", true);
        isGrappling = true;
        if (!didHitGrapplePoint)
            isInitialGrappleThrow = true;
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
        initialGrappleDistance = Vector3.Distance(grappleStart.position, grapplePoint);

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
        deltaGrapplePos = Mathf.Clamp(deltaGrapplePos, 0, maxDistance);

        if (!grappleLine.enabled)
            grappleLine.enabled = true;

        if (didHitGrapplePoint && deltaGrapplePos >= maxDistance)
        {
            deltaGravityAmount = 0;
            Vector3 finalVel = (velocity.normalized + directionNormalized) * (pullStrength * Time.deltaTime);
            //Debug.Log($"_DrawGrappleLine(): Velocity before : {velocity.normalized}, Normalized:{directionNormalized}, Velocity after : {finalVel}");
            velocity = finalVel;
            // Debug.Log(velocity.magnitude);
            armAnimator.SetBool("L_ArmGrappleEnd", true);
            armAnimator.SetBool("L_ArmGrappleStart", false);
            if ((player.transform.position - grapplePoint).sqrMagnitude < 4)
            {
                EndGrapple(true);
            }
        }
        else if (!didHitGrapplePoint && deltaGrapplePos >= maxDistance)
        {
            armAnimator.SetBool("L_ArmGrappleStart", false);
            EndGrapple(false);
        }
    }


    public void EndGrapple(bool isPreserveMomentum)
    {
        isGrappling = false;
        isInitialGrappleThrow = false;
        deltaGrapplePos = 0f;
        grappleLine.enabled = false; // hide the rope once released — drop this if handled elsewhere
        if (isPreserveMomentum)
            preserveMomentum = true;//tama yan
        else
            preserveMomentum = false;
        initialMomentum = velocity.magnitude;
        if (isPreserveMomentum)
            deltaGravityAmount = 0;
        armAnimator.SetLayerWeight(leftArmLayerIndex, 0f);
        armAnimator.SetBool("L_ArmGrappleEnd", false);
    }
    private void _Move()
    {
        if (preserveMomentum)
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

            Vector3 finalVelocity = normalizedMoveDir * moveInputInfluence + normalizedVelocity;
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
        Vector3 dashDir = new Vector3(velocity.x, 0, velocity.z);
        player.playerDashingState.SetDirection(dashDir.normalized);
    }

    private void _Gravity()
    {
        Quaternion slopeOffset = Quaternion.identity;

        if (PlayerStatus.Instance.isJumping)
            return;
        if (preserveMomentum)
        {
            return;
        }
        if (deltaGravityAmount + gravityAcceleration * Time.deltaTime < maxGravityAmount)
            deltaGravityAmount += gravityAcceleration * Time.deltaTime;
        if (isGrappling && !isInitialGrappleThrow)
            deltaGravityAmount /= 5;// if grappling, reduce influence of gravity


        //Vector3 finalVelocityOffset = slopeOffset *- Vector3.up;
        velocity.y = -deltaGravityAmount * Time.deltaTime;

    }
    float initialMomentum;

    private float GetDeltaGravityFromMomentum(float initialMomentum)
    {
        float momentumFactor = 1.0f / (initialMomentum * 90f);
        Debug.Log(momentumFactor);
        //float momentumFactor = .05f;
        return (deltaGravityAmount + gravityAcceleration * Time.deltaTime) * momentumFactor;
    }
    private void _ApplyMovement()
    {
        if (!PlayerStatus.Instance.isGrounded)
        {
            if (preserveMomentum)
            {
                //becomes negative
                /*if(velocity.magnitude  > 0) change every ve l version
                   velocity = velocity.normalized * (velocity.magnitude - momentumDeceleration*Time.deltaTime);*/
                if (velocity.magnitude > 0) //change only y
                    velocity.y = velocity.y - momentumDeceleration * Time.deltaTime;

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
        currentVelocity = velocity;
    }
    private void _Jump()
    {
        if (!PlayerStatus.Instance.isJumping)
            return;
        if (isGrappling)
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
        if (PlayerStatus.Instance.isWithinWall || PlayerStatus.Instance.isWallJumping)
        {
            //WallJump();
            //return;
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
        float jumpMomentum_percent = 1 - (deltaJumpPosition / (maxJumpHeight * 2));
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
    #region LADDER
    public bool isClimbing;
    private Ladder currentLadder;
    private float climbSpeed = 3f;

    public void SetLadder(Ladder ladder) => currentLadder = ladder;

    public void ClearLadder(Ladder ladder)
    {
        if (currentLadder != ladder) return;
        currentLadder = null;
        if (isClimbing)
            StopClimbing(playerInputs.moveDirection.y > 0);
    }

    private void StartClimbing()
    {
        if (isGrappling) EndGrapple(false);
        isClimbing = true;
        preserveMomentum = false;
        velocity = Vector3.zero;
        deltaGravityAmount = 0;
    }

    private void StopClimbing(bool hopForward = false)
    {
        isClimbing = false;
        velocity = Vector3.zero;
        deltaGravityAmount = 0;
        if (hopForward)
            player.characterController.Move(player.transform.forward * 0.6f + Vector3.up * 0.1f);
    }

    private bool _Climb()
    {
        if (currentLadder == null) return false;
        float input = playerInputs.moveDirection.y;

        if (!isClimbing)
        {
            if (input > 0.1f && !PlayerStatus.Instance.isJumping)
                StartClimbing();
            else
                return false;
        }

        if (PlayerStatus.Instance.isJumping || (input < -0.1f && PlayerStatus.Instance.isGrounded))
        {
            StopClimbing();
            return false;
        }

        player.characterController.Move(Vector3.up * input * climbSpeed * Time.deltaTime);
        return true;
    }
#endregion
}