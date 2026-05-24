using UnityEngine;

public static class NetworkMessageRouter
{
    public static void Handle(string json)
    {
        
        //Debug.Log("RAW JSON: " + json);

        var packet = JsonUtility.FromJson<ServerPacket>(json);
        //Debug.Log("PACKET IS NULL? " + (packet == null));

        if (packet != null)
           // Debug.Log("PACKET TYPE: " + packet.type);

        switch (packet.type)
        {
            case "DEBUG-npcstate":
                
                NPCManager.Instance.DEBUGNpcState(packet.data);
                break;
            case "npc-position-update":
                NPCManager.Instance.OnNPCUpdate(packet.data);
                break;

            case "npc-takedamage":
                NPCManager.Instance.OnNPCDamage(packet.data);
                break;
            case "player-takedamage":
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
            case "loot-spawn":
            {
                if (ClientLootManager.Instance == null)
                {
                    Debug.LogError("ClientLootManager not ready — loot-spawn dropped");
                    return;
                }
                Debug.Log("dropping loot");
            

                var data = JsonUtility.FromJson<LootSpawnPacket>(packet.data);
                ClientLootManager.Instance.SpawnLoot(data);
                Debug.Log(data.itemId + data.id);
                break;
            }

            case "loot-picked":
            {
                Debug.Log("picking loot");
                if (ClientLootManager.Instance == null)
                {
                    Debug.LogError("ClientLootManager not ready — loot-picked dropped");
                    return;
                }

                var data = JsonUtility.FromJson<LootPickedPacket>(packet.data);
                ClientLootManager.Instance.RemoveLoot(data.id);
                break;
            }
            case "emit-inventory":
            {
                if (InventoryManager.Instance == null)
                {
                    Debug.LogError("inventorymanager does not exist yet, emit inventory failed");
                }

                var data = JsonUtility.FromJson<InventoryEmitPacket>(packet.data);

                if (data.playerId != PlayerManager.Instance.localPlayerId)
                {
                    Debug.LogError("no local player");
                    return;
                }
                string[] items = data.items;
                foreach(string i in items)
                {
                    ItemDatabase itemDatabase = InventoryManager.Instance.database;
                    Item thisitem=itemDatabase.GetById(i);
                    InventoryManager.Instance.AddItem(thisitem);
                }
                break;
            }
            case "add-item":
            {
                if (InventoryManager.Instance == null)
                {
                    Debug.LogError("inventorymanager does not exist yet, add-item failed");
                }
                var data = JsonUtility.FromJson<AddItemPacket>(packet.data);
                Debug.Log("ADD-ITEM RAW DATA: " + packet.data);

                if (data.id != PlayerManager.Instance.localPlayerId)
                {
                    Debug.LogError("not local player");
                    return;
                }
                ItemDatabase itemDatabase = InventoryManager.Instance.database;
                Item thisitem=itemDatabase.GetById(data.itemId);
                InventoryManager.Instance.AddItem(thisitem);
                   
                break;
            }
            case "remove-item":
            {
                if (InventoryManager.Instance == null)
                {
                    Debug.LogError("inventorymanager does not exist yet, add-item failed");
                    return;
                }
                var data = JsonUtility.FromJson<RemoveItemPacket>(packet.data);
                Debug.Log("remove-ITEM RAW DATA: " + packet.data);
                if (data.playerId != PlayerManager.Instance.localPlayerId)
                {
                    Debug.LogError("not local player");
                    return;
                }

                InventoryManager.Instance.RemoveItem(data.itemId);
                break;
            }
            default:
                Debug.LogWarning("Unknown WS message type: " + packet.type);
                break;
        }
    }
}

