using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Game/Items/Ingredient")]
public class ingredient : Item
{
    [Header("Cooking Data")]
    public float cookingTime;
    public float burnTime;
    
}