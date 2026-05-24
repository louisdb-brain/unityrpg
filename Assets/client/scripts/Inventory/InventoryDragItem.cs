using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int FromSlotIndex { get; private set; } = -1;

    private Canvas rootCanvas;
    private InventorySlotUI fromSlot;

    private GameObject ghostGO;
    private RectTransform ghostRT;
    private Image ghostImage;

    void Awake()
    {
        fromSlot = GetComponentInParent<InventorySlotUI>();

        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            rootCanvas = canvas.rootCanvas; // ✅ FIX

        if (fromSlot == null)
            Debug.LogError("[InventoryDragItem] No InventorySlotUI found in parents.");
        if (rootCanvas == null)
            Debug.LogError("[InventoryDragItem] No Canvas found in parents.");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (fromSlot == null || fromSlot.inventoryManager == null)
            return;

        if (fromSlot.iconImage == null || fromSlot.iconImage.sprite == null)
            return;

        FromSlotIndex = fromSlot.slotIndex;

        CreateGhost(fromSlot.iconImage.sprite);

        eventData.pointerDrag = gameObject; // ✅ FIX

        SetRealIconAlpha(0f);
        fromSlot.inventoryManager.SetDropBackdropActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostRT == null) return;

        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            ghostRT.position = eventData.position;
        }
        else
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rootCanvas.transform as RectTransform,
                eventData.position,
                rootCanvas.worldCamera,
                out Vector3 worldPos
            );
            ghostRT.position = worldPos;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyGhost();

        if (fromSlot != null && fromSlot.inventoryManager != null)
        {
            fromSlot.inventoryManager.SetDropBackdropActive(false);
            fromSlot.inventoryManager.UpdateSlotUI(fromSlot.slotIndex);
        }

        FromSlotIndex = -1;
    }

    private void CreateGhost(Sprite sprite)
    {
        ghostGO = new GameObject("InventoryDragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        ghostGO.transform.SetParent(rootCanvas.transform, false);

        ghostRT = ghostGO.GetComponent<RectTransform>();
        ghostImage = ghostGO.GetComponent<Image>();

        ghostImage.sprite = sprite;
        ghostImage.preserveAspect = true;

        // Make ghost not interact with raycasts
        var cg = ghostGO.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
        cg.alpha = 0.9f;

        // size: match icon rect size
        var srcRT = fromSlot.iconImage.rectTransform;
        ghostRT.sizeDelta = srcRT.rect.size;
        ghostRT.position = Input.mousePosition;
    }

    private void DestroyGhost()
    {
        if (ghostGO != null)
            Destroy(ghostGO);

        ghostGO = null;
        ghostRT = null;
        ghostImage = null;
    }

    private void SetRealIconAlpha(float a)
    {
        if (fromSlot == null || fromSlot.iconImage == null) return;

        var c = fromSlot.iconImage.color;
        c.a = a;
        fromSlot.iconImage.color = c;
    }
}
