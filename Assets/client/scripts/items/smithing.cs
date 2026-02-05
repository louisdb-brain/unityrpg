using UnityEngine;

[CreateAssetMenu(fileName = "newsmithing", menuName = "Game/Items/smithing")]
public class smithing : Item
{
    [Header("Cooking Data")] 
    public bool cookable;
    public float cookingTime;
    public float burnTime;
    public float health;

}