using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionExportWindow : EditorWindow
{
    Vector2 scroll;
    string sceneName;
    CollisionFileData fileData;
    double lastRefreshTime;

    static readonly Color ColorSame = new Color(0.35f, 0.85f, 0.45f);
    static readonly Color ColorDifferent = new Color(0.95f, 0.35f, 0.35f);
    static readonly Color ColorMissing = new Color(0.35f, 0.85f, 0.95f);

    [MenuItem("Tools/Collision/Collision Export Manager")]
    public static void Open()
    {
        var w = GetWindow<CollisionExportWindow>("Collision Export");
        w.minSize = new Vector2(420, 320);
        w.Show();
    }

    void OnEnable()
    {
        Refresh();
        EditorApplication.update += OnEditorUpdate;
    }

    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    void OnEditorUpdate()
    {
        if (EditorApplication.timeSinceStartup - lastRefreshTime > 1.0)
            Refresh(silent: true);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Collision Export Manager", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Scene", sceneName ?? "—");
        string path = sceneName != null ? CollisionDataIO.GetFilePath(sceneName) : "—";
        EditorGUILayout.LabelField("File", path, EditorStyles.wordWrappedMiniLabel);

        bool fileExists = sceneName != null && System.IO.File.Exists(path);
        EditorGUILayout.LabelField("File status", fileExists ? "Found on disk" : "No file (export to create)");

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh"))
            Refresh();
        if (GUILayout.Button("Export All"))
        {
            CollisionDataIO.ExportScene(sceneName);
            Refresh();
        }
        GUI.enabled = fileExists;
        if (GUILayout.Button("Import All"))
        {
            if (ConfirmImport(
                    "Import all zones?",
                    $"Import all zones from collision_{sceneName}.json?\nThis will overwrite matching zone polygons in the scene."))
            {
                CollisionDataIO.ImportScene(sceneName);
                Refresh();
            }
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        DrawLegend();
        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawZoneList();
        EditorGUILayout.EndScrollView();
    }

    void DrawLegend()
    {
        EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        DrawStatusChip("Same", ColorSame);
        DrawStatusChip("Different", ColorDifferent);
        DrawStatusChip("Missing", ColorMissing);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox(
            "Green = scene matches exported file. Red = name exists but points differ. " +
            "Cyan = only in scene (not in file) or only in file (not in scene).",
            MessageType.None);
    }

    static void DrawStatusChip(string label, Color color)
    {
        var rect = GUILayoutUtility.GetRect(80, 20, GUILayout.Width(90));
        EditorGUI.DrawRect(rect, color);
        var style = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.black }
        };
        GUI.Label(rect, label, style);
    }

    void DrawZoneList()
    {
        var sceneZones = BuildSceneZoneMap();
        var fileZones = BuildFileZoneMap();
        var allNames = new HashSet<string>();
        foreach (var k in sceneZones.Keys) allNames.Add(k);
        foreach (var k in fileZones.Keys) allNames.Add(k);

        if (allNames.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No Collision World zones in scene (need 3+ points). Export file may still list saved zones.",
                MessageType.Info);
        }

        var sorted = new List<string>(allNames);
        sorted.Sort();

        foreach (var name in sorted)
        {
            sceneZones.TryGetValue(name, out CollisionZoneData sceneZone);
            fileZones.TryGetValue(name, out CollisionZoneData fileZone);
            var status = CollisionDataIO.CompareZone(sceneZone, fileZone);
            DrawZoneRow(name, status, sceneZone, fileZone);
        }
    }

    void DrawZoneRow(string name, CollisionSyncStatus status, CollisionZoneData sceneZone, CollisionZoneData fileZone)
    {
        Color bg = status switch
        {
            CollisionSyncStatus.Same => ColorSame,
            CollisionSyncStatus.Different => ColorDifferent,
            _ => ColorMissing
        };

        var oldBg = GUI.backgroundColor;
        GUI.backgroundColor = bg;
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel, GUILayout.Width(140));
        EditorGUILayout.LabelField(StatusLabel(status), GUILayout.Width(100));
        int scenePts = sceneZone?.points?.Length ?? 0;
        int filePts = fileZone?.points?.Length ?? 0;
        EditorGUILayout.LabelField($"Scene: {scenePts}  File: {filePts}", GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        var world = CollisionDataIO.FindWorldByName(name);
        if (world != null && GUILayout.Button("Select", GUILayout.Width(55)))
        {
            Selection.activeGameObject = world.gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        if (sceneZone != null && GUILayout.Button("Export", GUILayout.Width(55)))
        {
            if (world != null)
            {
                CollisionDataIO.ExportZone(world, sceneName);
                Refresh();
            }
        }

        GUI.enabled = fileZone != null;
        if (GUILayout.Button("Import", GUILayout.Width(55)))
        {
            if (ConfirmImport(
                    "Import zone?",
                    $"Import zone \"{name}\" from file?\nThis will overwrite the scene polygon."))
            {
                CollisionDataIO.ImportZone(fileZone, createIfMissing: true);
                Refresh();
            }
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        GUI.backgroundColor = oldBg;
        EditorGUILayout.Space(2);
    }

    static string StatusLabel(CollisionSyncStatus status) => status switch
    {
        CollisionSyncStatus.Same => "Same",
        CollisionSyncStatus.Different => "Different",
        CollisionSyncStatus.MissingInFile => "Not in file",
        CollisionSyncStatus.MissingInScene => "Not in scene",
        _ => "?"
    };

    Dictionary<string, CollisionZoneData> BuildSceneZoneMap()
    {
        var map = new Dictionary<string, CollisionZoneData>();
        foreach (var world in FindObjectsByType<CollisionWorld>(FindObjectsSortMode.None))
        {
            if (world.points == null || world.points.Count < 3) continue;
            var z = CollisionDataIO.CaptureZone(world);
            map[z.name] = z;
        }
        return map;
    }

    Dictionary<string, CollisionZoneData> BuildFileZoneMap()
    {
        var map = new Dictionary<string, CollisionZoneData>();
        if (fileData?.zones == null) return map;
        foreach (var z in fileData.zones)
        {
            if (string.IsNullOrEmpty(z.name)) continue;
            map[z.name] = z;
        }
        return map;
    }

    void Refresh(bool silent = false)
    {
        lastRefreshTime = EditorApplication.timeSinceStartup;
        sceneName = SceneManager.GetActiveScene().name;
        fileData = CollisionDataIO.ReadFile(CollisionDataIO.GetFilePath(sceneName));
        if (!silent)
            Repaint();
    }

    static bool ConfirmImport(string title, string message) =>
        EditorUtility.DisplayDialog(title, message, "Yes", "No");
}
