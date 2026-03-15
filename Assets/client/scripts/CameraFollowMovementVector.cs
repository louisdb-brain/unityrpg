using UnityEngine;

public class DiabloCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string targetTag = "Player";

    [Header("Follow")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 30f, -10f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Rotation")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(43f, 0f, 0f);

    private Transform targetTransform;
    private Vector3 followVelocity;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(fixedRotation);
        FindTarget();

        if (targetTransform != null)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        if (targetTransform == null)
        {
            FindTarget();

            if (targetTransform == null)
            {
                return;
            }

            SnapToTarget();
        }

        transform.rotation = Quaternion.Euler(fixedRotation);

        Vector3 targetPosition = targetTransform.position + positionOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref followVelocity,
            smoothTime
        );
    }

    private void FindTarget()
    {
        GameObject playerObject = GameObject.FindWithTag(targetTag);

        if (playerObject == null)
        {
            return;
        }

        targetTransform = playerObject.transform;
    }

    private void SnapToTarget()
    {
        if (targetTransform == null)
        {
            return;
        }

        Vector3 targetPosition = targetTransform.position + positionOffset;
        transform.position = targetPosition;
    }
}