using UnityEngine;
using System.Linq;

public class GrassBend : MonoBehaviour
{
    public float bendDistance = 0.7f;
    public float bendAmount = 20f; 
    public float returnSpeed = 2f;

    private Quaternion originalRot;

    void Start()
    {
        originalRot = transform.localRotation;
    }

    void LateUpdate()
    {
        Vector3? closestNpc = FindClosestNPC();

        // No NPCs => return to original
        if (closestNpc == null)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                originalRot,
                Time.deltaTime * returnSpeed
            );
            return;
        }

        float dist = Vector3.Distance(transform.position, closestNpc.Value);

        if (dist < bendDistance)
        {
            Vector3 away = (transform.position - closestNpc.Value).normalized;
            Quaternion bendRot = Quaternion.LookRotation(away);

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                bendRot,
                Time.deltaTime * 10f
            );
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                originalRot,
                Time.deltaTime * returnSpeed
            );
        }
    }

    Vector3? FindClosestNPC()
    {
        if (NPCManager.Instance == null || NPCManager.Instance.AllNPCs.Count == 0)
            return null;

        Vector3 pos = transform.position;

        return NPCManager.Instance.AllNPCs
            .Select(npc => npc.transform.position)
            .OrderBy(p => Vector3.Distance(pos, p))
            .First();
    }
}