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

            case "player-positionupdate":
            {
                PlayerStatePacket[] states =
                    JsonHelper.FromJson<PlayerStatePacket>(packet.data);

                foreach (var p in states)
                {
                    //  Never auto-spawn local player
                    if (p.id == PlayerManager.Instance.localPlayerId)
                        continue;

                    // ✅ Spawn remote player if missing
                    if (!PlayerManager.Instance.HasPlayer(p.id))
                    {
                        PlayerManager.Instance.SpawnPlayer(
                            p.id,
                            p.pos.ToUnity()
                        );
                    }

                    
                    PlayerManager.Instance.UpdatePlayerPos(
                        p.id,
                        p.pos.ToUnity(),
                        p.angle
                    );
                }

                break;
            }


            case "player-left":
            {
                PlayerPacket p = JsonUtility.FromJson<PlayerPacket>(packet.data);
                PlayerManager.Instance.RemovePlayer(p.id);
                break;
            }
            case "spell-spawn":
                SpellManager.Instance.SpawnSpell(
                    JsonUtility.FromJson<SpellSpawnPacket>(packet.data)
                );
                break;

            case "spell-update":
                SpellManager.Instance.UpdateSpell(
                    JsonUtility.FromJson<SpellUpdatePacket>(packet.data)
                );
                break;

            case "spell-despawn":
                SpellManager.Instance.DespawnSpell(
                    JsonUtility.FromJson<SpellDespawnPacket>(packet.data)
                );
                break;




    
            default:
                Debug.LogWarning("Unknown WS message type: " + packet.type);
                break;
        }
    }
}

