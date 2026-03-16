using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance;
    public SpellDatabase spellDatabase;
    
    private readonly Dictionary<string, GameObject> activeSpells = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================
    // SPAWN
    // =========================
    public void SpawnSpell(SpellSpawnPacket p)
    {
        if (activeSpells.ContainsKey(p.id))
            return;

        SpellPrototype spellProto = spellDatabase.Get(p.prefabName);
        if (spellProto == null)
        {
            Debug.LogError($"SpellDatabase has no spell with prefabName '{p.prefabName}'");
            return;
        }

        if (spellProto.prefab == null)
        {
            Debug.LogError($"SpellPrototype '{p.prefabName}' has no prefab assigned");
            return;
        }

        GameObject spell = Instantiate(
            spellProto.prefab,
            p.position,
            Quaternion.LookRotation(p.direction),
            transform // parent = SpellManager GameObject
        );
        
        spell.gameObject.GetComponent<SpellCollider>().id = p.id;
        spell.gameObject.GetComponent<SpellCollider>().casterId = PlayerManager.Instance.localPlayerId;

        activeSpells[p.id] = spell;
    }

    // =========================
    // UPDATE (server-authoritative)
    // =========================
    public void UpdateSpell(SpellUpdatePacket p)
    {
        if (!activeSpells.TryGetValue(p.id, out var spell))
            return;

        spell.transform.position = Vector3.Lerp(
            spell.transform.position,
            p.position,
            0.1f
        );
    }

    // =========================
    // DESPAWN
    // =========================
    public void DespawnSpell(SpellDespawnPacket p)
    {
        if (!activeSpells.TryGetValue(p.id, out var spell))
            return;

        Destroy(spell);
        activeSpells.Remove(p.id);
    }
}