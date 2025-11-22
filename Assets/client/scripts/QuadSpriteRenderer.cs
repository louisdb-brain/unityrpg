using UnityEngine;

public class QuadSpriteRenderer : MonoBehaviour
{
    public Material litMaterial;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (litMaterial == null)
            litMaterial = meshRenderer.material; 
    }

    // Animator calls this automatically because SpriteRenderer does it internally.
    public void SetSprite(Sprite sprite)
    {
        if (sprite == null) return;

        litMaterial.SetTexture("_BaseMap", sprite.texture);

        // OPTIONAL: match the quad’s aspect ratio to the sprite
        float w = sprite.rect.width / sprite.pixelsPerUnit;
        float h = sprite.rect.height / sprite.pixelsPerUnit;
        transform.localScale = new Vector3(w, h, transform.localScale.z);
    }
}
