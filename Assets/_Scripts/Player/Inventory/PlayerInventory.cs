using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{
    //simple slot on left. clicking it shows stuff on right
    //only on level start dapat kita
    private Dictionary<WeaponType, Slot> inventory;//inside inventory
    private List<Slot> availableItems;//available for equip
    [SerializeField]private UIDocument inventoryDocument;
    [SerializeField]public GameObject inventoryTablet;
    [NonSerialized]public VisualElement inventoryRoot;
    [NonSerialized]public VisualElement equippedSlots;
    [SerializeField] private GameObject weaponParent;
    [Header("Default")]
    [SerializeField]private GameObject handGunPrefab;
    private int maxItemPerWeaponType = 2;
    Player player;
    #region ACTIVE ITEM INFORMATION
    public Slot activeSlot;
    private GameObject activeWeaponObject;
    [SerializeField]private GameObject activeWeaponObjectParent;
    private Vector3 activeWeaponScale;
    #endregion

    private void Awake()
    {
        player = GetComponent<Player>();
        inventory = new();
        availableItems = new();
        StartCoroutine(SetupWorldSpaceUI());
        inventory.Add(WeaponType.Handgun, new Slot(2));
        inventory.Add(WeaponType.Shotgun, new Slot(2));
    }
    private System.Collections.IEnumerator SetupWorldSpaceUI()
    {
        while (inventoryDocument == null || inventoryDocument.rootVisualElement == null)
        {
            //Debug.Log("SDFDF");
            yield return null;
        }

        inventoryRoot = inventoryDocument.rootVisualElement;
        foreach(VisualElement v in inventoryRoot.Children())
        {
            v.pickingMode = PickingMode.Ignore;
        }
        equippedSlots = inventoryRoot.Q<VisualElement>("vs-equipped");
        UIHandler.Instance.SetInventoryVisible(false);
    }
    public void HideWeaponGameobject(bool hide)
    {
        if (activeWeaponObjectParent == null) return;

        // Get the animator that is locking the transform
        Animator weaponAnimator = player.gunAnimator;

        if (hide)
        {
            activeWeaponScale = Vector3.one;
            activeWeaponObjectParent.transform.localScale = Vector3.zero;
        }
        else
        {
            activeWeaponObjectParent.transform.localScale = activeWeaponScale;
            
        }
    }

    private void PopulateSlot()
    {
        if (inventory == null)
        {
            Debug.Log("null");
            return;
        }
        foreach(KeyValuePair<WeaponType, Slot> slot in inventory)
        {
            equippedSlots.Add(slot.Value);
            Debug.Log("popualate1");
        }
    }
    public void Initialize()
    {
        BaseWeaponClass handgunWeaponClass = handGunPrefab.GetComponent<BaseWeaponClass>();
        AddToSlot(handgunWeaponClass);
    }

    public void AddToSlot(BaseWeaponClass item)
    {
        WeaponType weaponType = item.data.weaponType;
        foreach (KeyValuePair<WeaponType, Slot> slots in inventory)
        {
            if (!slots.Key.Equals(weaponType))
                continue;
            if(slots.Value.AddItem(item.gameObject, item.data))
            {
                player.playerIdleState.ExitState();
                activeSlot = slots.Value;
                player.playerSwitchingState.SetWeaponSlot(GetIndexFromWeapon(slots.Key)+1);
                player.PlayerStateController.TryChangeState(E_PlayerStates.SWITCH);
                SwitchWeapon();
            }
            else
            {
                Debug.Log("Wala space");
            }
        }
    }
    public void TrySwitchWeapon(int index)
    {
        index -= 1;
        WeaponType typeToEquip = GetWeaponTypeFromIndex(index);
        Slot slot = inventory[typeToEquip];
        bool willSwitch = true;
        if (slot == null)
        {
            Debug.LogError("Equipped null slot");
            return;
        }
        if(slot.activeItem == null)
        {
            Debug.LogWarning($"No Active WEpon to equip for {typeToEquip} slot");
            return;
        }
        if(slot == activeSlot)
        {
            //switch active wepon there
            willSwitch = slot.SwitchActiveItem();
        }
        if(willSwitch)
        {
            player.playerIdleState.ExitState();
            activeSlot = slot;
            SwitchWeapon();
        }
        
        //REMOVE THE COLLIDER AFTER EQUUOP CUH
    }
    private void SwitchWeapon()
    {
        RealUnequip();
        RealEquip(activeSlot.activeItem.weaponPrefab);
        player.playerIdleState.EnterState();
    }
    private void RealEquip(GameObject weaponPrefab)//the gameobject parenting shit
    {
        //player.gunAnimator = weaponPrefab.GetComponent<Animator>();
        activeWeaponObject = Instantiate(weaponPrefab, activeWeaponObjectParent.transform, false);

        BaseWeaponClass weaponClass = activeWeaponObject.GetComponent<BaseWeaponClass>();
        player.weaponController.fireOrigin = weaponClass.fireOrigin;
        player.weaponController.data = weaponClass.data;
        activeWeaponObject.layer = LayerMask.NameToLayer("AlwaysOnTop");
        //activeWeaponPos = activeWeaponObject.transform.localPosition;

        Transform[] children = activeWeaponObject.GetComponentsInChildren<Transform>();
        foreach (Transform t in children)
        {
            t.gameObject.layer = LayerMask.NameToLayer("AlwaysOnTop");
        }
        
        //activeWeaponObject.GetComponent<BoxCollider>().enabled = false;
        player.gunAnimator = activeWeaponObject.GetComponent<Animator>();
        Vector3 gunCamPosition = player.cameraHandler.gunCamera.transform.localPosition;
        player.cameraHandler.gunCamera.transform.localPosition = new Vector3(gunCamPosition.x, gunCamPosition.y, weaponClass.gunCameraZ);
    }
    private void RealUnequip()
    {
        if(activeWeaponObject == null)
        {
            Debug.LogWarning("No Active WEpon to unequip");
            return;
        }
        Destroy(activeWeaponObject);
    }
    private WeaponType GetWeaponTypeFromIndex(int index)
    {
        switch(index)
        {
            case 0:
                return WeaponType.Handgun;
            case 1:
                return WeaponType.Shotgun;
            default:
                return WeaponType.None;
        }
    }
    private int GetIndexFromWeapon(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Handgun:
                return 0;
            case WeaponType.Shotgun:
                return 1;
            default :
                return -1;
        }
    }
    private void OnEnable()=>PopulateSlot();
    private void OnDisable()=>equippedSlots.Clear();
    private void OnDestroy(){equippedSlots.Clear();inventory.Clear();}
    public void Show()
    {
        if (inventoryRoot != null) inventoryRoot.style.display = DisplayStyle.Flex;
        PopulateSlot();
    }
    public void Hide()
    {
        if (inventoryRoot != null) inventoryRoot.style.display = DisplayStyle.None;
    }

}
public class Slot : VisualElement
{
    VisualElement vs_icon;
    VisualElement btn_left;
    VisualElement btn_right;
    VisualElement btn_interact;
    Label btn_label;
    List<Item> items;
    int maxItems;
    public Item activeItem;

