using UnityEngine;

public static class NetworkMessageRouter
{
    public static void Handle(string json)
    {
        
        Debug.Log("RAW JSON: " + json);

        var packet = JsonUtility.FromJson<ServerPacket>(json);
        Debug.Log("PACKET IS NULL? " + (packet == null));

        if (packet != null)
            Debug.Log("PACKET TYPE: " + packet.type);

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
            case "socket-id":
            {
                if (PlayerManager.Instance == null)
                {
                    Debug.LogError("PlayerManager not initialized yet!");
                    return;
                }

                
                var p = JsonUtility.FromJson<PlayerPacket>(packet.data);
                PlayerManager.Instance.localPlayerId = p.id;
                Debug.Log("socket-id received, create-player sent");
                NetworkClient.Instance.Send("create-player", new EmptyData());
                break;
            }
            case "spawn-player":
            {
                Debug.Log(" player spawn received");
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

