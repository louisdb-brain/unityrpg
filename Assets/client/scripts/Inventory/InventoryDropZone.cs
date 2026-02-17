using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    public InventoryManager inventoryManager;

    public void OnDrop(PointerEventData eventData)
    {
        if (inventoryManager == null)
            return;

        var draggedGO = eventData.pointerDrag;
        if (draggedGO == null)
            return;

        var drag = draggedGO.GetComponent<InventoryDragItem>();
        if (drag == null)
            return;

        int fromSlot = drag.FromSlotIndex;
        if (fromSlot < 0)
            return;

        // Tell inventory to drop/remove the item
        inventoryManager.DropItem(fromSlot);
    }
}