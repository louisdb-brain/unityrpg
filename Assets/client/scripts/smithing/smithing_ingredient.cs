using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class smithing_ingredient : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public int temperature = 0;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        // Let the raycast pass through this ingredient
        canvasGroup.blocksRaycasts = false;

        // Place above the other UI objects while dragging
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // If it was not dropped onto something valid,
        // return it to its original slot.
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}