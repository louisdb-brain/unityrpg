using UnityEngine;

public abstract class Item : ScriptableObject
{
    [Header("Common Item Data")]
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab; // optional: what to spawn in the world
    public string description;

    // Optional: a generic "use" behavior
    public virtual void Inspect(GameObject user)
    {
        Debug.Log(description);
    }

    public virtual void onUse(GameObject user)
    {
        Debug.Log("used item");
    }


    public virtual void onLoot(GameObject user)
    {
        
    }
}


