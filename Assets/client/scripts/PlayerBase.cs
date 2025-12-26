using UnityEngine;

public abstract class PlayerBase : MonoBehaviour
{
    public string playerId;
    
    public void Init(string id)
    {
        playerId = id;
    }

    public string getId()
    {
        return playerId;
    }
    public abstract void OnNetworkUpdate(Vector3 targetPos, float targetAngle);
    public abstract void OnServerCorrection(Vector3 pos, float angle);
}