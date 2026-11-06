using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static InputActions_Player;
using static PlayerStateController;
//using static UnityEditor.Progress;
public class InputHandler : IPlayerActions
{
    Player player;
    InputActions_Player inputActions;
    PlayerActions playerActions;
    public Vector2 mousePosition;
    public Vector2 moveDirection;
    LayerMask interactMask;
    MenuHandler menuHandler;
    public InputHandler(Player player, MenuHandler menuHandler)
    {
        inputActions = new InputActions_Player();
        playerActions = inputActions.Player;
        playerActions.SetCallbacks(this);
        playerActions.Enable();
        interactMask = LayerMask.GetMask("Weapon", "Interactable", "Door");
        this.menuHandler = menuHandler;
        //parent = pauseMenu.rootVisualElement.Q<VisualElement>("menu-parent");
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
       /* if (context.performed)
            player.PlayerStateController.TryChangeState(E_PlayerStates.RELOAD);*/
    }
    public void OnReload2(InputAction.CallbackContext context)//this is usually for reattaching shit
    {
        /*if (context.performed)
            player.PlayerStateController.TryChangeState(E_PlayerStates.RELOAD2);*/
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
            ItemType type = ItemType.None;
            if (item.TryGetComponent<Interactable>(out Interactable interactable))
            {
                type = interactable.interactableType;
            } else
            {
                if (hit.collider.CompareTag("Door")&& hit.distance <= 1.1f)
                {
                    HintDamageManager.Instance.ShowHint("Door does not open from this side");
                }
                if (hit.collider.CompareTag("FinalDoor") && hit.distance <= 1.1f)
                {
                    HintDamageManager.Instance.ShowHint("Door unlocks with a mechanism");
                }
            }
            switch (type)
            {
                case ItemType.Weapon:
                    WeaponInteractable weapInteractable = item.GetComponent<WeaponInteractable>();
                    player.playerInventory.AddToSlot(weapInteractable.prefab.GetComponent<BaseWeaponClass>());
                    break;
                case ItemType.Door:
                    if (hit.distance <= 1.5f)
                        item.GetComponent<Door>().OpenDoor();
                    break;
                case ItemType.Terminal:
                    if (hit.distance <= 1.5f)
                        item.GetComponent<Terminal>().Interact();
                    break;
                case ItemType.Guide:
                    if (hit.distance <= 2f)
                        item.GetComponent<Guide>().ShowText();
                    break;
                case ItemType.None:
                    break;
            }
            player.weaponController.DecreaseBulletCount(0);//just to update
        }
        
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.canceled && player.AllowJump())
        {
            PlayerStatus.Instance.isJumping = true;//can only be made false by PlayerMovement
            //player.playerWalkingState.ResetJump();
            player.playerWalkingState.jumpCounter--;
        }

    }
    public float dashCooldown = 1f;
    float lastDashTime = -Mathf.Infinity;
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.canceled) return;
        if (Time.time < lastDashTime + dashCooldown) return;

        lastDashTime = Time.time;
        player.PlayerStateController.TryChangeState(E_PlayerMovementStates.DASH);
    }
    public void OnSlam(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
           // player.PlayerStateController.TryChangeState(E_PlayerMovementStates.SLAM);
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
        if (context.performed)
            player.playerWalkingState.isWalking = true;
        else if (context.canceled)
            player.playerWalkingState.ResetWalkSound();
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
        int slot = int.Parse(control.name);
        player.playerSwitchingState.SetWeaponSlot(slot);
        Debug.Log(player.playerInventory.CheckSlotExist(slot));//fix reload earlier ma relaod
        if(!player.playerSwitchingState.CheckCanSwitch(slot) || !player.playerInventory.CheckSlotExist(slot))
            return;
        player.PlayerStateController.TryChangeState(E_PlayerStates.SWITCH);
        player.weaponController.DecreaseBulletCount(0);//just to update
        
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
           // player.playerLeftThrowState.SetSlot(context.control.name);
           // player.PlayerStateController.TryChangeState(E_PlayerLeftStates.THROW);
        }
    }

    public void OnOnPause(InputAction.CallbackContext context)
    {
        menuHandler.SetPauseVisible(true);
    }
}
