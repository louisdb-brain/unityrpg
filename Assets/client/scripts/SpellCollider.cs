using UnityEngine;

public class SpellCollider : MonoBehaviour
{
    public string casterId;
    public string id;

    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        //if (hasHit) return;

        Debug.Log($"OnTriggerEnter {casterId} {PlayerManager.Instance.localPlayerId} {id} {other.gameObject.name}");

        if (casterId != PlayerManager.Instance.localPlayerId)
        {
            Debug.Log("wrong caster");
            return;
        }

        if (!other.CompareTag("NPC"))
        {
            Debug.Log("tag mismatch " + other.tag);
            return;
        }

        NPCController npc = other.GetComponent<NPCController>();
        if (npc == null)
        {
            Debug.LogError("NPC tag found but NPCController missing on " + other.name);
            return;
        }

        hasHit = true;

        Debug.Log("hit");
        Debug.Log("Spell " + id + " hit NPC: " + other.name + " from caster: " + casterId);

        NetworkClient.Instance.Send("collision-spell-npc", new SpellCollisionPacket
        {
            SpellId = id,
            TargetId = npc.npcId
        });
    }
}