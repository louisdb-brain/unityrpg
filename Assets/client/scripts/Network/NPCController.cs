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
        // Update target position
        targetPosition = new Vector3(
            data.position.x,
            data.position.y,
            data.position.z
        );

        // Update type & level
        npcType = data.npcType;
        level = data.level;

        // Update health
        health = data.health;

        // damage flash
        if (data.health < maxHealth)
            hitFlashTimer = damageFlashDuration;
    }


    void Update()
    {
        float delta = Time.deltaTime;

        // --- 1) Move smoothly toward target ---
        Vector3 direction = targetPosition - transform.position;
        float distance = direction.magnitude;

        if (distance > 0.05f)
        {
            Vector3 step = direction.normalized * speed * delta;
            transform.position += step;

            // Rotate
            if (step != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(step);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * delta);
            }

            if (anim) anim.SetBool("Moving", true);
        }
        else
        {
            if (anim) anim.SetBool("Moving", false);
        }

        // --- 2) Damage flash ---
        if (hitFlashTimer > 0)
        {
            hitFlashTimer -= delta;

            // Flash visual (optional)
            // Example: change material color
            // meshRenderer.material.color = Color.red;
        }

        // --- 3) Health bar ---
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
