using UnityEngine;

public class RemotePlayer : PlayerBase
{
    private Vector3 targetPos;
    private float targetAngle;

    public float lerpSpeed = 10f;

    public override void OnNetworkUpdate(Vector3 pos, float angle)
    {
        targetPos = pos;
        targetAngle = angle;
    }

    public override void OnServerCorrection(Vector3 pos, float angle)
    {
        // Not used for remote players
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * lerpSpeed);

        Quaternion targetRot = Quaternion.Euler(0, targetAngle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lerpSpeed);
    }
}