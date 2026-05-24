using UnityEngine;

public abstract class Item : ScriptableObject
{
    [Header("Common Item Data")]
    [Tooltip("Display name shown in UI (e.g. copper ore). Not used on the network.")]
    public string itemName;

    [Tooltip("Stable id matching this asset filename (e.g. ore_copper). Used on the network.")]
    public string itemId;
    public Sprite icon;
    public GameObject worldPrefab;
    public string description;

    public string Id => itemId;
    public string DisplayName => itemName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(name) && itemId != name)
            itemId = name;
    }
#endif

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
