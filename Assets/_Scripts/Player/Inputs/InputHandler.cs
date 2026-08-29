using UnityEngine;
using UnityEngine.InputSystem;
using static InputActions_Player;
using static PlayerStateController;
using static UnityEditor.Progress;
public class InputHandler : IPlayerActions
{
    Player player;
    InputActions_Player inputActions;
    PlayerActions playerActions;
    public Vector2 mousePosition;
    public Vector2 moveDirection;
    LayerMask interactMask;
    public InputHandler(Player player)
    {
        inputActions = new InputActions_Player();
        playerActions = inputActions.Player;
        playerActions.SetCallbacks(this);
        playerActions.Enable();
        interactMask = LayerMask.GetMask("Weapon", "Interactable");
        this.player = player;
    }
    public InputActions_Player GetInputActions()
    {
        return inputActions;
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
            player.PlayerStateController.isAttacking = true;
        if (context.canceled)
            player.PlayerStateController.isAttacking = false;
    }
    public void OnChargeAltFire(InputAction.CallbackContext context)
    {
        if (context.performed)
            player.PlayerStateController.isChargingAltFire = true;
        if (context.canceled)
            player.PlayerStateController.isChargingAltFire = false;
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
        if (context.canceled)
            return;
        Vector3 camForward = player.cameraHandler.GetForward();
        
        if (Physics.Raycast(player.transform.position, camForward, out RaycastHit hit, 100f, interactMask))
        {
            GameObject item = hit.collider.gameObject;
            ItemType type = item.GetComponent<Interactable>().interactableType;
            switch (type)
            {
                case ItemType.Weapon:
                        WeaponInteractable weapInteractable = item.GetComponent<WeaponInteractable>();
                        player.playerInventory.AddToSlot(weapInteractable.prefab.GetComponent<BaseWeaponClass>());
                    break;
            }
        }
        
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
                    player.playerWalkingState.EndGrapple(player.playerWalkingState.didHitGrapplePoint);//if already grappling
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
    public void OnUI_Inventory(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            UIHandler.Instance.SetInventoryVisible(true);
            //Cursor.lockState = CursorLockMode.Confined;
        }
            
    }
    public void OnNext(InputAction.CallbackContext context)
    {
        
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        
    }

    public void SetEnabled(bool enabled)
    {
        if(enabled)
            playerActions.Enable();
        else
            playerActions.Disable();
    }

    public void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        InputControl control = context.control;
        if(control == null)
        {
            Debug.LogError("No Control Binding for "+context);
            return;
        }
        player.playerSwitchingState.SetWeaponSlot(int.Parse(control.name));
        player.PlayerStateController.TryChangeState(E_PlayerStates.SWITCH);
        
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            player.playerLeftThrowState.SetSlot(context.control.name);
            player.PlayerStateController.TryChangeState(E_PlayerLeftStates.THROW);
        }
    }

    
}
