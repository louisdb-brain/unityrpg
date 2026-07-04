using UnityEngine;
using UnityEngine.UI;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance { get; private set; }

    [Header("Data")]
    public SpellDatabase spellDatabase;

    [Header("HUD (prefab-wired)")]
    public RectTransform hudRoot;
    public SpellQuickSlotUI[] spellSlotUIs = new SpellQuickSlotUI[4];
    public ItemQuickSlotUI[] itemSlotUIs = new ItemQuickSlotUI[4];

    private const int SpellSlotCount = 4;
    private const int ItemSlotCount = 4;

    private readonly int[] itemBagRefs = new int[ItemSlotCount];
    private readonly SpellPrototype[] spellSlots = new SpellPrototype[SpellSlotCount];
    private int[] spellCycleIndices;
    private int activeSpellSlotIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < ItemSlotCount; i++)
            itemBagRefs[i] = -1;
    }

    void Start()
    {
        InitDefaultSpells();
        WireSlots();
        RefreshAllItemSlots();
        RefreshAllSpellSlots();
        SelectSpellSlot(0);
    }

    void Update()
    {
        if (PlayerManager.Instance == null || PlayerManager.Instance.GetLocalPlayer() == null)
            return;

        HandleSpellHotkeys();
        HandleItemHotkeys();
    }

    void InitDefaultSpells()
    {
        if (spellDatabase == null || spellDatabase.spells == null)
            return;

        spellCycleIndices = new int[SpellSlotCount];
        for (int i = 0; i < SpellSlotCount && i < spellDatabase.spells.Count; i++)
        {
            spellSlots[i] = spellDatabase.spells[i];
            spellCycleIndices[i] = i;
        }
    }

    void WireSlots()
    {
        if (hudRoot == null)
        {
            Debug.LogError("[QuickSlotManager] hudRoot is not assigned in the prefab.");
            return;
        }

        if (spellSlotUIs == null || spellSlotUIs.Length == 0 || spellSlotUIs[0] == null)
        {
            var spellRow = hudRoot.Find("SpellQuickSlots");
            if (spellRow != null)
            {
                spellSlotUIs = spellRow.GetComponentsInChildren<SpellQuickSlotUI>(true);
                Debug.LogWarning("[QuickSlotManager] spellSlotUIs auto-resolved from prefab hierarchy.");
            }
        }

        if (itemSlotUIs == null || itemSlotUIs.Length == 0 || itemSlotUIs[0] == null)
        {
            var itemRow = hudRoot.Find("ItemQuickSlots");
            if (itemRow != null)
            {
                itemSlotUIs = itemRow.GetComponentsInChildren<ItemQuickSlotUI>(true);
                Debug.LogWarning("[QuickSlotManager] itemSlotUIs auto-resolved from prefab hierarchy.");
            }
        }

        for (int i = 0; i < spellSlotUIs.Length; i++)
        {
            if (spellSlotUIs[i] != null)
                spellSlotUIs[i].Init(i, this);
        }

        for (int i = 0; i < itemSlotUIs.Length; i++)
        {
            if (itemSlotUIs[i] != null)
                itemSlotUIs[i].Init(i, this);
        }
    }

    public bool TryAssignFromBag(int quickSlotIndex, int bagIndex)
    {
        if (InventoryManager.Instance == null)
            return false;

        Item item = InventoryManager.Instance.GetItemAt(bagIndex);
        if (item == null)
            return false;

        if (quickSlotIndex == 0)
        {
            if (item is not weapon)
                return false;

            itemBagRefs[0] = bagIndex;
            RefreshItemSlot(0);
            InventoryManager.Instance.EquipFromBag(bagIndex);
            return true;
        }

        if (item is weapon)
            return false;

        itemBagRefs[quickSlotIndex] = bagIndex;
        RefreshItemSlot(quickSlotIndex);
        return true;
    }

    public void TrySwapQuickSlots(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex)
            return;

        if (InventoryManager.Instance == null)
            return;

        int fromBag = itemBagRefs[fromIndex];
        if (fromBag < 0)
            return;

        Item item = InventoryManager.Instance.GetItemAt(fromBag);
        if (item == null)
        {
            ClearItemQuickSlot(fromIndex);
            return;
        }

        if (toIndex == 0 && item is not weapon)
            return;

        if (toIndex != 0 && item is weapon)
            return;

        int temp = itemBagRefs[toIndex];
        itemBagRefs[toIndex] = fromBag;
        itemBagRefs[fromIndex] = temp;

        RefreshItemSlot(fromIndex);
        RefreshItemSlot(toIndex);

        if (toIndex == 0)
            InventoryManager.Instance.EquipFromBag(fromBag);
        else if (fromIndex == 0)
            InventoryManager.Instance.UnequipWeapon();
    }

    public void ClearItemQuickSlot(int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= ItemSlotCount)
            return;

        bool wasWeapon = quickSlotIndex == 0 && itemBagRefs[0] >= 0;
        itemBagRefs[quickSlotIndex] = -1;
        RefreshItemSlot(quickSlotIndex);

        if (wasWeapon)
            InventoryManager.Instance?.UnequipWeapon();
    }

    public void OnQuickSlotDroppedOnBag(int quickSlotIndex)
    {
        ClearItemQuickSlot(quickSlotIndex);
    }

    public void OnBagSlotChanged(int bagIndex)
    {
        for (int i = 0; i < ItemSlotCount; i++)
        {
            if (itemBagRefs[i] != bagIndex)
                continue;

            Item item = InventoryManager.Instance?.GetItemAt(bagIndex);
            if (item == null || !IsValidForQuickSlot(i, item))
                ClearItemQuickSlot(i);
            else
                RefreshItemSlot(i);
        }
    }

    bool IsValidForQuickSlot(int quickSlotIndex, Item item)
    {
        if (quickSlotIndex == 0)
            return item is weapon;
        return item is not weapon;
    }

    public void RefreshItemSlot(int quickSlotIndex)
    {
        if (itemSlotUIs == null || quickSlotIndex < 0 || quickSlotIndex >= itemSlotUIs.Length)
            return;

        int bagIndex = itemBagRefs[quickSlotIndex];
        Sprite icon = null;

        if (bagIndex >= 0 && InventoryManager.Instance != null)
        {
            Item item = InventoryManager.Instance.GetItemAt(bagIndex);
            if (item != null)
                icon = item.icon;
        }

        itemSlotUIs[quickSlotIndex].SetIcon(icon);
    }

    void RefreshAllItemSlots()
    {
        for (int i = 0; i < ItemSlotCount; i++)
            RefreshItemSlot(i);
    }

    public void CycleSpellSlot(int slotIndex)
    {
        if (spellDatabase == null || spellDatabase.spells == null || spellDatabase.spells.Count == 0)
            return;

        if (spellCycleIndices == null)
            spellCycleIndices = new int[SpellSlotCount];

        spellCycleIndices[slotIndex] = (spellCycleIndices[slotIndex] + 1) % spellDatabase.spells.Count;
        spellSlots[slotIndex] = spellDatabase.spells[spellCycleIndices[slotIndex]];
        RefreshSpellSlot(slotIndex);
        SelectSpellSlot(slotIndex);
    }

    void RefreshSpellSlot(int index)
    {
        if (spellSlotUIs == null || index < 0 || index >= spellSlotUIs.Length)
            return;

        spellSlotUIs[index].SetSpell(spellSlots[index]);
    }

    void RefreshAllSpellSlots()
    {
        for (int i = 0; i < SpellSlotCount; i++)
            RefreshSpellSlot(i);
    }

    void SelectSpellSlot(int index)
    {
        activeSpellSlotIndex = index;

        var localPlayer = PlayerManager.Instance?.GetLocalPlayer();
        if (localPlayer != null && spellSlots[index] != null)
            localPlayer.activeSpell = spellSlots[index];

        if (spellSlotUIs == null)
            return;

        for (int i = 0; i < spellSlotUIs.Length; i++)
            spellSlotUIs[i].SetSelected(i == index);
    }

    void HandleSpellHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSpellSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSpellSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSpellSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSpellSlot(3);
    }

    void HandleItemHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.E)) UseItemQuickSlot(1);
        if (Input.GetKeyDown(KeyCode.R)) UseItemQuickSlot(2);
        if (Input.GetKeyDown(KeyCode.T)) UseItemQuickSlot(3);
    }

    void UseItemQuickSlot(int quickSlotIndex)
    {
        if (InventoryManager.Instance == null)
            return;

        int bagIndex = itemBagRefs[quickSlotIndex];
        if (bagIndex < 0)
            return;

        Item item = InventoryManager.Instance.GetItemAt(bagIndex);
        if (item == null)
        {
            ClearItemQuickSlot(quickSlotIndex);
            return;
        }

        var localPlayer = PlayerManager.Instance.GetLocalPlayer();
        if (localPlayer == null)
            return;

        item.onUse(localPlayer.gameObject);
    }
}
