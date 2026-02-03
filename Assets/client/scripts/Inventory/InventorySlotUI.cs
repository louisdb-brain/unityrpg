using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    [Header("UI")]
    public Image iconImage;
    public Image highlightImage;

    [HideInInspector] public int slotIndex;
    public InventoryManager inventoryManager;

    void Awake()
    {
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();

        if (highlightImage == null)
            highlightImage = transform.Find("Highlight")?.GetComponent<Image>();
    }

    public void Init(int index, InventoryManager manager)
    {
        slotIndex = index;
        inventoryManager = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventoryManager?.OnSlotRightClick(slotIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            inventoryManager?.OnSlotClicked(slotIndex);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventoryManager == null) return;

        var draggedGO = eventData.pointerDrag;
        if (draggedGO == null) return;

        var drag = draggedGO.GetComponent<InventoryDragItem>();
        if (drag == null) return;

        int from = drag.FromSlotIndex;
        int to = slotIndex;

        if (from == to) return;

        // swap/move in the DATA model
        inventoryManager.MoveItem(from, to);

        // UI refresh (your MoveItem likely already calls UpdateSlotUI,
        // but calling refresh here is safe if it doesn't)
        inventoryManager.UpdateSlotUI(from);
        inventoryManager.UpdateSlotUI(to);
    }

    public void SetActive(bool active)
    {
        if (highlightImage != null)
            highlightImage.enabled = active;
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage == null) return;

        iconImage.sprite = sprite;

        // Hide/show icon (alpha is more predictable than enabled/disabled)
        var c = iconImage.color;
        c.a = (sprite != null) ? 1f : 0f;
        iconImage.color = c;
    }
}