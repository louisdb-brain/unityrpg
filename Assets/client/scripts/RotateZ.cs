using UnityEngine;

public class RotateZ : MonoBehaviour
{
    public float speed = 90f; // degrees per second

    void Update()
    {
        transform.Rotate(0f, 0f, -speed * Time.deltaTime);
    }
}