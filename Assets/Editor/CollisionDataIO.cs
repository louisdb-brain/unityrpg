using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum CollisionSyncStatus
{
    MissingInFile,
    MissingInScene,
    Different,
    Same
}

[Serializable]
public class CollisionFileData
{
    public string scene;
    public CollisionZoneData[] zones;
    public CollisionObstacleData[] obstacles;
}

[Serializable]
public class CollisionZoneData
{
    public string name;
    public CollisionPointData[] points;
}

[Serializable]
public class CollisionPointData
{
    public float x, z;
}

[Serializable]
public class CollisionObstacleData
{
    public float x, z, w, d, y, h;
}

public static class CollisionDataIO
{
    const float Epsilon = 0.01f;

    public static string DataDirectory =>
        Path.Combine(Application.dataPath, "../server/data");

    public static string GetFilePath(string sceneName) =>
        Path.Combine(DataDirectory, $"collision_{sceneName}.json");

    public static string ActiveSceneName =>
        SceneManager.GetActiveScene().name;

    public static string GetZoneKey(CollisionWorld world) =>
        string.IsNullOrEmpty(world.zoneName) ? world.gameObject.name : world.zoneName;

    public static CollisionFileData ReadFile(string path)
    {
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<CollisionFileData>(File.ReadAllText(path));
    }

    public static CollisionFileData CaptureScene(string sceneName)
    {
        var data = new CollisionFileData
        {
            scene = sceneName,
            zones = CaptureZonesFromScene().ToArray(),
            obstacles = CaptureObstaclesFromScene().ToArray()
        };
        return data;
    }

    public static List<CollisionZoneData> CaptureZonesFromScene()
    {
        var zones = new List<CollisionZoneData>();
        foreach (var world in UnityEngine.Object.FindObjectsByType<CollisionWorld>(FindObjectsSortMode.None))
        {
            if (world.points == null || world.points.Count < 3) continue;
            zones.Add(CaptureZone(world));
        }
        return zones;
    }

    public static CollisionZoneData CaptureZone(CollisionWorld world)
    {
        var pts = new List<CollisionPointData>();
        foreach (var p in world.points)
        {
            Vector3 wp = world.transform.TransformPoint(p);
            pts.Add(new CollisionPointData { x = wp.x, z = wp.z });
        }
        return new CollisionZoneData
        {
            name = GetZoneKey(world),
            points = pts.ToArray()
        };
    }

