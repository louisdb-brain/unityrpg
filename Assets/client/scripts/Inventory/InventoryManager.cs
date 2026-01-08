using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("UI Setup")]
    public InventorySlotUI slotPrefab;
    public Transform slotParent;
    public int slotCount = 28;

    // =========================
    // DATA
    // =========================

    [System.Serializable]
    public class InventorySlot
    {
        public Item item;
        public int amount;
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

    void Start()
    {
        // Initialize inventory data
        inventory = new InventorySlot[slotCount];
        for (int i = 0; i < slotCount; i++)
            inventory[i] = new InventorySlot();

        BuildSlots();
        SetActiveSlot(0);

        Debug.Log("InventoryManager STARTED");
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

            UpdateSlotUI(i);
        }
    }

    // =========================
    // PUBLIC API (USED BY LOOT)
    // =========================

    public bool AddItem(Item item)
    {
        // 1️⃣ Try stacking
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].item == item)
            {
                inventory[i].amount++;
                UpdateSlotUI(i);
                return true;
            }
        }

        // 2️⃣ Find empty slot
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].item == null)
            {
                inventory[i].item = item;
                inventory[i].amount = 1;
                UpdateSlotUI(i);
                return true;
            }
        }

        // 3️⃣ Inventory full
        return false;
    }

    // =========================
    // UI INTERACTION
    // =========================

    public void OnSlotClicked(int index)
    {
        // Grab
        if (grabbedSlotIndex == -1)
        {
            grabbedSlotIndex = index;
            SetActiveSlot(index);
        }
        // Drop / swap
        else
        {
            SwapSlots(grabbedSlotIndex, index);
            grabbedSlotIndex = -1;
        }
    }

    private void SwapSlots(int from, int to)
    {
        InventorySlot temp = inventory[from];
        inventory[from] = inventory[to];
        inventory[to] = temp;

        UpdateSlotUI(from);
        UpdateSlotUI(to);

        SetActiveSlot(to);
    }

    // =========================
    // UI HELPERS
    // =========================

    private void UpdateSlotUI(int index)
    {
        InventorySlot slot = inventory[index];

        if (slot.item == null)
        {
            slots[index].SetIcon(null);
        }
        else
        {
            slots[index].SetIcon(slot.item.icon);
        }
    }

    private void SetActiveSlot(int index)
    {
        activeSlotIndex = index;

        for (int i = 0; i < slots.Count; i++)
            slots[i].SetActive(i == activeSlotIndex);
    }

    private void MoveActive(int delta)
    {
        int next = Mathf.Clamp(activeSlotIndex + delta, 0, slotCount - 1);
        SetActiveSlot(next);
    }
}
