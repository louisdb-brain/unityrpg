using System.Collections.Generic;
using UnityEngine;

public class ClientLootManager : MonoBehaviour
{
    public static ClientLootManager Instance;
    public ItemDatabase itemDatabase;

    private Dictionary<string, GameObject> activeLoot
        = new Dictionary<string, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void SpawnLoot(LootSpawnPacket data)
    {
        if (activeLoot.ContainsKey(data.id))
            return;

        Item itemData = itemDatabase.GetByName(data.itemName);
        if (itemData == null || itemData.worldPrefab == null)
        {
            Debug.LogWarning($"No prefab for item {data.itemName}");
            return;
        }

        GameObject go = Instantiate(
            itemData.worldPrefab,
            data.position,
            Quaternion.identity
        );


        LootWorldObject lootObj = go.GetComponent<LootWorldObject>();
        lootObj.Init(data.id, data.itemName);

        activeLoot[data.id] = go;
    }

    public void RemoveLoot(string id)
    {
        if (!activeLoot.TryGetValue(id, out var go))
            return;

        Destroy(go);
        activeLoot.Remove(id);
    }
}