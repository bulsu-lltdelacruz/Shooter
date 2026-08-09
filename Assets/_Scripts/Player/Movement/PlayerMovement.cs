using System;
using UnityEditor.UIElements;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Player player;
    InputHandler playerInputs;
    CharacterController characterController;
    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float horizontalSpeedMultiplier = 0.75f;
    [SerializeField] float sprintMultiplier;
    [Header("Jump")]
    [SerializeField] float maxJumpHeight;
    [SerializeField] float initialJumpForce;
    [SerializeField]float minJumpForce;//when reaching max height
    [SerializeField]public AnimationCurve jumpCurve;
    float deltaJumpPosition;
    [Header("Gravty")]
    [SerializeField]float gravityAcceleration;
    [SerializeField] float maxGravityAmount;
    float deltaGravityAmount;
    private Vector3 verticalVelocity;


    void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();
        
    }

    void Update()
    {
        _Move();
        _Gravity();
        _Jump();
    }

    private void _Move()
    {
        if (playerInputs.moveDirection.magnitude < 0.01f)
            return;
        Vector2 _2DMoveDir = playerInputs.moveDirection * moveSpeed * Time.deltaTime;
        _2DMoveDir.x *= horizontalSpeedMultiplier;
        Vector3 forward = player.transform.forward;
        Vector3 right = player.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 finalMoveDirection = (player.transform.right * _2DMoveDir.x) + (player.transform.forward * _2DMoveDir.y);
        characterController.Move(finalMoveDirection);
    }
    
    private void _Jump()
    {
        if(!playerInputs.isJump)
            return;
        deltaGravityAmount = 0;
        verticalVelocity = Vector3.up *initialJumpForce * Time.deltaTime;
        float jumpMomentum_percent = 1 - (deltaJumpPosition / maxJumpHeight);//percent the higher, the lower
        float jumpMomentum = jumpCurve.Evaluate(jumpMomentum_percent);
        if (verticalVelocity.magnitude >= minJumpForce || jumpMomentum > 0)
        {
            verticalVelocity *= jumpMomentum;
        }
        //Debug.Log(jumpMomentum);

        characterController.Move(verticalVelocity);
        deltaJumpPosition += verticalVelocity.magnitude;
        if (maxJumpHeight- deltaJumpPosition <= 0.2f || jumpMomentum < 0)
        {
            playerInputs.isJump = false;
            deltaJumpPosition = 0;
        }
    }
    private void _Gravity()
    {
        if(characterController.isGrounded)
        {
            deltaGravityAmount = 0;
            return;
        }
        if (playerInputs.isJump)
            return;
        if(deltaGravityAmount+ gravityAcceleration * Time.deltaTime < maxGravityAmount)
            deltaGravityAmount += gravityAcceleration * Time.deltaTime;

        verticalVelocity = -Vector3.up * deltaGravityAmount * Time.deltaTime;
        characterController.Move(verticalVelocity);
    }
    internal void SetInstances(InputHandler playerInputs)
    {
        this.playerInputs = playerInputs;
    }
}
