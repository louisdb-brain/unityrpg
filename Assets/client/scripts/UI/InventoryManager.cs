using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] slots;

    public bool AddItem(Item newItem)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.currentItem == null)
            {
                slot.SetItem(newItem);
                return true;
            }
        }

        Debug.Log("Inventory Full!");
        return false;
    }
}