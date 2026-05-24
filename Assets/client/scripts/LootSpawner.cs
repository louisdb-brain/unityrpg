using UnityEngine;
using Random = UnityEngine.Random;
using System;

public class LootSpawner : MonoBehaviour
{
    [Header("Loot Definition")]
    public string lootId;

    [Header("Optional")]
    public int level = 1;

    private bool sent = false;

    public void spawnLoot(Item item)
    {
        if (sent || item == null)
            return;

        sent = true;
        Vector3 randomOffset = Random.insideUnitSphere * 2f;
        randomOffset.y = 0f;
        LootSpawnRequest data = new LootSpawnRequest
        {
            id = lootId + Guid.NewGuid().ToString(),
            itemId = item.Id,
            position = transform.position + randomOffset,
            level = level
        };

        Debug.Log($"Sending loot-spawn-request: {data.itemId} id={data.id}");
        NetworkClient.Instance.Send("loot-spawn-request", data);
        Destroy(gameObject);
    }
}

[System.Serializable]
public class LootSpawnRequest
{
    public string id;
    public string itemId;
    public Vector3 position;
    public int level;
}
