using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragItem : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    // ✅ When you start dragging
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.8f;          // Slight transparency
        canvasGroup.blocksRaycasts = false; // So it doesn't block drop detection
        
        // ✅ Get cookingItem on this UI element
        cookingItem item = GetComponent<cookingItem>();

        if (item != null)
        {
            item.StopCooking(); // ✅ stops cooking if it was cooking
            item.PlayPop();     // ✅ plays the pop sound
        }
    }

    // ✅ While dragging (follows mouse)
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += 
            eventData.delta / canvas.scaleFactor;
    }

    // ✅ When you release the mouse
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // ✅ It stays wherever you dropped it
    }
}

