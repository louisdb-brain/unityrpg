using UnityEngine;

[CreateAssetMenu(fileName = "newfood", menuName = "Game/Items/Food")]
public class food : item
{
    [Header("Cooking Data")] 
    public bool cookable;
    public float cookingTime;
    public float burnTime;
    public float health;

}