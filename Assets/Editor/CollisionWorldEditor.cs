using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollisionWorld))]
public class CollisionWorldEditor : Editor
{
    int selectedIndex = -1;
    bool addPointMode;

    public override void OnInspectorGUI()
    {
        var world = (CollisionWorld)target;

        EditorGUILayout.LabelField("Collision World", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Open Tools > Collision > Collision World Editor, or draw here: " +
            "enable Add Point and click in the Scene view.",
            MessageType.Info);

        if (GUILayout.Button("Open Collision World Editor"))
            CollisionWorldToolWindow.OpenWindow();

        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("zoneName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmoHeight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fillColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outlineColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("vertexColor"));
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Vertices: {world.points.Count}");

        addPointMode = GUILayout.Toggle(addPointMode, "Add Point (click in Scene view)", "Button");

        if (GUILayout.Button("Add Point At View Center"))
            AddPointAt(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : world.transform.position);

        if (selectedIndex >= 0 && selectedIndex < world.points.Count)
        {
            EditorGUILayout.LabelField($"Selected: vertex {selectedIndex}");
            if (GUILayout.Button("Remove Selected Vertex"))
            {
                Undo.RecordObject(world, "Remove Collision Vertex");
                world.points.RemoveAt(selectedIndex);
                selectedIndex = -1;
                EditorUtility.SetDirty(world);
            }
        }

        if (GUILayout.Button("Clear All Vertices"))
        {
            Undo.RecordObject(world, "Clear Collision Vertices");
            world.points.Clear();
            selectedIndex = -1;
            EditorUtility.SetDirty(world);
        }
    }

    void OnSceneGUI()
    {
        var world = (CollisionWorld)target;
        CollisionWorldDraw.Draw(world, ref selectedIndex, addPointMode, AddPointAt);
    }

    void AddPointAt(Vector3 worldPos)
    {
        var world = (CollisionWorld)target;
        Undo.RecordObject(world, "Add Collision Vertex");
        worldPos.y = world.transform.position.y;
        world.points.Add(world.transform.InverseTransformPoint(worldPos));
        selectedIndex = world.points.Count - 1;
        EditorUtility.SetDirty(world);
        SceneView.RepaintAll();
    }
}

public class CollisionWorldToolWindow : EditorWindow
{
    int selectedIndex = -1;
    bool addPointMode;

