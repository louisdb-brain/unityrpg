using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneCollisionExporter
{
    [MenuItem("Tools/Export Scene Collision")]
    public static void Export()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        var zoneEntries = new List<ZoneEntry>();
        foreach (var world in UnityEngine.Object.FindObjectsByType<CollisionWorld>(FindObjectsSortMode.None))
        {
            if (world.points == null || world.points.Count < 3) continue;

            var pts = new List<PointEntry>();
            foreach (var p in world.points)
            {
                Vector3 wp = world.transform.TransformPoint(p);
                pts.Add(new PointEntry { x = wp.x, z = wp.z });
            }

            zoneEntries.Add(new ZoneEntry
            {
                name = string.IsNullOrEmpty(world.zoneName) ? world.gameObject.name : world.zoneName,
                points = pts.ToArray()
            });
        }

        var obstacles = new List<ObstacleEntry>();
        foreach (var col in UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
        {
            if (!col.enabled || col.isTrigger) continue;
            if (!col.CompareTag("Obstacle")) continue;

            Bounds b = col.bounds;
            obstacles.Add(new ObstacleEntry
            {
                x = b.center.x,
                z = b.center.z,
                w = b.size.x,
                d = b.size.z,
                y = b.center.y,
                h = b.size.y
            });
        }

        string json = BuildJson(sceneName, zoneEntries, obstacles);

        string serverDir = Path.Combine(Application.dataPath, "../server/data");
        Directory.CreateDirectory(serverDir);
        string serverPath = Path.Combine(serverDir, $"collision_{sceneName}.json");
        File.WriteAllText(serverPath, json);

        AssetDatabase.Refresh();
        Debug.Log($"Exported collision for '{sceneName}': {zoneEntries.Count} zones, {obstacles.Count} obstacles\n  {serverPath}");
    }

    static string BuildJson(string sceneName, List<ZoneEntry> zones, List<ObstacleEntry> obstacles)
    {
        var sb = new StringBuilder();
        sb.Append("{\n");
        sb.Append($"  \"scene\": \"{sceneName}\",\n");
        sb.Append("  \"zones\": [\n");
        for (int z = 0; z < zones.Count; z++)
        {
            var zone = zones[z];
            sb.Append("    {\n");
            sb.Append($"      \"name\": \"{zone.name}\",\n");
            sb.Append("      \"points\": [\n");
            for (int p = 0; p < zone.points.Length; p++)
            {
                var pt = zone.points[p];
                sb.Append($"        {{ \"x\": {pt.x}, \"z\": {pt.z} }}");
                sb.Append(p < zone.points.Length - 1 ? ",\n" : "\n");
            }
            sb.Append("      ]\n");
            sb.Append("    }");
            sb.Append(z < zones.Count - 1 ? ",\n" : "\n");
        }
        sb.Append("  ],\n");
        sb.Append("  \"obstacles\": [\n");
        for (int o = 0; o < obstacles.Count; o++)
        {
            var ob = obstacles[o];
            sb.Append($"    {{ \"x\": {ob.x}, \"z\": {ob.z}, \"w\": {ob.w}, \"d\": {ob.d}, \"y\": {ob.y}, \"h\": {ob.h} }}");
            sb.Append(o < obstacles.Count - 1 ? ",\n" : "\n");
        }
        sb.Append("  ]\n");
        sb.Append("}\n");
        return sb.ToString();
    }

    [Serializable]
    class ZoneEntry
    {
        public string name;
        public PointEntry[] points;
    }

    [Serializable]
    class PointEntry
    {
        public float x, z;
    }

    [Serializable]
    class ObstacleEntry
    {
        public float x, z, w, d, y, h;
    }
}
