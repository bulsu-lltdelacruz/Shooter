using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler
{
    InputHandler playerInputs;
    UI_InputHandler uiInputs;
    GameObject inventoryTablet;
    Player player;
    public static UIHandler Instance;
    public UIHandler(GameObject inventoryTablet, InputHandler playerInputs, UI_InputHandler uiInputs, Player player)
    {
        this.inventoryTablet = inventoryTablet;
        this.playerInputs = playerInputs;
        this.uiInputs = uiInputs;
        this.player = player;
        if(Instance == null)
            Instance = this;
        //this.inventoryRoot.style.display = DisplayStyle.None;
    }
    public void SetInventoryVisible(bool show)
    {
        if (show)
        {
            //inventoryTablet.SetActive(show);
            player.playerInventory.Show();
            playerInputs.SetEnabled(false);
            uiInputs.SetEnabled(true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            player.playerInventory.Hide();
            playerInputs.SetEnabled(true);
            uiInputs.SetEnabled(false);
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            //inventoryTablet.SetActive(show);
        }
    }
}
