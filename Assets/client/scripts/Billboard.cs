using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
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
}

