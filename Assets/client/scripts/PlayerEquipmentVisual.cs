using UnityEngine;

public class PlayerEquipmentVisual : MonoBehaviour
{
    public SpriteRenderer weaponRenderer;
    public Vector3 weaponLocalOffset = new Vector3(0.4f, 0.3f, 0f);
    public float weaponScale = 0.5f;

    void Awake()
    {
        if (weaponRenderer == null)
        {
            var go = new GameObject("EquippedWeapon");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = weaponLocalOffset;
            go.transform.localScale = Vector3.one * weaponScale;
            weaponRenderer = go.AddComponent<SpriteRenderer>();
            weaponRenderer.sortingOrder = 10;
        }

        SetWeaponSprite(null);
    }

    public void SetWeaponSprite(Sprite sprite)
    {
        if (weaponRenderer == null)
            return;

        weaponRenderer.sprite = sprite;
        weaponRenderer.enabled = sprite != null;
    }
}
