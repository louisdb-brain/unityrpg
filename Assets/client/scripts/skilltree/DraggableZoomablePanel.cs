using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DraggableZoomablePanelInMask : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
{
    [Header("Scale limits relative to original size")]
    [Range(0.1f, 10f)] public float minScale = 0.5f;
    [Range(0.1f, 10f)] public float maxScale = 2.0f;

    [Header("Max distance from original position (multiples of original sizeDelta)")]
    [Range(0.1f, 10f)] public float maxDragMultiple = 2.0f;

    [Header("Scroll zoom speed")]
    [Range(0.01f, 5f)] public float zoomSpeed = 0.15f;

    private RectTransform panelRect;
    private RectTransform maskRect; // parent mask rect
    private Canvas rootCanvas;

    private Vector2 originalAnchoredPosition;
    private Vector2 originalSizeDelta;
    private float currentScale = 1f;

    private bool isDragging;
    private Vector2 pointerOffsetLocal;

    private void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        maskRect = panelRect.parent as RectTransform;
        rootCanvas = GetComponentInParent<Canvas>();

        if (maskRect == null)
        {
            Debug.LogError("This panel needs a RectTransform parent (your Mask).");
            return;
        }

        if (rootCanvas == null)
        {
            Debug.LogError("This panel needs to be under a Canvas.");
            return;
        }

        originalAnchoredPosition = panelRect.anchoredPosition;
        originalSizeDelta = panelRect.sizeDelta;

        currentScale = 1f;
        ApplyScaleAndClamp();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        isDragging = true;

        // Offset in the coordinate space of the MASK (direct parent)
        Vector2 localPointerPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                maskRect, eventData.position, eventData.pressEventCamera, out localPointerPos))
        {
            pointerOffsetLocal = Vector2.zero;
            return;
        }

        pointerOffsetLocal = panelRect.anchoredPosition - localPointerPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 localPointerPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                maskRect, eventData.position, eventData.pressEventCamera, out localPointerPos))
        {
            return;
        }

        Vector2 desired = localPointerPos + pointerOffsetLocal;
        panelRect.anchoredPosition = ClampToRules(desired);
    }

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;
        float delta = scroll * zoomSpeed;

        float newScale = currentScale * (1f + delta);
        currentScale = Mathf.Clamp(newScale, minScale, maxScale);

        ApplyScaleAndClamp();
    }

    private void ApplyScaleAndClamp()
    {
        panelRect.sizeDelta = originalSizeDelta * currentScale;
        panelRect.anchoredPosition = ClampToRules(panelRect.anchoredPosition);
    }

    private Vector2 ClampToRules(Vector2 desiredAnchoredPos)
    {
        // Rule A: clamp relative to original position by multiples of original size
        float maxXFromOriginal = originalSizeDelta.x * maxDragMultiple;
        float maxYFromOriginal = originalSizeDelta.y * maxDragMultiple;

        float x = Mathf.Clamp(desiredAnchoredPos.x,
            originalAnchoredPosition.x - maxXFromOriginal,
            originalAnchoredPosition.x + maxXFromOriginal);

        float y = Mathf.Clamp(desiredAnchoredPos.y,
            originalAnchoredPosition.y - maxYFromOriginal,
            originalAnchoredPosition.y + maxYFromOriginal);

        Vector2 pos = new Vector2(x, y);

        // Rule B: clamp so the panel stays within the mask bounds as much as possible.
        // This assumes both panel and mask have centered pivots (0.5, 0.5).
        // If your pivots differ, tell me and I will adapt it.

        Vector2 maskSize = maskRect.rect.size;
        Vector2 panelSize = panelRect.rect.size;

        float halfMaskW = maskSize.x * 0.5f;
        float halfMaskH = maskSize.y * 0.5f;

        float halfPanelW = panelSize.x * 0.5f;
        float halfPanelH = panelSize.y * 0.5f;

        // If the panel is larger than the mask, allow panning, but clamp edges.
        // If the panel is smaller than the mask, keep it from drifting outside.
        float minX = -halfMaskW + halfPanelW;
        float maxX =  halfMaskW - halfPanelW;

        float minY = -halfMaskH + halfPanelH;
        float maxY =  halfMaskH - halfPanelH;

        // If panel bigger than mask, minX may be > maxX; swap behavior to allow movement.
        if (minX > maxX)
        {
            // Panel wider than mask: allow movement but keep edges covering mask
            float edgeMinX = -halfMaskW + halfPanelW; // positive
            float edgeMaxX =  halfMaskW - halfPanelW; // negative
            pos.x = Mathf.Clamp(pos.x, edgeMaxX, edgeMinX);
        }
        else
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
        }

        if (minY > maxY)
        {
            float edgeMinY = -halfMaskH + halfPanelH;
            float edgeMaxY =  halfMaskH - halfPanelH;
            pos.y = Mathf.Clamp(pos.y, edgeMaxY, edgeMinY);
        }
        else
        {
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }

        return pos;
    }
    private void OnMouseDown()
    {
        Debug.Log("OnMouseDown fired");
    }
}