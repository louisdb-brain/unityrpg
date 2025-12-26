using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public item currentItem;

    void Awake()
    {
        ClearSlot();
    }

    public void SetItem(item newItem)
    {
        currentItem = newItem;
        icon.sprite = newItem.icon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        currentItem = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            currentItem.inspect(gameObject);   // uses YOUR inspect()
            ClearSlot();
        }
    }
}