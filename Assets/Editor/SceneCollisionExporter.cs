using UnityEditor;

/// <summary>
/// Legacy menu shortcut — opens the Collision Export Manager.
/// </summary>
public static class SceneCollisionExporter
{
    [MenuItem("Tools/Export Scene Collision")]
    public static void Export()
    {
        CollisionExportWindow.Open();
    }

    [MenuItem("Tools/Export Scene Collision (Quick Save)")]
    public static void QuickExport()
    {
        CollisionDataIO.ExportScene();
    }
}
