using System;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Header("Shadow Settings")]
    public float groundOffset = 0.1f;
    public Vector2 shadowOffset = new Vector2(0.25f, 0.25f); // offset on the X and Z axes
    public float shadowScale = 1f;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.45f);
    private SpriteRenderer shadowSpriterenderer;
    [Header("Shadow Material (skew shader)")]
    public Material shadowMaterial;   // assign your SpriteShadowSkewDirLight material here
    
    private Camera cam;
    
    private void Awake()
    {
        // Rotate this object once to face the camera
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = cam.transform.forward;
        Vector3 up = cam.transform.up;

        transform.LookAt(transform.position + forward, up);
    }
    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            
            if (cam == null) return;
        }

        // Match camera's forward, but not roll
        Vector3 forward = cam.transform.forward;
        Vector3 up = cam.transform.up;

        transform.LookAt(transform.position + forward, up);
    }

   

    private void Start()
    {
        cam = Camera.main;
        // Create the shadow after the billboard rotation
        SpriteRenderer parentSr = GetComponent<SpriteRenderer>();
        if (parentSr == null) return;

        string parentName = gameObject.name;
        GameObject shadowGO = new GameObject(parentName + "_shadow");
        Transform shadowTf = shadowGO.transform;

        // Place it in world space
        Vector3 pos = transform.position;
        pos.y = transform.position.y+groundOffset;
        pos.x += shadowOffset.x;
        pos.z += shadowOffset.y;
        shadowTf.position = pos;

        // Lay it flat (SpriteRenderer quad is in the X Y plane, so rotate ninety degrees on X)
        shadowTf.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Scale the shadow
        shadowTf.localScale = Vector3.one * shadowScale;

        // Add SpriteRenderer and use the same sprite, but darker
        shadowSpriterenderer = shadowGO.AddComponent<SpriteRenderer>();
        
        shadowSpriterenderer.sprite = parentSr.sprite;
        shadowSpriterenderer.sortingLayerID = parentSr.sortingLayerID;
        shadowSpriterenderer.sortingOrder = parentSr.sortingOrder - 1;
        shadowSpriterenderer.color = shadowColor;

        // Apply your skew shader material to the child
        if (shadowMaterial != null)
        {
            shadowSpriterenderer.material = shadowMaterial;
        }
        if (shadowMaterial != null)
        {
            // This creates an instance of the material for this shadow
            shadowSpriterenderer.material = new Material(shadowMaterial);

            
        }

        // Parent it, but keep world transform as it is
        shadowTf.SetParent(transform, true);
    }

    private void Update()
    {
        
        // Look up the light named "sunlight" in the scene
        GameObject sunObject = GameObject.Find("Sunlight");
        if (sunObject != null)
        {
            Light sunLight = sunObject.GetComponent<Light>();
            if (sunLight != null)
            {
                // Light direction in world space (points from surface towards the light)
                Vector3 lightDirection = sunLight.transform.forward*-1;

                // Shadow is cast in the opposite direction
                Vector3 shadowDirection = -lightDirection;

                // Project onto the ground XZ plane
                Vector2 groundDirection = new Vector2(shadowDirection.x, shadowDirection.z);
                if (groundDirection.sqrMagnitude > 0.0001f)
                {
                    groundDirection.Normalize();
                }
                else
                {
                    groundDirection = new Vector2(1f, 0f);
                }

                // In the shader, X and Y correspond to sprite local X and Y.
                // After you rotate the shadow ninety degrees around X,
                // local X maps to world X and local Y maps to world Z.
                Vector4 skewDir = new Vector4(groundDirection.x, groundDirection.y, 0f, 0f);
                shadowSpriterenderer.material.SetVector("_SkewDir", skewDir);
            }
        }
        
        
    }
}