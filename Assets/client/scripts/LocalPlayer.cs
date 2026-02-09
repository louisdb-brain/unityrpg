using UnityEngine;

public class LocalPlayer : PlayerBase
{
    [Header("Spells")]
    public SpellPrototype activeSpell;

    public Vector3 LastMoveDirection { get; private set; } = Vector3.forward;

    void Start()
    {
        NetworkClient.Instance.Send("request-inventory",JsonUtility.FromJson<PlayerIdPacket>(playerId));
    }

    void Update()
    {
        UpdateMoveDirection();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CastSpell();
        }
    }

    void UpdateMoveDirection()
    {
        Vector3 move = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        if (move.sqrMagnitude > 0.001f)
        {
            LastMoveDirection = move.normalized;
            
        }
        CheckInteractRay();
    }

    void CheckInteractRay()
    {
        float interactDistance = 1.5f;
        Debug.DrawRay(transform.position, LastMoveDirection *interactDistance, Color.red);

        
        Ray ray = new Ray(transform.position, LastMoveDirection);
        if (Physics.Raycast(ray, out RaycastHit hit,interactDistance))
        {
                            Debug.DrawRay(transform.position, LastMoveDirection *interactDistance, Color.red);

            switch (hit.collider.tag)
            {
                case "NPC":
                break;
                case "Player":
                    //used for inspecting and trading
                case "NODE":
                    GameObject hitObject=hit.collider.gameObject;
                    if (hit.collider.TryGetComponent<nodeBehaviour>(out var node))
                    {
                        node.ShowInteract();
                    }                    
                    break;
            }
            
        }
    }

    void CheckInventoryForItem()
    {
        
    }

    void CastSpell()
    {
        if (activeSpell == null) return;

        NetworkClient.Instance.Send(
            "spellcast",
            new CastSpellPacket
            {
                spellId = System.Guid.NewGuid().ToString(),
                prefabName = activeSpell.prefabName,

                position = transform.position,
                direction = LastMoveDirection,

                speed = activeSpell.speed,
                radius = activeSpell.radius,
                damage = activeSpell.damage,
                lifetime = activeSpell.lifetime
            }
        );
    }


    public override void OnNetworkUpdate(Vector3 targetPos, float targetAngle) {}
    public override void OnServerCorrection(Vector3 pos, float angle) {}
}