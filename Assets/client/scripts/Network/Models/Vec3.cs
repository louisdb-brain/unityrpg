using UnityEngine;

[System.Serializable]
public class Vec3
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToUnity()
    {
        return new Vector3(x, y, z);
    }
}