using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Spell Database")]
public class SpellDatabase : ScriptableObject
{
    public List<SpellPrototype> spells;

    private Dictionary<string, SpellPrototype> lookup;

    public SpellPrototype Get(string prefabName)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, SpellPrototype>();
            foreach (var s in spells)
                lookup[s.prefabName] = s;   
        }

        return lookup.TryGetValue(prefabName, out var spell)
            ? spell
            : null;
    }
}