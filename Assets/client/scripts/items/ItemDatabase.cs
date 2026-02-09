using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> allItems = new List<Item>();

    // Fast lookup by name (JSON / server safe)
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

            if (lookup.ContainsKey(item.name))
            {
                Debug.LogError(
                    $"Duplicate item name '{item.name}' in ItemDatabase. Names must be unique.",
                    item
                );
                continue;
            }

            lookup.Add(item.name, item);
        }
    }

    public Item GetByName(string itemName)
    {
        if (lookup == null)
            BuildLookup();

        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogError("GetByName called with null or empty itemName");
            return null;
        }

        if (!lookup.TryGetValue(itemName, out Item found))
        {
            Debug.LogWarning($"Item not found in database: '{itemName}'");
            return null;
        }

        return found;
    }
}