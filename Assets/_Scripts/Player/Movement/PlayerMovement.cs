using System;
using UnityEditor.UIElements;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("These are the base values")]
    Player player;
    InputHandler playerInputs;
    CharacterController characterController;
    [Header("Movement")]
    [SerializeField] public float moveSpeed;
    [SerializeField] public float horizontalSpeedMultiplier = 0.75f;
    [SerializeField] public float sprintMultiplier;
    private Vector3 horizontalVelocity;
    [Header("Jump")]
    [SerializeField] public float maxJumpHeight;
    [SerializeField] public float initialJumpForce;
    [SerializeField] public float minJumpForce;//when reaching max height
    [SerializeField] public AnimationCurve jumpCurve;
    float deltaJumpPosition;
    [Header("Gravty")]
    [SerializeField] public float gravityAcceleration;
    [SerializeField] public float maxGravityAmount;
    float deltaGravityAmount;
    private Vector3 verticalVelocity;

    public int jumpCounter;
    int maxJumps = 2;
    [Header("Advanced Movements")]
    [Header("Dash")]
    [SerializeField] public float dashSpeed;
    [SerializeField] public float maxDashDistance;
    private float deltaDashDistance;
    [Header("Slam")]
    [SerializeField] public float slamSpeed;
    [SerializeField] public float maxSlamSpeed;
    [SerializeField] public float slamAcceleration;
    [SerializeField] public GameObject slamColliderObject;
    [Header("Grapple")]
    [SerializeField] public float grappleSpeed;
    [SerializeField] public GameObject grappleStart;
    [SerializeField] public GameObject grappleLineObj;
    [SerializeField] public float pullStrength;
    [SerializeField] public float movementMultiplier;
    [SerializeField] public float moveInputInfluence;
    [Header("Momentum")]
    [SerializeField] public float momentumDeceleration;

    void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();
        jumpCounter = maxJumps;
    }

    void Update()
    {
       // _Move();
       // _Gravity();
       // _Jump();
    }

    private void _Move()
    {
        
        if (playerInputs.moveDirection.magnitude < 0.01f)
        {
            _Dash(transform.forward);
            return;
        }
        Vector2 _2DMoveDir = playerInputs.moveDirection * moveSpeed * Time.deltaTime;
        _2DMoveDir.x *= horizontalSpeedMultiplier;
        Vector3 forward = player.transform.forward;
        Vector3 right = player.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        horizontalVelocity = (player.transform.right * _2DMoveDir.x) + (player.transform.forward * _2DMoveDir.y);
        characterController.Move(horizontalVelocity);
        _Dash(horizontalVelocity.normalized);
    }
    public void ResetJump()
    {
        if (jumpCounter == 1)//second jump
        {
            deltaJumpPosition = 0;
        }
    }
    private void _Jump()//needs to be on update because everyupdate it fixes the velocity
    {
        if(!PlayerStatus.Instance.isJumping)
            return;
        
        deltaGravityAmount = 0;
        verticalVelocity = Vector3.up *initialJumpForce * Time.deltaTime;
        float jumpMomentum_percent = 1 - (deltaJumpPosition / maxJumpHeight);//percent the higher, the lower
        float jumpMomentum = jumpCurve.Evaluate(jumpMomentum_percent);
        if (verticalVelocity.magnitude >= minJumpForce || jumpMomentum > 0)//minjumpforce, since the higher you go the lower it is. so keep decreasing velocity until the max
        {
            verticalVelocity *= jumpMomentum;
        }
        //Debug.Log(jumpMomentum);

        characterController.Move(verticalVelocity);
        deltaJumpPosition += verticalVelocity.magnitude;
        if (maxJumpHeight- deltaJumpPosition <= 0.2f || jumpMomentum < 0)//pag pababa na
        {
            PlayerStatus.Instance.isJumping = false;
            deltaJumpPosition = 0;
        }
        
    }
    private void _Dash(Vector3 forward)
    {
       // if (PlayerStatus.Instance.isDashing)
        {
            float frameDashDistance = dashSpeed * Time.deltaTime;
            Vector3 dashStep = forward * frameDashDistance;
            characterController.Move(dashStep);

            deltaDashDistance += frameDashDistance;

            if (deltaDashDistance >= maxDashDistance)
            {
                //PlayerStatus.Instance.isDashing = false;
                deltaDashDistance = 0f;
            }
        }
    }
    private void _Slam()
    {
       /* if (PlayerStatus.Instance.isSlamming)
        {
            float frameDashDistance = dashSpeed * Time.deltaTime;
            Vector3 dashStep = forward * frameDashDistance;
            characterController.Move(dashStep);

            deltaDashDistance += frameDashDistance;

            if (deltaDashDistance >= maxDashDistance)
            {
                PlayerStatus.Instance.isDashing = false;
                deltaDashDistance = 0f;
            }
        }*/
    }
    private void _Gravity()
    {
        if (PlayerStatus.Instance.isGrounded)
        {
            deltaGravityAmount = 0;
            jumpCounter = maxJumps;
        }
        if (PlayerStatus.Instance.isJumping)
            return;

        if (deltaGravityAmount+ gravityAcceleration * Time.deltaTime < maxGravityAmount)
            deltaGravityAmount += gravityAcceleration * Time.deltaTime;

        verticalVelocity = -Vector3.up * deltaGravityAmount * Time.deltaTime;
        characterController.Move(verticalVelocity);
        
    }
    internal void SetInstances(InputHandler playerInputs)
    {
        this.playerInputs = playerInputs;
    }
}
