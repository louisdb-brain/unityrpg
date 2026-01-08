using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Image iconImage;
    public Image highlightImage;

    [HideInInspector] public int slotIndex;
    private InventoryManager inventoryManager;

   public void Init(int index, InventoryManager manager)
    {
        slotIndex = index;
        inventoryManager = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryManager.OnSlotClicked(slotIndex);
    }

    public void SetActive(bool active)
    {
        highlightImage.enabled = active;
    }

    public void SetIcon(Sprite sprite)
    {
        iconImage.enabled = sprite != null;
        iconImage.sprite = sprite;
    }
}