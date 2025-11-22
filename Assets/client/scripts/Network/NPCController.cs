using UnityEngine;

public class NPCController : MonoBehaviour
{
    public string npcId;

    private Vector3 targetPos;
    private float targetAngle;

    public void Init(string id)
    {
        npcId = id;
    }

    public void NetworkUpdate(NPCPacket data)
    {
        targetPos = new Vector3(data.position.x, data.position.y, data.position.z);
        targetAngle = data.angle;
    }

    void Update()
    {
        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, targetPos, 10f * Time.deltaTime);

        // Smooth rotation
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0, Mathf.Rad2Deg * targetAngle, 0),
            10f * Time.deltaTime
        );
    }

    public void TakeDamage(int amount)
    {
        // TODO: flash color, spawn UI damage text
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}