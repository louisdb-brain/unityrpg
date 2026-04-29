using UnityEngine;

[CreateAssetMenu(fileName = "newsmithing", menuName = "Game/Items/recipe")]
public class Recipe : ScriptableObject
{
    [Header("Visuals")]
    public string recipeName;
    public Sprite icon;

    [Header("Recipe Ingredients")]
    public Item[] ingredients;

    [Header("Recipe Result")]
    public Item result;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (result == null)
        {
            icon = null;
            return;
        }

        icon = result.icon;
    }
#endif
}