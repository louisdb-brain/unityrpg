using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

public class LootSpawner : MonoBehaviour
{
    [Header("Loot Definition")]
    public string lootId;     // MUST be unique across the whole world
       // Must exist in ItemDatabase
    
    [Header("Optional")]
    public int level = 1;

    private bool sent = false;

    public void spawnLoot(Item item)
    {
        // Prevent accidental double-sends
        if (sent) return;
        sent = true;
        Vector3 randomOffset = Random.insideUnitSphere * 2f;
        randomOffset.y = 0f;
        LootSpawnRequest data = new LootSpawnRequest
        {
            id = lootId+Guid.NewGuid().ToString(),
            itemName = item.itemName,
            position = transform.position + randomOffset ,
            level = level
        };

        NetworkClient.Instance.Send("loot-spawn-request", data);
        Destroy(this.gameObject);
    }
}

[System.Serializable]
public class LootSpawnRequest
{
    public string id;
    public string itemName;
    public Vector3 position;
    public int level;
    
}