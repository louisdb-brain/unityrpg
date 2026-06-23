using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a walkable area polygon for this level.
/// Everything inside the shape is walkable; outside is blocked (used when collision is wired up later).
/// </summary>
[ExecuteAlways]
[AddComponentMenu("Collision/Collision World")]
public class CollisionWorld : MonoBehaviour
{
    public string zoneName = "WalkableArea";
    public List<Vector3> points = new();
    public float gizmoHeight = 0.1f;

    [Header("Gizmo Colors")]
    public Color fillColor = new Color(0.2f, 0.85f, 0.35f, 0.25f);
    public Color outlineColor = new Color(0.1f, 0.95f, 0.4f, 0.95f);
    public Color vertexColor = new Color(0.2f, 1f, 0.5f, 1f);

    public Vector3 GetWorldPoint(int index)
    {
        return transform.TransformPoint(points[index]);
    }

    public Vector3[] GetWorldPoints()
    {
        var world = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
            world[i] = GetWorldPoint(i) + Vector3.up * gizmoHeight;
        return world;
    }

    void OnDrawGizmos()
    {
        DrawGizmo(false);
    }

    void OnDrawGizmosSelected()
    {
        DrawGizmo(true);
    }

    void DrawGizmo(bool selected)
    {
        if (points == null || points.Count < 2) return;

        var world = GetWorldPoints();

        Gizmos.color = outlineColor;
        for (int i = 0; i < world.Length; i++)
        {
            int next = (i + 1) % world.Length;
            Gizmos.DrawLine(world[i], world[next]);
        }

        float sphereSize = selected ? 0.2f : 0.12f;
        Gizmos.color = vertexColor;
        foreach (var p in world)
            Gizmos.DrawSphere(p, sphereSize);
    }
}
