using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> allItems = new List<Item>();

    private Dictionary<string, Item> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in allItems)
        {
            if (item == null)
            {
                Debug.LogError("ItemDatabase contains a null Item reference", this);
                continue;
            }

            string key = item.Id;
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError($"Item '{item.name}' has no itemId", item);
                continue;
            }

            if (lookup.ContainsKey(key))
            {
                Debug.LogError(
                    $"Duplicate itemId '{key}' in ItemDatabase.",
                    item
                );
                continue;
            }

            lookup.Add(key, item);
        }
    }

    public Item GetById(string itemId)
    {
        if (lookup == null)
            BuildLookup();

        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogError("GetById called with null or empty itemId");
            return null;
        }

        if (!lookup.TryGetValue(itemId, out Item found))
        {
            Debug.LogWarning($"Item not found in database: '{itemId}'");
            return null;
        }

        return found;
    }
}
