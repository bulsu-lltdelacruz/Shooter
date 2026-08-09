using UnityEngine;
using UnityEngine.InputSystem;
using static InputActions_Player;
public class InputHandler : IPlayerActions
{
    Player player;
    InputActions_Player inputActions;
    PlayerActions playerActions;
    public Vector2 mousePosition;
    public Vector2 moveDirection;
    public bool isJump = false;//can only be made false by PlayerMovement
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
    public void OnCrouch(InputAction.CallbackContext context)
    {
        
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.canceled && player.AllowJump())
        {
            isJump = true;//can only be made false by PlayerMovement
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

    public void OnSprint(InputAction.CallbackContext context)
    {
        
    }
}
