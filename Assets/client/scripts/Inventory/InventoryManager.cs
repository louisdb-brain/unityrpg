using System.Collections.Generic;
using UnityEngine;

public  class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public ItemDatabase database;
    [Header("UI Setup")]
    public InventorySlotUI slotPrefab;
    public Transform slotParent;
    public int slotCount = 28;
    [Header(" INVENTORY (Editor Only)")]
    public List<Item> Items = new List<Item>();

    // =========================
    // DATA
    // =========================

    [System.Serializable]
    public class InventorySlot
    {
        public Item item;
        //public int amount;
    }

    private InventorySlot[] inventory;
    private List<InventorySlotUI> slots = new List<InventorySlotUI>();

    // =========================
    // STATE
    // =========================

    private int activeSlotIndex = 0;
    private int grabbedSlotIndex = -1;

    // =========================
    // UNITY
    // =========================
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    
        Instance = this;
    }
    void Start()
    {
        // Initialize inventory data
        inventory = new InventorySlot[slotCount];
        for (int i = 0; i < slotCount; i++)
            inventory[i] = new InventorySlot();

        BuildSlots();
        SetActiveSlot(0);

        Debug.Log("InventoryManager STARTED");
        foreach (Item item in Items)
        {
            if (item == null)
                continue;

            bool added = AddItem(item);

            if (!added)
            {
                Debug.LogWarning("Inventory full, could not add test item: " + item.name);
                break;
            }
        }
        Debug.Log("InventoryManager STARTED (Test Mode)");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveActive(1);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveActive(-1);
    }

    // =========================
    // SLOT BUILDING
    // =========================

    private void BuildSlots()
    {
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, slotParent);
            slot.Init(i, this);
            slots.Add(slot);

            Debug.Log($"[BuildSlots] index={i}, instanceID={slot.GetInstanceID()}");

            UpdateSlotUI(i);
        }
    }


    // =========================
    // PUBLIC API (USED BY LOOT)
    // =========================

    public bool AddItem(Item item)
    {
        // Find first empty slot
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].item == null)
            {
                inventory[i].item = item;
                UpdateSlotUI(i);
                return true;
            }
        }
        

        // Inventory full
        return false;
    }
   


    // =========================
    // UI INTERACTION
    // =========================

    public void OnSlotClicked(int index)
    {
        // Nothing grabbed yet → try to grab
        if (grabbedSlotIndex == -1)
        {
            // ❌ Do NOT grab empty slots
            if (inventory[index].item == null)
                return;

            grabbedSlotIndex = index;
            SetActiveSlot(index);
            return;
        }

        // Something is grabbed → drop / move
        MoveItem(grabbedSlotIndex, index);
        grabbedSlotIndex = -1;
        
    }

    public void OnSlotRightClick(int index)
    {
        Item thisitem = inventory[index].item;
        
        //don't rightclick empty items
        if( thisitem== null)
            return;
        bool isbase=false;
        if (thisitem is buildingItem build && build.properties.HasFlag(BuildPropertyType.Base))
        {
            isbase = true;
        }
        ConstructionPlacer.Instance.StartPlacing(thisitem.icon,isbase);
        // Right-click = start building
        
    }


    public void MoveItem(int from, int to)
    {
        if (from == to)
            return;

        Item temp = inventory[from].item;
        inventory[from].item = inventory[to].item;
        inventory[to].item = temp;

        UpdateSlotUI(from);
        UpdateSlotUI(to);
        SetActiveSlot(to);
        
    }


    // =========================
    // UI HELPERS
    // =========================

    public void UpdateSlotUI(int index)
    {
        InventorySlot slot = inventory[index];

        if (slot.item == null)
        {
            slots[index].SetIcon(null);
            Debug.Log("no icon for slot" );
        }
        else
        {
            slots[index].SetIcon(slot.item.icon);
        }
    }

    public void SetActiveSlot(int index)
    {
        activeSlotIndex = index;

        for (int i = 0; i < slots.Count; i++)
            slots[i].SetActive(i == activeSlotIndex);
    }

    public void MoveActive(int delta)
    {
        int next = Mathf.Clamp(activeSlotIndex + delta, 0, slotCount - 1);
        SetActiveSlot(next);
    }
}
