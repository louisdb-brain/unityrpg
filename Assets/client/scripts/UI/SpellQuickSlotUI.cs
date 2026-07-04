using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellQuickSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Image highlightImage;
    public Text labelText;

    [HideInInspector] public int slotIndex;
    public QuickSlotManager manager;

    void Awake()
    {
        if (highlightImage == null)
            highlightImage = transform.Find("Highlight")?.GetComponent<Image>();

        if (labelText == null)
            labelText = transform.Find("Label")?.GetComponent<Text>();
    }

    public void Init(int index, QuickSlotManager slotManager)
    {
        slotIndex = index;
        manager = slotManager;

        if (labelText != null)
            labelText.text = (index + 1).ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        manager?.CycleSpellSlot(slotIndex);
    }

    public void SetSpell(SpellPrototype spell)
    {
        if (labelText == null)
            return;

        labelText.text = spell != null ? spell.prefabName : (slotIndex + 1).ToString();
    }

    public void SetSelected(bool selected)
    {
        if (highlightImage != null)
            highlightImage.enabled = selected;
    }
}
