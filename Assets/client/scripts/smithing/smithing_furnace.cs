using UnityEngine;
using UnityEngine.EventSystems;

public class smithing_furnace : MonoBehaviour, IDropHandler
{
    public Transform ingredientSnapPoint;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        smithing_ingredient ingredient =
            eventData.pointerDrag.GetComponent<smithing_ingredient>();

        if (ingredient == null)
            return;

        Transform snapTarget =
            ingredientSnapPoint != null
                ? ingredientSnapPoint
                : transform;

        RectTransform ingredientRect =
            ingredient.GetComponent<RectTransform>();

        ingredient.transform.SetParent(snapTarget);
        ingredient.transform.SetAsLastSibling();

        ingredientRect.anchoredPosition = Vector2.zero;
        ingredientRect.localScale = Vector3.one;

        Debug.Log($"Dropped {ingredient.name} into furnace.");
    }
}