using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> allItems = new List<Item>();

    // ✅ Fast lookup by name (for JSON)
    private Dictionary<string, Item> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, Item>();

        foreach (var i in allItems)
        {
            if (!lookup.ContainsKey(i.name))
                lookup.Add(i.name, i);
        }
    }

    public Item GetByName(string itemName)
    {
        if (lookup == null)
            BuildLookup();

        lookup.TryGetValue(itemName, out Item found);
        return found;
    }
}