    [MenuItem("Tools/Collision/Collision World Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<CollisionWorldToolWindow>("Collision World");
        window.minSize = new Vector2(280, 220);
        window.Show();
    }

    [MenuItem("Tools/Collision/Create Collision World")]
    public static void CreateFromToolsMenu()
    {
        CollisionWorldCreate.Create();
    }

    [MenuItem("GameObject/Collision/Collision World", false, 10)]
    public static void CreateFromGameObjectMenu()
    {
        CollisionWorldCreate.Create();
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Collision World Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Create or select a Collision World, then draw a walkable polygon in the Scene view.",
            MessageType.Info);

        if (GUILayout.Button("Create Collision World"))
            CollisionWorldCreate.Create();

        var worlds = FindObjectsByType<CollisionWorld>(FindObjectsSortMode.None);
        EditorGUILayout.LabelField($"In scene: {worlds.Length}");

        foreach (var world in worlds)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(world.zoneName, GUILayout.Width(120));
            EditorGUILayout.LabelField($"{world.points.Count} pts", GUILayout.Width(50));
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = world.gameObject;
                SceneView.FrameLastActiveSceneView();
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        addPointMode = GUILayout.Toggle(addPointMode, "Add Point (click Scene view)");

        var selected = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<CollisionWorld>()
            : null;

        if (selected == null)
        {
            EditorGUILayout.HelpBox("Select a Collision World object to edit vertices.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Add Point At View Center"))
        {
            Undo.RecordObject(selected, "Add Collision Vertex");
            Vector3 pos = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : selected.transform.position;
            pos.y = selected.transform.position.y;
            selected.points.Add(selected.transform.InverseTransformPoint(pos));
            EditorUtility.SetDirty(selected);
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI(SceneView view)
    {
        var selected = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<CollisionWorld>()
            : null;
        if (selected == null) return;

        CollisionWorldDraw.Draw(selected, ref selectedIndex, addPointMode, worldPos =>
        {
            Undo.RecordObject(selected, "Add Collision Vertex");
            worldPos.y = selected.transform.position.y;
            selected.points.Add(selected.transform.InverseTransformPoint(worldPos));
            selectedIndex = selected.points.Count - 1;
            EditorUtility.SetDirty(selected);
            Repaint();
        });
    }
}

static class CollisionWorldCreate
{
    public static void Create()
    {
        var go = new GameObject("CollisionWorld");
        go.AddComponent<CollisionWorld>();
        Undo.RegisterCreatedObjectUndo(go, "Create Collision World");
        Selection.activeGameObject = go;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();
        CollisionWorldToolWindow.OpenWindow();
    }
}

static class CollisionWorldDraw
{
    public static void Draw(CollisionWorld world, ref int selectedIndex, bool addPointMode, System.Action<Vector3> addPointAt)
    {
        if (world == null || world.points == null) return;

        DrawFilledPolygon(world);
        DrawVertexHandles(world, ref selectedIndex);
        DrawLabels(world);

        if (!addPointMode) return;

        Event e = Event.current;
        if (e.type != EventType.MouseDown || e.button != 0 || e.alt) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 hitPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
            hitPoint = hit.point;
        else
        {
            float y = world.transform.position.y + world.gizmoHeight;
            if (Mathf.Abs(ray.direction.y) < 0.0001f) return;
            float t = (y - ray.origin.y) / ray.direction.y;
            hitPoint = ray.origin + ray.direction * t;
        }

        addPointAt(hitPoint);
        e.Use();
        SceneView.RepaintAll();
    }

    static void DrawFilledPolygon(CollisionWorld world)
    {
        if (world.points.Count < 3) return;

        Handles.color = world.fillColor;
        Handles.DrawAAConvexPolygon(world.GetWorldPoints());

        Handles.color = world.outlineColor;
        var pts = world.GetWorldPoints();
        for (int i = 0; i < pts.Length; i++)
            Handles.DrawLine(pts[i], pts[(i + 1) % pts.Length]);
    }

    static void DrawVertexHandles(CollisionWorld world, ref int selectedIndex)
    {
        for (int i = 0; i < world.points.Count; i++)
        {
            Vector3 worldPos = world.GetWorldPoint(i) + Vector3.up * world.gizmoHeight;
            float pickSize = HandleUtility.GetHandleSize(worldPos) * 0.1f;

            if (Handles.Button(worldPos, Quaternion.identity, pickSize, pickSize, Handles.SphereHandleCap))
                selectedIndex = i;

            if (selectedIndex == i)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(world, "Move Collision Vertex");
                    moved.y -= world.gizmoHeight;
                    world.points[i] = world.transform.InverseTransformPoint(moved);
                    EditorUtility.SetDirty(world);
                }
            }
        }
    }

    static void DrawLabels(CollisionWorld world)
    {
        if (world.points.Count == 0) return;

        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = world.outlineColor },
            alignment = TextAnchor.MiddleCenter
        };

        Vector3 labelPos = Vector3.zero;
        for (int i = 0; i < world.points.Count; i++)
            labelPos += world.GetWorldPoint(i);
        labelPos /= world.points.Count;
        labelPos.y += world.gizmoHeight + 0.5f;

        if (world.points.Count >= 3)
            Handles.Label(labelPos, world.zoneName, style);

        var indexStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };
        for (int i = 0; i < world.points.Count; i++)
        {
            Vector3 p = world.GetWorldPoint(i) + Vector3.up * (world.gizmoHeight + 0.35f);
            Handles.Label(p, i.ToString(), indexStyle);
        }
    }
}
