using UnityEngine;
using UnityEngine.InputSystem;
using static InputActions_Player;
using static PlayerStateController;
public class UI_InputHandler : IUIActions
{
    Player player;
    InputActions_Player inputActions;
    UIActions uiActions;
    MenuHandler menuHandler;

    public UI_InputHandler(Player player, InputActions_Player inputActions, MenuHandler menuHandler)
    {
        this.inputActions = inputActions;
        uiActions = inputActions.UI;
        uiActions.SetCallbacks(this);
        uiActions.Disable();
        this.player = player;
        this.menuHandler = menuHandler;
    }
    public void OnPlayer_CloseInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UIHandler.Instance.SetInventoryVisible(false);
            //Cursor.lockState = CursorLockMode.Locked;
        }
        
    }
    public void OnCancel(InputAction.CallbackContext context)
    {

    }

    public void OnClick(InputAction.CallbackContext context)
    {

    }

    public void OnMiddleClick(InputAction.CallbackContext context)
    {

    }

    public void OnNavigate(InputAction.CallbackContext context)
    {

    }

    public void OnPoint(InputAction.CallbackContext context)
    {

    }

    public void OnRightClick(InputAction.CallbackContext context)
    {

    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {

    }

    public void OnSubmit(InputAction.CallbackContext context)
    {

    }

    public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
    {

    }

    public void OnTrackedDevicePosition(InputAction.CallbackContext context)
    {

    }
    public void SetEnabled(bool enabled)
    {
        if (enabled)
            uiActions.Enable();
        else
            uiActions.Disable();
    }

    public void OnClosePause(InputAction.CallbackContext context)
    {
        menuHandler.SetPauseVisible(false);
    }
}
