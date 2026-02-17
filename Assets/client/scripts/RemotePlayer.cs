using UnityEngine;

public class RemotePlayer : PlayerBase
{
    [Header("Movement Particles")]
    public GameObject moveParticlePrefab;
    public float particleInterval = 0.3f;
    public Vector3 particleOffset = new Vector3(0, 0.02f, 0);
    private float particleTimer = 0f;
    
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
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * lerpSpeed
        );

        Quaternion targetRot = Quaternion.Euler(0f, targetAngle, 0f);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * lerpSpeed
        );
    }
    void HandleMovementParticles()
    {
        Vector3 move = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        // If player is moving
        if (move.sqrMagnitude > 0.001f)
        {
            particleTimer += Time.deltaTime;

            if (particleTimer >= particleInterval)
            {
                SpawnMoveParticle();
                particleTimer = 0f;
            }
        }
        else
        {
            // Reset timer so it doesn't instantly spawn when moving again
            particleTimer = particleInterval;
        }
    }
    void SpawnMoveParticle()
    {
        if (moveParticlePrefab == null) return;

        Vector3 spawnPos = transform.position + particleOffset;

        Instantiate(moveParticlePrefab, spawnPos, Quaternion.identity);
    }
}