    public static List<CollisionObstacleData> CaptureObstaclesFromScene()
    {
        var obstacles = new List<CollisionObstacleData>();
        foreach (var col in UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
        {
            if (!col.enabled || col.isTrigger) continue;
            if (!col.CompareTag("Obstacle")) continue;

            Bounds b = col.bounds;
            obstacles.Add(new CollisionObstacleData
            {
                x = b.center.x,
                z = b.center.z,
                w = b.size.x,
                d = b.size.z,
                y = b.center.y,
                h = b.size.y
            });
        }
        return obstacles;
    }

    public static void WriteFile(string path, CollisionFileData data)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, BuildJson(data));
    }

    public static void ExportScene(string sceneName = null)
    {
        sceneName ??= ActiveSceneName;
        var data = CaptureScene(sceneName);
        string path = GetFilePath(sceneName);
        WriteFile(path, data);
        AssetDatabase.Refresh();
        Debug.Log($"Exported collision '{sceneName}': {data.zones.Length} zones, {data.obstacles.Length} obstacles → {path}");
    }

    public static CollisionZoneData FindZone(CollisionFileData file, string zoneName)
    {
        if (file?.zones == null) return null;
        foreach (var z in file.zones)
            if (z.name == zoneName) return z;
        return null;
    }

    public static CollisionSyncStatus CompareZone(CollisionZoneData scene, CollisionZoneData file)
    {
        if (scene == null && file == null) return CollisionSyncStatus.Same;
        if (scene == null) return CollisionSyncStatus.MissingInScene;
        if (file == null) return CollisionSyncStatus.MissingInFile;
        return ZonesEqual(scene, file) ? CollisionSyncStatus.Same : CollisionSyncStatus.Different;
    }

    public static bool ZonesEqual(CollisionZoneData a, CollisionZoneData b)
    {
        if (a.points == null || b.points == null) return false;
        if (a.points.Length != b.points.Length) return false;
        for (int i = 0; i < a.points.Length; i++)
        {
            if (Mathf.Abs(a.points[i].x - b.points[i].x) > Epsilon) return false;
            if (Mathf.Abs(a.points[i].z - b.points[i].z) > Epsilon) return false;
        }
        return true;
    }

    public static void ApplyZoneToWorld(CollisionWorld world, CollisionZoneData zone)
    {
        Undo.RecordObject(world, "Import Collision Zone");
        world.zoneName = zone.name;
        world.points.Clear();
        float groundY = world.transform.position.y;
        foreach (var pt in zone.points)
        {
            Vector3 worldPos = new Vector3(pt.x, groundY, pt.z);
            world.points.Add(world.transform.InverseTransformPoint(worldPos));
        }
        EditorUtility.SetDirty(world);
        SceneView.RepaintAll();
    }

    public static CollisionWorld FindWorldByName(string zoneName)
    {
        foreach (var world in UnityEngine.Object.FindObjectsByType<CollisionWorld>(FindObjectsSortMode.None))
        {
            if (GetZoneKey(world) == zoneName)
                return world;
        }
        return null;
    }

    public static CollisionWorld ImportZone(CollisionZoneData zone, bool createIfMissing = true)
    {
        var world = FindWorldByName(zone.name);
        if (world == null && createIfMissing)
        {
            var go = new GameObject(zone.name);
            world = go.AddComponent<CollisionWorld>();
            Undo.RegisterCreatedObjectUndo(go, "Import Collision Zone");
        }
        if (world == null) return null;
        ApplyZoneToWorld(world, zone);
        return world;
    }

    public static void ImportScene(string sceneName = null)
    {
        sceneName ??= ActiveSceneName;
        var file = ReadFile(GetFilePath(sceneName));
        if (file?.zones == null)
        {
            Debug.LogWarning($"No collision file for scene '{sceneName}'");
            return;
        }
        foreach (var zone in file.zones)
            ImportZone(zone);
        Debug.Log($"Imported {file.zones.Length} zone(s) for '{sceneName}'");
    }

    public static void ExportZone(CollisionWorld world, string sceneName = null)
    {
        sceneName ??= ActiveSceneName;
        string path = GetFilePath(sceneName);
        var file = ReadFile(path) ?? new CollisionFileData
        {
            scene = sceneName,
            zones = Array.Empty<CollisionZoneData>(),
            obstacles = Array.Empty<CollisionObstacleData>()
        };

        var zoneList = new List<CollisionZoneData>(file.zones ?? Array.Empty<CollisionZoneData>());
        var captured = CaptureZone(world);
        int idx = zoneList.FindIndex(z => z.name == captured.name);
        if (idx >= 0) zoneList[idx] = captured;
        else zoneList.Add(captured);

        file.scene = sceneName;
        file.zones = zoneList.ToArray();
        if (file.obstacles == null)
            file.obstacles = CaptureObstaclesFromScene().ToArray();

        WriteFile(path, file);
        AssetDatabase.Refresh();
    }

    static string BuildJson(CollisionFileData data)
    {
        var sb = new StringBuilder();
        sb.Append("{\n");
        sb.Append($"  \"scene\": \"{data.scene}\",\n");
        sb.Append("  \"zones\": [\n");
        if (data.zones != null)
        {
            for (int z = 0; z < data.zones.Length; z++)
            {
                var zone = data.zones[z];
                sb.Append("    {\n");
                sb.Append($"      \"name\": \"{EscapeJson(zone.name)}\",\n");
                sb.Append("      \"points\": [\n");
                for (int p = 0; p < zone.points.Length; p++)
                {
                    var pt = zone.points[p];
                    sb.Append($"        {{ \"x\": {pt.x}, \"z\": {pt.z} }}");
                    sb.Append(p < zone.points.Length - 1 ? ",\n" : "\n");
                }
                sb.Append("      ]\n");
                sb.Append("    }");
                sb.Append(z < data.zones.Length - 1 ? ",\n" : "\n");
            }
        }
        sb.Append("  ],\n");
        sb.Append("  \"obstacles\": [\n");
        if (data.obstacles != null)
        {
            for (int o = 0; o < data.obstacles.Length; o++)
            {
                var ob = data.obstacles[o];
                sb.Append($"    {{ \"x\": {ob.x}, \"z\": {ob.z}, \"w\": {ob.w}, \"d\": {ob.d}, \"y\": {ob.y}, \"h\": {ob.h} }}");
                sb.Append(o < data.obstacles.Length - 1 ? ",\n" : "\n");
            }
        }
        sb.Append("  ]\n");
        sb.Append("}\n");
        return sb.ToString();
    }

    static string EscapeJson(string s) =>
        string.IsNullOrEmpty(s) ? s : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
