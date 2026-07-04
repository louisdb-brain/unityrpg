using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollisionWorld))]
public class CollisionWorldEditor : Editor
{
    readonly CollisionWorldSelection selection = new();
    bool addPointMode;
    Vector2 pointsScroll;

    public override void OnInspectorGUI()
    {
        var world = (CollisionWorld)target;

        EditorGUILayout.LabelField("Collision World", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Open Tools > Collision > Collision World Editor, or draw here: " +
            "enable Add Point and click in the Scene view. Shift+click to multi-select. " +
            "Select two adjacent points and use Split Edge.",
            MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Collision World Editor"))
            CollisionWorldToolWindow.OpenWindow();
        if (GUILayout.Button("Export Manager"))
            CollisionExportWindow.Open();
        EditorGUILayout.EndHorizontal();

        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("zoneName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmoHeight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fillColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outlineColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("vertexColor"));
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Vertices: {world.points.Count}  |  Selected: {selection.Count}");

        addPointMode = GUILayout.Toggle(addPointMode, "Add Point (click in Scene view)", "Button");

        if (GUILayout.Button("Add Point At View Center"))
            AddPointAt(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : world.transform.position);

        CollisionWorldPointsList.DrawSplitEdgeButton(world, selection);

        if (selection.Count > 0)
        {
            if (GUILayout.Button($"Remove Selected ({selection.Count})"))
            {
                Undo.RecordObject(world, "Remove Collision Vertices");
                CollisionWorldSelection.RemoveIndices(world, selection);
                EditorUtility.SetDirty(world);
            }
        }

        if (GUILayout.Button("Clear All Vertices"))
        {
            Undo.RecordObject(world, "Clear Collision Vertices");
            world.points.Clear();
            selection.Clear();
            EditorUtility.SetDirty(world);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Drop All Points To Floor"))
            CollisionWorldFloorDrop.DropPoints(world);

        if (selection.Count > 0 && GUILayout.Button($"Drop Selected To Floor ({selection.Count})"))
            CollisionWorldFloorDrop.DropPoints(world, selection);

        EditorGUILayout.Space();
        pointsScroll = EditorGUILayout.BeginScrollView(pointsScroll, GUILayout.MaxHeight(260));
        CollisionWorldPointsList.Draw(world, selection);
        EditorGUILayout.EndScrollView();
    }

    void OnSceneGUI()
    {
        var world = (CollisionWorld)target;
        CollisionWorldDraw.Draw(world, selection, addPointMode, AddPointAt);
    }

    void AddPointAt(Vector3 worldPos)
    {
        var world = (CollisionWorld)target;
        Undo.RecordObject(world, "Add Collision Vertex");
        worldPos.y = world.transform.position.y;
        world.points.Add(world.transform.InverseTransformPoint(worldPos));
        selection.SetSingle(world.points.Count - 1);
        EditorUtility.SetDirty(world);
        SceneView.RepaintAll();
    }
}

public class CollisionWorldToolWindow : EditorWindow
{
    readonly CollisionWorldSelection selection = new();
    bool addPointMode;
    Vector2 pointsScroll;

    [MenuItem("Tools/Collision/Collision World Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<CollisionWorldToolWindow>("Collision World");
        window.minSize = new Vector2(520, 360);
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
            "Shift+click points to multi-select. Select two adjacent edge points and Split Edge. " +
            "Edit X/Y/Z in the list to move points.",
            MessageType.Info);

        if (GUILayout.Button("Create Collision World"))
            CollisionWorldCreate.Create();

        if (GUILayout.Button("Open Export Manager"))
            CollisionExportWindow.Open();

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

        EditorGUILayout.LabelField($"Selected vertices: {selection.Count}");

        if (GUILayout.Button("Add Point At View Center"))
        {
            Undo.RecordObject(selected, "Add Collision Vertex");
            Vector3 pos = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : selected.transform.position;
            pos.y = selected.transform.position.y;
            selected.points.Add(selected.transform.InverseTransformPoint(pos));
            selection.SetSingle(selected.points.Count - 1);
            EditorUtility.SetDirty(selected);
            SceneView.RepaintAll();
        }

        CollisionWorldPointsList.DrawSplitEdgeButton(selected, selection);

        if (selection.Count > 0 && GUILayout.Button($"Remove Selected ({selection.Count})"))
        {
            Undo.RecordObject(selected, "Remove Collision Vertices");
            CollisionWorldSelection.RemoveIndices(selected, selection);
            EditorUtility.SetDirty(selected);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Drop All Points To Floor"))
            CollisionWorldFloorDrop.DropPoints(selected);

        if (selection.Count > 0 && GUILayout.Button($"Drop Selected To Floor ({selection.Count})"))
            CollisionWorldFloorDrop.DropPoints(selected, selection);

        EditorGUILayout.Space();
        pointsScroll = EditorGUILayout.BeginScrollView(pointsScroll);
        CollisionWorldPointsList.Draw(selected, selection);
        EditorGUILayout.EndScrollView();
    }

    void OnSceneGUI(SceneView view)
    {
        var selected = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<CollisionWorld>()
            : null;
        if (selected == null) return;

        CollisionWorldDraw.Draw(selected, selection, addPointMode, worldPos =>
        {
            Undo.RecordObject(selected, "Add Collision Vertex");
            worldPos.y = selected.transform.position.y;
            selected.points.Add(selected.transform.InverseTransformPoint(worldPos));
            selection.SetSingle(selected.points.Count - 1);
            EditorUtility.SetDirty(selected);
            Repaint();
        });
    }
}

class CollisionWorldSelection
{
    readonly HashSet<int> indices = new();

    public int Count => indices.Count;
    public IEnumerable<int> Indices => indices.OrderBy(i => i);
    public bool IsSelected(int i) => indices.Contains(i);

    public void SetSingle(int i)
    {
        indices.Clear();
        if (i >= 0) indices.Add(i);
    }

    public void Add(int i)
    {
        if (i >= 0) indices.Add(i);
    }

    public void Toggle(int i)
    {
        if (i < 0) return;
        if (!indices.Remove(i))
            indices.Add(i);
    }

    public void Clear() => indices.Clear();

    public void HandleClick(int i)
    {
        var e = Event.current;
        if (e.control)
            Toggle(i);
        else if (e.shift)
            Add(i);
        else
            SetSingle(i);
    }

    public static void RemoveIndices(CollisionWorld world, CollisionWorldSelection selection)
    {
        foreach (int i in selection.indices.OrderByDescending(x => x))
        {
            if (i >= 0 && i < world.points.Count)
                world.points.RemoveAt(i);
        }
        selection.Clear();
    }
}

static class CollisionWorldEdgeSplit
{
    public static bool CanSplit(int a, int b, int count)
    {
        if (count < 2) return false;
        if (a > b) (a, b) = (b, a);
        if (b - a == 1) return true;
        return a == 0 && b == count - 1;
    }

    public static int GetInsertIndex(int a, int b, int count)
    {
        if (a > b) (a, b) = (b, a);
        if (b - a == 1) return b;
        if (a == 0 && b == count - 1) return count;
        return -1;
    }

    public static int Split(CollisionWorld world, int a, int b, CollisionWorldSelection selection)
    {
        if (!CanSplit(a, b, world.points.Count))
            return -1;

        Undo.RecordObject(world, "Split Collision Edge");
        Vector3 mid = (world.GetWorldPoint(a) + world.GetWorldPoint(b)) * 0.5f;
        int insertAt = GetInsertIndex(a, b, world.points.Count);
        world.points.Insert(insertAt, world.transform.InverseTransformPoint(mid));
        selection.SetSingle(insertAt);
        EditorUtility.SetDirty(world);
        SceneView.RepaintAll();
        return insertAt;
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
    public static void Draw(CollisionWorld world, CollisionWorldSelection selection, bool addPointMode, System.Action<Vector3> addPointAt)
    {
        if (world == null || world.points == null) return;

        DrawFilledPolygon(world);
        DrawVertexHandles(world, selection);
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

    static void DrawVertexHandles(CollisionWorld world, CollisionWorldSelection selection)
    {
        for (int i = 0; i < world.points.Count; i++)
        {
            Vector3 worldPos = world.GetWorldPoint(i) + Vector3.up * world.gizmoHeight;
            float pickSize = HandleUtility.GetHandleSize(worldPos) * 0.1f;

            if (selection.IsSelected(i))
                Handles.color = Color.cyan;

            if (Handles.Button(worldPos, Quaternion.identity, pickSize, pickSize, Handles.SphereHandleCap))
                selection.HandleClick(i);

            Handles.color = Color.white;

            if (selection.IsSelected(i))
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

static class CollisionWorldPointsList
{
    public static void DrawSplitEdgeButton(CollisionWorld world, CollisionWorldSelection selection)
    {
        if (selection.Count != 2) return;

        var pair = selection.Indices.ToArray();
        int a = pair[0];
        int b = pair[1];
        bool canSplit = CollisionWorldEdgeSplit.CanSplit(a, b, world.points.Count);

        EditorGUI.BeginDisabledGroup(!canSplit);
        if (GUILayout.Button(canSplit
                ? $"Split Edge ({a} — {b}) At Midpoint"
                : "Split Edge (select two adjacent points)"))
        {
            CollisionWorldEdgeSplit.Split(world, a, b, selection);
        }
        EditorGUI.EndDisabledGroup();
    }

    public static void Draw(CollisionWorld world, CollisionWorldSelection selection)
    {
        EditorGUILayout.LabelField("Points (world space)", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("#", GUILayout.Width(22));
        EditorGUILayout.LabelField("X", GUILayout.Width(64));
        EditorGUILayout.LabelField("Y", GUILayout.Width(64));
        EditorGUILayout.LabelField("Z", GUILayout.Width(64));
        EditorGUILayout.LabelField("", GUILayout.Width(108));
        EditorGUILayout.EndHorizontal();

        if (world.points == null || world.points.Count == 0)
        {
            EditorGUILayout.HelpBox("No points yet.", MessageType.None);
            return;
        }

        for (int i = 0; i < world.points.Count; i++)
        {
            Vector3 w = world.GetWorldPoint(i);
            bool isSelected = selection.IsSelected(i);

            var oldBg = GUI.backgroundColor;
            if (isSelected)
                GUI.backgroundColor = new Color(0.45f, 0.75f, 1f);

            EditorGUILayout.BeginHorizontal("box");

            if (GUILayout.Button($"{i}", GUILayout.Width(22)))
            {
                var e = Event.current;
                if (e.control)
                    selection.Toggle(i);
                else if (e.shift)
                    selection.Add(i);
                else
                    selection.SetSingle(i);
            }

            EditorGUI.BeginChangeCheck();
            float nx = EditorGUILayout.FloatField(w.x, GUILayout.Width(64));
            float ny = EditorGUILayout.FloatField(w.y, GUILayout.Width(64));
            float nz = EditorGUILayout.FloatField(w.z, GUILayout.Width(64));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(world, "Move Collision Vertex");
                world.points[i] = world.transform.InverseTransformPoint(new Vector3(nx, ny, nz));
                EditorUtility.SetDirty(world);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Sel", GUILayout.Width(28)))
            {
                selection.SetSingle(i);
                Selection.activeGameObject = world.gameObject;
                Vector3 lookAt = world.GetWorldPoint(i) + Vector3.up * world.gizmoHeight;
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.LookAt(lookAt);
                    SceneView.lastActiveSceneView.Repaint();
                }
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Drop", GUILayout.Width(40)))
                CollisionWorldFloorDrop.DropPoints(world, i);

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = oldBg;
        }
    }
}

static class CollisionWorldFloorDrop
{
    const float RayStartHeight = 500f;
    const float RayDistance = 1200f;

    public static void DropPoints(CollisionWorld world, int selectedIndex = -1)
    {
        if (world == null || world.points == null || world.points.Count == 0)
            return;

        Undo.RecordObject(world, "Drop Collision Points To Floor");

        for (int i = 0; i < world.points.Count; i++)
        {
            if (selectedIndex >= 0 && i != selectedIndex)
                continue;

            Vector3 worldPt = world.GetWorldPoint(i);
            TryRaycastDown(worldPt, world, out Vector3 hit);
            world.points[i] = world.transform.InverseTransformPoint(hit);
        }

        EditorUtility.SetDirty(world);
        SceneView.RepaintAll();
    }

    public static void DropPoints(CollisionWorld world, CollisionWorldSelection selection)
    {
        if (world == null || world.points == null || world.points.Count == 0 || selection.Count == 0)
            return;

        Undo.RecordObject(world, "Drop Collision Points To Floor");

        foreach (int i in selection.Indices.ToList())
        {
            if (i < 0 || i >= world.points.Count) continue;
            Vector3 worldPt = world.GetWorldPoint(i);
            TryRaycastDown(worldPt, world, out Vector3 hit);
            world.points[i] = world.transform.InverseTransformPoint(hit);
        }

        EditorUtility.SetDirty(world);
        SceneView.RepaintAll();
    }

    static void TryRaycastDown(Vector3 worldPt, CollisionWorld world, out Vector3 hitPoint)
    {
        float startY = Mathf.Max(worldPt.y, world.transform.position.y) + RayStartHeight;
        Vector3 origin = new Vector3(worldPt.x, startY, worldPt.z);
        var hits = Physics.RaycastAll(origin, Vector3.down, RayDistance);

        if (hits.Length > 1)
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider == null)
                continue;
            if (IsSelfOrChild(hit.collider.transform, world.transform))
                continue;

            hitPoint = hit.point;
            return;
        }

        hitPoint = new Vector3(worldPt.x, world.transform.position.y, worldPt.z);
    }

    static bool IsSelfOrChild(Transform hit, Transform self)
    {
        Transform t = hit;
        while (t != null)
        {
            if (t == self)
                return true;
            t = t.parent;
        }
        return false;
    }
}
