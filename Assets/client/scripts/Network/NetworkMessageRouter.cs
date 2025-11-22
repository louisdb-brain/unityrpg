using UnityEngine;

public static class NetworkMessageRouter
{
    public static void Handle(string json)
    {
        var packet = JsonUtility.FromJson<ServerPacket>(json);

        switch (packet.type)
        {
            case "npc-position-update":
                NPCManager.Instance.OnNPCUpdate(packet.data);
                break;

            case "npc-takedamage":
                NPCManager.Instance.OnNPCDamage(packet.data);
                break;

            case "npc-kill":
                NPCManager.Instance.OnNPCKill(packet.data);
                break;

            default:
                Debug.LogWarning("Unknown WS message type: " + packet.type);
                break;
        }
    }
}

