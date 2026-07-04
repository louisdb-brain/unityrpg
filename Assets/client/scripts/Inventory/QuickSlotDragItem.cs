using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlotDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int FromQuickSlotIndex { get; private set; } = -1;

    private Canvas rootCanvas;
    private ItemQuickSlotUI fromSlot;

    private GameObject ghostGO;
    private RectTransform ghostRT;
    private Image ghostImage;

    void Awake()
    {
        fromSlot = GetComponentInParent<ItemQuickSlotUI>();

        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            rootCanvas = canvas.rootCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (fromSlot == null || QuickSlotManager.Instance == null)
            return;

        if (fromSlot.iconImage == null || fromSlot.iconImage.sprite == null)
            return;

        FromQuickSlotIndex = fromSlot.quickSlotIndex;
        CreateGhost(fromSlot.iconImage.sprite);
        eventData.pointerDrag = gameObject;
        SetRealIconAlpha(0f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostRT == null)
            return;

        ghostRT.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyGhost();

        if (fromSlot != null)
        {
            QuickSlotManager.Instance?.RefreshItemSlot(fromSlot.quickSlotIndex);
            SetRealIconAlpha(1f);
        }

        FromQuickSlotIndex = -1;
    }

    private void CreateGhost(Sprite sprite)
    {
        ghostGO = new GameObject("QuickSlotDragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        ghostGO.transform.SetParent(rootCanvas.transform, false);

        ghostRT = ghostGO.GetComponent<RectTransform>();
        ghostImage = ghostGO.GetComponent<Image>();
        ghostImage.sprite = sprite;
        ghostImage.preserveAspect = true;

        var cg = ghostGO.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
        cg.alpha = 0.9f;

        ghostRT.sizeDelta = fromSlot.iconImage.rectTransform.rect.size;
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
        if (fromSlot == null || fromSlot.iconImage == null)
            return;

        var c = fromSlot.iconImage.color;
        c.a = a;
        fromSlot.iconImage.color = c;
    }
}
