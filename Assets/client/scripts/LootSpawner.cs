using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    [Header("Loot Definition")]
    public string lootId;     // MUST be unique across the whole world
    public string itemName;   // Must exist in ItemDatabase

    [Header("Optional")]
    public int level = 1;

    private bool sent = false;

    void Start()
    {
        // Prevent accidental double-sends
        if (sent) return;
        sent = true;

        LootSpawnRequest data = new LootSpawnRequest
        {
            id = lootId,
            itemName = itemName,
            position = transform.position,
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