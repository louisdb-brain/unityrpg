using UnityEngine;
using UnityEngine.UI;

public class SmithingUi : MonoBehaviour
{
    public static SmithingUi Instance { get; private set; }

    public GameObject smithingCanvas;
    public GameObject ingredientPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CreateIngredient(Item ingredientItem)
    {
        if (ingredientItem == null)
        {
            Debug.LogWarning("[SmithingUi] Ingredient item is null.");
            return;
        }

        if (ingredientPrefab == null)
        {
            Debug.LogError("[SmithingUi] Ingredient prefab is not assigned.");
            return;
        }

        if (smithingCanvas == null)
        {
            Debug.LogError("[SmithingUi] Smithing canvas is not assigned.");
            return;
        }

        GameObject ingredient = Instantiate(
            ingredientPrefab,
            smithingCanvas.transform
        );

        Image image = ingredient.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError("[SmithingUi] Ingredient prefab has no Image component.");
            Destroy(ingredient);
            return;
        }

        image.sprite = ingredientItem.icon;
    }
}