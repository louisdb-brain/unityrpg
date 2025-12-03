using UnityEngine;

public class NPCController : MonoBehaviour
{
    // === Metadata ===
    public string npcId;
    public string npcType;
    public int level;

    // === State ===
    public float maxHealth = 100f;
    public float health = 100f;

    public Vector3 targetPosition;
    public float speed = 2f;

    // === Damage flash ===
    private float hitFlashTimer = 0f;
    public float damageFlashDuration = 0.25f;

    // === Healthbar ===
    public Transform healthbarRoot;
    public Transform healthbarFill;

    // === Animation (optional) ===
    public Animator anim;
    
    private Vector3 prevPos;
    private Vector3 nextPos;
    private float lerpTimer = 0f;
    private float lerpDuration = 0.1f; // match server tick rate


    void Start()
    {
        targetPosition = transform.position;
    }

    public void Init(string id, string type, int lvl)
    {
        npcId = id;
        npcType = type;
        level = lvl;
    }

    // Called by NPCManager when network packet arrives
    public void NetworkUpdate(NPCPacket data)
    {
        // Store old -> new positions
        prevPos = transform.position;
        nextPos = new Vector3(data.position.x, data.position.y, data.position.z);

        // Reset interpolation
        lerpTimer = 0f;

        // Type, level, health
        npcType = data.npcType;
        level = data.level;
        health = data.health;
    }



    void Update()
    {
        float delta = Time.deltaTime;

        // Interpolation timer
        lerpTimer += delta;
        float t = Mathf.Clamp01(lerpTimer / lerpDuration);

        // Interpolate position
        transform.position = Vector3.Lerp(prevPos, nextPos, t);

        // Rotate toward movement direction (optional)
        Vector3 direction = nextPos - prevPos;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
        }

        UpdateHealthbar();
    }


    void UpdateHealthbar()
    {
        if (!healthbarFill) return;

        float pct = Mathf.Clamp01(health / maxHealth);

        healthbarFill.localScale = new Vector3(pct, 1, 1);

        // Face camera
        if (Camera.main)
        {
            healthbarRoot.transform.LookAt(Camera.main.transform);
        }
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(0, health - amount);
        hitFlashTimer = damageFlashDuration;
        UpdateHealthbar();
    }

    public void Die()
    {
        // Play dissolve / particle / etc
        Destroy(gameObject);
    }
}
