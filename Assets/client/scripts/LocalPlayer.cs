using UnityEngine;

public class LocalPlayer : PlayerBase
{
    [Header("Movement Particles")]
    public GameObject moveParticlePrefab;
    public float particleInterval = 0.3f;
    public Vector3 particleOffset = new Vector3(0, 0.02f, 0);
    private float particleTimer = 0f;

    [Header("Spells")]
    public SpellPrototype activeSpell;

    public Vector3 LastMoveDirection { get; private set; } = Vector3.forward;

    void Start()
    {
        NetworkClient.Instance.Send("request-inventory",new PlayerIdPacket { playerId = playerId });
    }

    void Update()
    {
        UpdateMoveDirection();
        HandleMovementParticles(); 
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
                case "NPC_TALKER": 
                    GameObject talkeObject=hit.collider.gameObject;
                    Debug.Log(("hit ping"));
                    if (hit.collider.TryGetComponent<DialogueStarter>(out var talk))
                    {
                        talk.ShowInteract();
                    }                    
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