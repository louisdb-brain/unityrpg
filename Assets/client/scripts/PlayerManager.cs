using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("Prefabs")]
    public GameObject localPlayerPrefab;
    public GameObject remotePlayerPrefab;

    // Players stored by server ID
    private Dictionary<string, PlayerBase> players = new();

    // Your own player ID (set by server upon connect)
    public string localPlayerId;

    void Awake()
    {
        // Singleton protection
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===========================================
    // SPAWNING PLAYERS
    // ===========================================
    public void SpawnPlayer(string id, Vector3 pos)
    {
        if (players.ContainsKey(id))
            return;

        bool isLocal = (id == localPlayerId);

        GameObject prefab = isLocal ? localPlayerPrefab : remotePlayerPrefab;

        GameObject playerObj = Instantiate(prefab, pos, Quaternion.identity);

        PlayerBase baseClass = playerObj.GetComponent<PlayerBase>();
        baseClass.playerId = id;

        players.Add(id, baseClass);

        Debug.Log($"Spawned {(isLocal ? "local" : "remote")} player: {id}");
    }

    // ===========================================
    // POSITION / MOVEMENT UPDATES
    // ===========================================
    public void UpdatePlayerPos(string id, Vector3 pos, float angle)
    {
        if (!players.TryGetValue(id, out var player))
            return;

        // Local player uses client prediction (skip server correction)
        if (id == localPlayerId)
        {
            player.OnServerCorrection(pos, angle);
        }
        else
        {
            player.OnNetworkUpdate(pos, angle);
        }
    }

    // ===========================================
    // REMOVE PLAYERS
    // ===========================================
    public void RemovePlayer(string id)
    {
        if (!players.ContainsKey(id))
            return;

        Destroy(players[id].gameObject);
        players.Remove(id);

        Debug.Log($"Removed player: {id}");
    }
}
