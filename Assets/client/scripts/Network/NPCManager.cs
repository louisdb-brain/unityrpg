using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;
    public GameObject npcPrefab;

    private Dictionary<string, NPCController> npcs = new();

    void Awake() => Instance = this;

    // ----------------------------------
    // NPC POSITION UPDATES
    // ----------------------------------
    public void OnNPCUpdate(string json)
    {
        NPCPacket[] list = JsonHelper.FromJson<NPCPacket>(json);

        foreach (var npc in list)
        {
            if (!npcs.ContainsKey(npc.id))
            {
                // Spawn capsule NPC
                GameObject obj = Instantiate(npcPrefab);
                NPCController ctrl = obj.GetComponent<NPCController>();

                ctrl.Init(npc.id,npc.npcType,npc.level);

                npcs[npc.id] = ctrl;
            }

            // Update position & rotation
            npcs[npc.id].NetworkUpdate(npc);
        }
    }

    // ----------------------------------
    // DAMAGE
    // ----------------------------------
    public void OnNPCDamage(string json)
    {
        var dmg = JsonUtility.FromJson<NPCDamagePacket>(json);

        if (npcs.TryGetValue(dmg.id, out var npc))
        {
            npc.TakeDamage(dmg.amount);
        }
    }

    // ----------------------------------
    // NPC KILL
    // ----------------------------------
    public void OnNPCKill(string json)
    {
        var dead = JsonUtility.FromJson<NPCKillPacket>(json);

        if (npcs.TryGetValue(dead.id, out var npc))
        {
            npc.Die();
            npcs.Remove(dead.id);
        }
    }
}