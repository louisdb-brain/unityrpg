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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnLoot(LootSpawnPacket data)
    {
        if (activeLoot.ContainsKey(data.id))
            return;

        Item itemData = itemDatabase.GetById(data.itemId);
        if (itemData == null)
        {
            Debug.LogWarning($"no item in itemdatabase {data.itemId}");
            return;
        }

        if (itemData.worldPrefab == null)
        {
            Debug.LogWarning($"No prefab for item - {data.itemId}");
            return;
        }

        Debug.Log(data.itemId);
        GameObject go = Instantiate(
            itemData.worldPrefab,
            data.position.ToUnity(),
            Quaternion.identity
        );

        LootWorldObject lootObj = go.GetComponent<LootWorldObject>();
        lootObj.Init(data.id, data.itemId, itemData.icon);

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
