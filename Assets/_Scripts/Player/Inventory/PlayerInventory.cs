using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInventory : MonoBehaviour
{
    //simple slot on left. clicking it shows stuff on right
    //only on level start dapat kita
    private Dictionary<WeaponType, Slot> inventory;//inside inventory
    [SerializeField] private UIDocument inventoryDocument;
    [SerializeField] public GameObject inventoryTablet;
    [NonSerialized] public VisualElement inventoryRoot;
    [NonSerialized] public VisualElement equippedSlots;
    [SerializeField] private GameObject weaponParent;
    [Header("Default")]
    [SerializeField] private GameObject handGunPrefab;
    Player player;

    #region ACTIVE ITEM INFORMATION
    public Slot activeSlot;
    private GameObject activeWeaponObject;
    [SerializeField] private GameObject activeWeaponObjectParent;
    private Vector3 activeWeaponParentScale = Vector3.one;
    private int alwaysOnTopLayer;
    #endregion

    private void Awake()
    {
        player = GetComponent<Player>();
        alwaysOnTopLayer = LayerMask.NameToLayer("AlwaysOnTop");
        if (activeWeaponObjectParent != null)
            activeWeaponParentScale = activeWeaponObjectParent.transform.localScale;

        inventory = new();
        inventory.Add(WeaponType.Handgun, new Slot(2));
        inventory.Add(WeaponType.Shotgun, new Slot(2));
        inventory.Add(WeaponType.CrossGun, new Slot(2));
        StartCoroutine(SetupWorldSpaceUI());
    }

    public List<Slot> GetInventorySlots()
    {
        return new List<Slot>(inventory.Values);
    }

    private System.Collections.IEnumerator SetupWorldSpaceUI()
    {
        while (inventoryDocument == null || inventoryDocument.rootVisualElement == null || UIHandler.Instance == null)
        {
            yield return null;
        }

        inventoryRoot = inventoryDocument.rootVisualElement;
        foreach (VisualElement v in inventoryRoot.Children())
        {
            v.pickingMode = PickingMode.Ignore;
        }
        equippedSlots = inventoryRoot.Q<VisualElement>("vs-equipped");
        UIHandler.Instance.SetInventoryVisible(false);
    }

    public void HideWeaponGameobject(bool hide)
    {
        if (activeWeaponObjectParent == null) return;

        activeWeaponObjectParent.transform.localScale = hide ? Vector3.zero : activeWeaponParentScale;
    }

    private void PopulateSlot()
    {
        if (inventory == null || equippedSlots == null) return;

        equippedSlots.Clear(); // idempotent: Show()/OnEnable can both call this
        foreach (KeyValuePair<WeaponType, Slot> slot in inventory)
        {
            equippedSlots.Add(slot.Value);
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
        if (!inventory.TryGetValue(weaponType, out Slot slot))
        {
            Debug.LogWarning($"No inventory slot for {weaponType}");
            return;
        }
        if (slot.IsFull)
        {
            Debug.Log("Wala space");
            return;
        }

        player.playerIdleState.ExitState();

        slot.AddItem(item.gameObject, item.data);
        activeSlot = slot;

        SwapWeaponObjects();

        player.gunAnimator.SetBool(activeSlot.activeItem.weaponClass.GUN_idleTrigger, true);
        player.armAnimator.SetBool(activeSlot.activeItem.weaponClass.ARM_idleTrigger, true);

        player.playerSwitchingState.SetWeaponSlot(GetIndexFromWeapon(weaponType) + 1);
        player.PlayerStateController.TryChangeState(E_PlayerStates.SWITCH);
    }

    public void TrySwitchWeapon(int index)
    {
        WeaponType typeToEquip = GetWeaponTypeFromIndex(index - 1);
        if (!inventory.TryGetValue(typeToEquip, out Slot slot) || slot == null)
        {
            Debug.LogWarning($"No slot for key {index}");
            return;
        }
        if (slot.activeItem == null)
        {
            Debug.LogWarning($"No Active Weapon to equip for {typeToEquip} slot");
            return;
        }

        bool sameSlot = slot == activeSlot;
        if (sameSlot && !slot.CanSwitchActiveItem())
            return; // nothing to cycle to

        player.playerIdleState.ExitState();

        if (sameSlot)
            slot.SwitchActiveItem();
        activeSlot = slot;
        SwapWeaponObjects();
        player.playerIdleState.EnterState();
    }

    private void SwapWeaponObjects()
    {
        //player.PlayerStateController.reloadTime = activeSlot.activeItem.data.reloadTime;
        //player.playerShootingState.UpdateWeapon()
        RealUnequip();
        RealEquip(activeSlot.activeItem.weaponPrefab);
        player.weaponController.DecreaseBulletCount(0);
    }

    private void RealEquip(GameObject weaponPrefab)//the gameobject parenting shit
    {
        activeWeaponObject = Instantiate(weaponPrefab, activeWeaponObjectParent.transform, false);

        BaseWeaponClass weaponClass = activeWeaponObject.GetComponent<BaseWeaponClass>();
        player.weaponController.fireOrigin = weaponClass.fireOrigin;
        player.weaponController.data = weaponClass.data;
        player.weaponController.weaponState = weaponClass;

        // GetComponentsInChildren includes the root itself.
        foreach (Transform t in activeWeaponObject.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = alwaysOnTopLayer;
        }

        player.gunAnimator = activeWeaponObject.GetComponent<Animator>();
        Vector3 gunCamPosition = player.cameraHandler.gunCamera.transform.localPosition;
        player.cameraHandler.gunCamera.transform.localPosition = new Vector3(gunCamPosition.x, gunCamPosition.y, weaponClass.gunCameraZ);
    }

    private void RealUnequip()
    {
        if (activeWeaponObject == null) return; // first equip: nothing to remove

        // Destroy() is deferred to end of frame; disable now so the old gun/animator
        // can't overlap or keep animating next to the new one.
        activeWeaponObject.SetActive(false);
        Destroy(activeWeaponObject);
        activeWeaponObject = null;
    }

    private WeaponType GetWeaponTypeFromIndex(int index)
    {
        switch (index)
        {
            case 0: return WeaponType.Handgun;
            case 1: return WeaponType.Shotgun;
            case 2: return WeaponType.CrossGun;
            default: return WeaponType.None;
        }
    }

    private int GetIndexFromWeapon(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Handgun: return 0;
            case WeaponType.Shotgun: return 1;
            case WeaponType.CrossGun: return 2; // was missing -> returned -1
            default: return -1;
        }
    }

    private void OnEnable() => PopulateSlot();
    private void OnDisable() => equippedSlots?.Clear();
    private void OnDestroy()
    {
        equippedSlots?.Clear();
        inventory?.Clear();
    }

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

    public bool IsFull => items.Count >= maxItems;

    public Slot(int maxItems)
    {
        this.maxItems = maxItems;
        items = new List<Item>();
        BuildSlot();
        btn_interact.RegisterCallback<ClickEvent>(OnClick);
        activeItem = null;
    }

    public bool AddItem(GameObject weaponPrefab, WeaponData data)
    {
        if (IsFull)
            return false;
        Item item = new Item(weaponPrefab, data);
        items.Add(item);
        // The newly picked-up weapon is the one that gets equipped. Before, a slot that
        // became full never updated activeItem, so the pickup switched to the old weapon.
        SetActiveItem(item);
        return true;
    }

    public bool CanSwitchActiveItem()
    {
        return items.Count >= 2 && items.IndexOf(activeItem) != -1;
    }

    public bool SwitchActiveItem()
    {
        if (items.Count == 0)
        {
            Debug.LogWarning("no item/s in the slot");
            return false;
        }
        if (items.Count < 2)
        {
            Debug.LogWarning("Only one item in slot, cannot switch to another weapon.");
            return false;
        }
        int equippedIndex = items.IndexOf(activeItem);
        if (equippedIndex == -1)
        {
            Debug.LogError("NO ACTIVE ITEM");
            return false;
        }
        int switchIndex = (equippedIndex == 0) ? 1 : 0;
        SetActiveItem(items[switchIndex]);
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
        foreach (VisualElement child in children)
        {
            if (child.ClassListContains("equipped-icon") || child.ClassListContains("equipped-interact"))
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
        public int currentBulletCount;
        public Item(GameObject weaponPrefab, WeaponData data)
        {
            this.weaponPrefab = weaponPrefab;
            this.data = data;
            this.weaponClass = weaponPrefab.GetComponent<BaseWeaponClass>();
            currentBulletCount = data.maxBulletCount;
            AddToClassList("equipped-icon");
            style.backgroundImage = data.icon;
            pickingMode = PickingMode.Position;
        }
    }
}