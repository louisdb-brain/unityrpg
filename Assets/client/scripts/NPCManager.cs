using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;
    public GameObject npcPrefab;
    public GameObject bloodPrefab;

    private Dictionary<string, NPCController> npcs = new();
    public List<Transform> AllNPCs = new List<Transform>();
    
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
                AllNPCs.Add(obj.transform);
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

        if (!npcs.TryGetValue(dmg.id, out var npc))
            return;

        npc.TakeDamage(dmg.amount);

        if (DamagePopupSpawner.Instance != null)
        {
            DamagePopupSpawner.Instance.Spawn(
                npc.transform.position,
                dmg.amount
            );
        }
    }



    // ----------------------------------
    // NPC KILL
    // ----------------------------------
    public void OnNPCKill(string json)
    {
        var dead = JsonUtility.FromJson<NPCKillPacket>(json);

        if (!npcs.TryGetValue(dead.id, out var npc))
            return;

        Vector3 deathPos = npc.transform.position;

        // Spawn blood effect
        if (bloodPrefab != null)
        {
            GameObject blood = Instantiate(
                bloodPrefab,
                deathPos,
                Quaternion.identity
            );

            Destroy(blood, 3f);
        }

        // Remove from lists
        AllNPCs.Remove(npc.transform);
        npcs.Remove(dead.id);

        // Destroy NPC GameObject
        Destroy(npc.gameObject);
    }

}