using UnityEngine;
using UnityEngine.EventSystems;

public class StovePanel : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        // Get the cookingItem script from the dragged UI element
        cookingItem item = eventData.pointerDrag.GetComponent<cookingItem>();

        if (item != null)
        {
            item.StartCooking(); // ✅ THIS sets it to "isCooking" via your enum
        }
    }
}