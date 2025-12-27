using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<item> allItems = new List<item>();

    // ✅ Fast lookup by name (for JSON)
    private Dictionary<string, item> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, item>();

        foreach (var i in allItems)
        {
            if (!lookup.ContainsKey(i.name))
                lookup.Add(i.name, i);
        }
    }

    public item GetByName(string itemName)
    {
        if (lookup == null)
            BuildLookup();

        lookup.TryGetValue(itemName, out item found);
        return found;
    }
}