    public Slot(int maxItems)
    {
        items = new List<Item>();
        BuildSlot();
        this.maxItems = maxItems;
        btn_interact.RegisterCallback<ClickEvent>(OnClick);
        activeItem = null;
    }
    public bool AddItem(GameObject weaponPrefab, WeaponData data)
    {
        if(items.Count >= maxItems)
            return false;
        Item item = new Item(weaponPrefab, data);
        items.Add(item);
        if (items.Count < maxItems)
            SetActiveItem(item);
        return true;
    }
    public bool SwitchActiveItem()
    {
        if(items.Count == 0)
        {
            Debug.LogWarning("no item/s in the slot");
            return false;
        }
        if(items.Count <2)
        {
            Debug.LogWarning("Only one item in slot, cannot swithch to another weapon.");
            return false;
        }
        int equippedIndex = items.IndexOf(activeItem);
        if(equippedIndex == -1)
        {
            Debug.LogError("NO ACTIVE ITEM");
            return false;
        }
        int switchIndex = (equippedIndex == 0) ? 1 : 0;
        Item otherItem = items[switchIndex];
        SetActiveItem(otherItem);
        return true;
    }
    private void SetActiveItem(Item item)
    {
        activeItem = item;
        UpdateSlot();
    }
    private void OnClick(ClickEvent evt)
    {
        Debug.Log("Hello");
    }
    private void UpdateSlot()
    {
        List<VisualElement> children = new List<VisualElement>(Children());
        foreach(VisualElement child in children)
        {
            if(child.ClassListContains("equipped-icon")|| child.ClassListContains("equipped-interact"))
                Remove(child);
        }
        Add(activeItem);
        Add(btn_interact);
            
    }
    private void BuildSlot()
    {
        vs_icon = new VisualElement();//empty fornow
        btn_left = new VisualElement();
        btn_right = new VisualElement();
        btn_interact = new VisualElement();
        btn_label = new Label();

        vs_icon.pickingMode = PickingMode.Position;
        btn_left.pickingMode = PickingMode.Ignore;
        btn_right.pickingMode = PickingMode.Ignore;
        btn_interact.pickingMode = PickingMode.Position;
        btn_label.pickingMode = PickingMode.Ignore;

        AddToClassList("equipped-slot");
        vs_icon.AddToClassList("equipped-icon");
        btn_left.AddToClassList("left");
        btn_right.AddToClassList("right");
        btn_interact.AddToClassList("equipped-interact");
        btn_label.AddToClassList("equipped-interact-label");

        vs_icon.Add(btn_left);
        vs_icon.Add(btn_right);
        btn_interact.Add(btn_label);
        Add(vs_icon);
        Add(btn_interact);
    }
    public class Item : VisualElement
    {
        public GameObject weaponPrefab;
        public BaseWeaponClass weaponClass;
        public WeaponData data;
        public Item(GameObject weaponPrefab, WeaponData data)
        {
            this.weaponPrefab = weaponPrefab;
            this.data = data;
            this.weaponClass = weaponPrefab.GetComponent<BaseWeaponClass>();
            AddToClassList("equipped-icon");
            style.backgroundImage = data.icon;
            pickingMode = PickingMode.Position;
        }
    }
}
