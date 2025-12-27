using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;

    private Rigidbody rb;
    private LocalPlayer localPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        localPlayer = GetComponent<LocalPlayer>();
    }

    void FixedUpdate()
    {
        // Only the LOCAL player sends movement
        if (localPlayer == null)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0, v).normalized;
        Vector3 targetPos = rb.position + move * speed * Time.fixedDeltaTime;

        rb.MovePosition(targetPos);

        if (move.sqrMagnitude > 0.01f)
            transform.forward = move;

        // 🔑 SEND MOVEMENT TO SERVER
        NetworkClient.Instance.Send(
            "player-move",
            new PlayerMovePacket
            {
                x = rb.position.x,
                y = rb.position.y,
                z = rb.position.z,
                angle = transform.eulerAngles.y
            }
        );
    }
}