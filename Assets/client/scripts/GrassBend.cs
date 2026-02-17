using UnityEngine;

public class GrassBend : MonoBehaviour
{
    public float triggerDistance = 1.5f;   // Radius of effect
    public float squashYScale = 0.6f;      // Min height at center
    public float maxBendAngle = 50;       // Max Z rotation in degrees
    public float smoothSpeed = 3f;

    private Transform player;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private Vector3 targetScale;
    private Quaternion targetRotation;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        originalScale = transform.localScale;
        originalRotation = transform.localRotation;

        targetScale = originalScale;
        targetRotation = originalRotation;
    }

    void TryFindPlayer()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    void Update()
    {
        if (player == null)
        {
            TryFindPlayer();
            if (player == null) return;
        }

        // Horizontal direction only (ignore height)
        Vector3 toPlant = transform.position - player.position;
        toPlant.y = 0f;

        float distance = toPlant.magnitude;

        if (distance <= triggerDistance)
        {
            float t = distance / triggerDistance;   // 0 = center, 1 = edge

            // -------- SCALE --------
            float yScale = Mathf.Lerp(squashYScale, 1f, t);

            targetScale = new Vector3(
                originalScale.x,
                originalScale.y * yScale,
                originalScale.z
            );

            // -------- ROTATION --------
            toPlant.Normalize();

            // Decide left/right bend based on player's relative X direction
            float side = Mathf.Sign(toPlant.x);

            float angle = Mathf.Lerp(maxBendAngle, 0f, t) * side;

            targetRotation = originalRotation * Quaternion.Euler(angle, angle, angle);
        }
        else
        {
            targetScale = originalScale;
            targetRotation = originalRotation;
        }

        // Smooth transitions
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * smoothSpeed
        );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }
}
