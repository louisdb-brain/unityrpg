using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float floatSpeed = 1.5f;
    public float lifetime = 1f;

    private TextMeshPro text;
    private Color startColor;
    private float timer;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();

        startColor = text.color;
        startColor.a = 1f;
        text.color = startColor;
    }

    public void SetDamage(int amount)
    {
        text.text = amount.ToString();
    }

    void Update()
    {
        // Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Face camera
        if (Camera.main)
        {
            transform.forward = Camera.main.transform.forward;
        }

        // Fade out
        timer += Time.deltaTime;
        float t = timer / lifetime;

        text.color = new Color(
            startColor.r,
            startColor.g,
            startColor.b,
            1f - t
        );

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}