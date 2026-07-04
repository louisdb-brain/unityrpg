using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemQuickSlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    [Header("UI")]
    public Image iconImage;
    public Image highlightImage;
    public Text hotkeyLabel;

    [HideInInspector] public int quickSlotIndex;
    public QuickSlotManager manager;

    void Awake()
    {
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();

        if (highlightImage == null)
            highlightImage = transform.Find("Highlight")?.GetComponent<Image>();

        if (hotkeyLabel == null)
            hotkeyLabel = transform.Find("Label")?.GetComponent<Text>();
    }

    public void Init(int index, QuickSlotManager slotManager)
    {
        quickSlotIndex = index;
        manager = slotManager;

        if (hotkeyLabel != null)
        {
            hotkeyLabel.text = index switch
            {
                1 => "E",
                2 => "R",
                3 => "T",
                _ => ""
            };
        }
    }

    public void OnPointerClick(PointerEventData eventData) { }

    public void OnDrop(PointerEventData eventData)
    {
        if (manager == null || eventData.pointerDrag == null)
            return;

        var bagDrag = eventData.pointerDrag.GetComponent<InventoryDragItem>();
        if (bagDrag != null && bagDrag.FromSlotIndex >= 0)
        {
            manager.TryAssignFromBag(quickSlotIndex, bagDrag.FromSlotIndex);
            return;
        }

        var quickDrag = eventData.pointerDrag.GetComponent<QuickSlotDragItem>();
        if (quickDrag != null && quickDrag.FromQuickSlotIndex >= 0)
        {
            manager.TrySwapQuickSlots(quickDrag.FromQuickSlotIndex, quickSlotIndex);
        }
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage == null)
            return;

        iconImage.sprite = sprite;
        var c = iconImage.color;
        c.a = sprite != null ? 1f : 0f;
        iconImage.color = c;
    }

    public void SetSelected(bool selected)
    {
        if (highlightImage != null)
            highlightImage.enabled = selected;
    }
}
