using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Spell Prototype")]
public class SpellPrototype : ScriptableObject
{
    public string prefabName;     // 🔑 single source of identity

    public GameObject prefab;

    public float speed = 8f;
    public float radius = 2f;
    public int damage = 5;
    public float lifetime = 1.5f;
}