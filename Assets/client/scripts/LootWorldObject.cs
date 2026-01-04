using UnityEngine;

public class LootWorldObject : MonoBehaviour
{
    [Header("Runtime Data")]
    public string lootId;
    public string itemName;

    [Header("Bob Animation")]
    public float bobHeight = 0.25f;
    public float bobSpeed = 2f;
    public float rotateSpeed = 45f;

    private Vector3 startPos;

    void Awake()
    {
        startPos = transform.position;
    }

    public void Init(string id, string name)
    {
        lootId = id;
        itemName = name;
    }

    void Update()
    {
        // Vertical bob
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + Vector3.up * yOffset;

        // Slow rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrEmpty(lootId))
            return;

        NetworkClient.Instance.Send("loot-pickup", new LootPickupRequest
        {
            id = lootId
        });
    }
}

[System.Serializable]
public class LootPickupRequest
{
    public string id;
}