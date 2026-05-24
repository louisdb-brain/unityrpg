using UnityEngine;
using UnityEngine.EventSystems;

public enum dropZoneType
{
    DROPZONE,COOKING,SMITHING,CRAFTING,WEAPON,SHOP,QUEST
}
public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    public InventoryManager inventoryManager;
    public dropZoneType zone;

    void Awake()
    {
        if (zone == dropZoneType.DROPZONE && inventoryManager != null)
            inventoryManager.RegisterDropBackdrop(gameObject);
    }

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
        switch (zone)
        {
            case dropZoneType.DROPZONE:
                Debug.Log("DROPPED IN DROPZONE");
                inventoryManager.DropItem(fromSlot);
                break;
            
            
        }
        
    }
}