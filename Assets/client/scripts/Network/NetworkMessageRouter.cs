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
            case "spawn-player":
            {
                PlayerPacket p = JsonUtility.FromJson<PlayerPacket>(packet.data);
                PlayerManager.Instance.SpawnPlayer(p.id, new Vector3(p.x, p.y, p.z));
                break;
            }

            case "player-update":
            {
                PlayerPacket p = JsonUtility.FromJson<PlayerPacket>(packet.data);
                PlayerManager.Instance.UpdatePlayerPos(p.id, new Vector3(p.x, p.y, p.z), p.angle);
                break;
            }

            case "player-left":
            {
                PlayerPacket p = JsonUtility.FromJson<PlayerPacket>(packet.data);
                PlayerManager.Instance.RemovePlayer(p.id);
                break;
            }


    
            default:
                Debug.LogWarning("Unknown WS message type: " + packet.type);
                break;
        }
    }
}

