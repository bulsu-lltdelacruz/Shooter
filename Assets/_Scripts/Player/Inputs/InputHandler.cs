using UnityEngine;
using UnityEngine.InputSystem;
using static InputActions_Player;
using static PlayerStateController;
public class InputHandler : IPlayerActions
{
    Player player;
    InputActions_Player inputActions;
    PlayerActions playerActions;
    public Vector2 mousePosition;
    public Vector2 moveDirection;
    public InputHandler(Player player)
    {
        inputActions = new InputActions_Player();
        playerActions = inputActions.Player;
        playerActions.SetCallbacks(this);
        playerActions.Enable();
        this.player = player;
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
            player.PlayerStateController.isAttacking = true;
        if (context.canceled)
            player.PlayerStateController.isAttacking = false;
    }
    public void OnReload(InputAction.CallbackContext context)//this is usually for detatching shit
    {
        if (context.performed)
            player.PlayerStateController.TryChangeState(E_PlayerStates.RELOAD);
    }
    public void OnReload2(InputAction.CallbackContext context)//this is usually for reattaching shit
    {
        if (context.performed)
            player.PlayerStateController.TryChangeState(E_PlayerStates.RELOAD2);
    }

    public void OnMelee(InputAction.CallbackContext context)
    {
        if (context.started)
            player.PlayerStateController.isMeleeing = true;
        if (context.canceled)
            player.PlayerStateController.isMeleeing = false;
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.canceled && player.AllowJump())
        {
            PlayerStatus.Instance.isJumping = true;//can only be made false by PlayerMovement
            player.playerWalkingState.ResetJump();
            player.playerWalkingState.jumpCounter--;
        }

    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.canceled)
        {
            //cooldownhandler
            //PlayerStatus.Instance.isDashing = true;
            player.PlayerStateController.TryChangeState(E_PlayerMovementStates.DASH);
        }
    }
    public void OnSlam(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            player.PlayerStateController.TryChangeState(E_PlayerMovementStates.SLAM);
        }
    }
    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(player.PlayerStateController.GetCurrentLocoState() is LocoPlayerWalkingState)
            {
                if(!player.playerWalkingState.isGrappling)
                    player.playerWalkingState.StartGrapple();
                else
                    player.playerWalkingState.EndGrapple();
            }
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {//camera movement
        mousePosition = context.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        
    }
}
