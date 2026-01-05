using UnityEngine;

public class Loot : MonoBehaviour
{
    public Item itemData;
    public int amount = 1;

    [Header("Pickup Settings")]
    public float magnetSpeed = 12f;
    public float pickupDistance = 0.6f;

    [Header("Audio")]
    public AudioClip pickupSound;

    private Transform player;
    private InventoryManager inventory;
    private bool isBeingPulled = false;

    void Update()
    {
        if (!isBeingPulled || player == null)
            return;

        // ✅ Move loot smoothly toward player
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            magnetSpeed * Time.deltaTime
        );

        // ✅ If close enough → pick up
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < pickupDistance)
        {
            Pickup();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        inventory = other.GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        // ✅ Start magnet pull instead of instant pickup
        player = other.transform;
        isBeingPulled = true;
    }

    void Pickup()
    {
        bool added = inventory.AddItem(itemData);

        if (!added)
            return; // inventory full → do nothing

        // ✅ Play pickup sound
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // ✅ Run item logic
        itemData.onLoot(player.gameObject);

        // ✅ Destroy loot object
        Destroy(gameObject);
    }
}