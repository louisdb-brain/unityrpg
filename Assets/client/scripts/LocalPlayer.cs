using UnityEngine;

public class LocalPlayer : PlayerBase
{
    public override void OnNetworkUpdate(Vector3 targetPos, float targetAngle)
    {
        // Do nothing — local player moves itself
    }

    public override void OnServerCorrection(Vector3 pos, float angle)
    {
        // Optional: enable this if you want server reconciliation
        // transform.position = pos;
    }
}