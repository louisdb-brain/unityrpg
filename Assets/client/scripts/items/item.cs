using UnityEngine;

public abstract class item : ScriptableObject
{
    [Header("Common Item Data")]
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab; // optional: what to spawn in the world
    public string description;

    // Optional: a generic "use" behavior
    public  void inspect(GameObject user)
    {
        Debug.Log(description.ToString());
    }

    public virtual void onLoot(GameObject user)
    {
        
    }
}


