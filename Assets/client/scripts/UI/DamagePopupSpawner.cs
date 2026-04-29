using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    public static DamagePopupSpawner Instance;
    public GameObject damagePopupPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void Spawn(Vector3 position, int amount)
    {
        GameObject obj = Instantiate(
            damagePopupPrefab,
            position + Vector3.up * 1.5f,
            Quaternion.identity
        );

        obj.GetComponent<DamagePopup>().SetDamage(amount);
    }
    public void SpawnCombo(Vector3 position, int amount)
    {
        GameObject obj = Instantiate(
            damagePopupPrefab,
            position + Vector3.up * 1.5f,
            Quaternion.identity
        );

        obj.GetComponent<DamagePopup>().setText("combo "+amount+"!");
    }
}