using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ItemProjectIconDrawer
{
    static ItemProjectIconDrawer()
    {
        EditorApplication.projectWindowItemOnGUI += DrawCustomIcon;
    }

    private static void DrawCustomIcon(string guid, Rect rect)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        item thisitem = AssetDatabase.LoadAssetAtPath<item>(path);

        if (thisitem == null || thisitem.icon == null)
            return;

        Texture2D tex = thisitem.icon.texture;
        if (tex == null)
            return;

        // ✅ Draw your Sprite as the Project icon
        GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit);
    }
}