using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class SpritePrefabBatchCreator : EditorWindow
{
    private GameObject basePrefab;
    private DefaultAsset outputFolder;

    private bool searchSpriteRendererInChildren = true;
    private bool overwriteExisting = false;

    [MenuItem("Tools/Prefabs/Create Prefabs From Selected Sprites...", priority = 120)]
    public static void OpenWindow()
    {
        SpritePrefabBatchCreator window = GetWindow<SpritePrefabBatchCreator>("Sprite Prefab Batch Creator");
        window.minSize = new Vector2(520f, 220f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Create Prefabs From Selected Sprites (Unique Prefabs, Not Variants)", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        basePrefab = (GameObject)EditorGUILayout.ObjectField(
            "Base Prefab (Template)",
            basePrefab,
            typeof(GameObject),
            false
        );

        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Output Folder",
            outputFolder,
            typeof(DefaultAsset),
            false
        );

        searchSpriteRendererInChildren = EditorGUILayout.ToggleLeft(
            "If no SpriteRenderer on root, search in children",
            searchSpriteRendererInChildren
        );

        overwriteExisting = EditorGUILayout.ToggleLeft(
            "Overwrite existing prefabs (same file name)",
            overwriteExisting
        );

        EditorGUILayout.Space(10);

        Sprite[] selectedSprites = GetSelectedSprites();
        EditorGUILayout.LabelField($"Selected sprites: {selectedSprites.Length}");

        using (new EditorGUI.DisabledScope(basePrefab == null || selectedSprites.Length == 0))
        {
            if (GUILayout.Button("Create Prefabs", GUILayout.Height(34)))
            {
                CreatePrefabs(basePrefab, selectedSprites, GetOutputFolderPath());
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "This uses LoadPrefabContents + SaveAsPrefabAsset, which creates brand new prefabs (no prefab variant link).\n\n" +
            "Usage:\n" +
            "1) Select one or more Sprite assets in the Project window.\n" +
            "2) Open Tools → Prefabs → Create Prefabs From Selected Sprites...\n" +
            "3) Assign a Base Prefab, choose an Output Folder, then click Create.",
            MessageType.Info
        );
    }

    private static Sprite[] GetSelectedSprites()
    {
        Object[] objects = Selection.objects;
        if (objects == null || objects.Length == 0)
        {
            return new Sprite[0];
        }

        System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] is Sprite sprite)
            {
                sprites.Add(sprite);
            }
        }

        return sprites.ToArray();
    }

    private string GetOutputFolderPath()
    {
        if (outputFolder == null)
        {
            if (basePrefab != null)
            {
                string prefabPath = AssetDatabase.GetAssetPath(basePrefab);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    string dir = Path.GetDirectoryName(prefabPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        return dir.Replace("\\", "/");
                    }
                }
            }

            return "Assets";
        }

        string folderPath = AssetDatabase.GetAssetPath(outputFolder);
        if (string.IsNullOrEmpty(folderPath))
        {
            return "Assets";
        }

        return folderPath.Replace("\\", "/");
    }

    private void CreatePrefabs(GameObject templatePrefab, Sprite[] sprites, string folderPath)
    {
        if (templatePrefab == null)
        {
            Debug.LogError("Base Prefab is not assigned.");
            return;
        }

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("No sprites selected.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError($"Output folder is not valid: {folderPath}");
            return;
        }

        string templatePath = AssetDatabase.GetAssetPath(templatePrefab);
        if (string.IsNullOrEmpty(templatePath) || !templatePath.EndsWith(".prefab"))
        {
            Debug.LogError("Base Prefab must be a prefab asset from the Project window.");
            return;
        }

        int createdCount = 0;
        int skippedCount = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];
                if (sprite == null)
                {
                    skippedCount++;
                    continue;
                }

                string safeFileName = MakeSafeFileName(sprite.name);
                string targetPath = $"{folderPath}/{safeFileName}.prefab".Replace("\\", "/");

                if (!overwriteExisting)
                {
                    targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);
                }

                // Load prefab contents as a disconnected editable root (this prevents variants).
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(templatePath);
                if (contentsRoot == null)
                {
                    Debug.LogError($"Failed to load prefab contents from: {templatePath}");
                    skippedCount++;
                    continue;
                }

                try
                {
                    SpriteRenderer spriteRenderer = contentsRoot.GetComponent<SpriteRenderer>();
                    if (spriteRenderer == null && searchSpriteRendererInChildren)
                    {
                        spriteRenderer = contentsRoot.GetComponentInChildren<SpriteRenderer>(true);
                    }

                    if (spriteRenderer == null)
                    {
                        Debug.LogWarning($"Skipped '{sprite.name}': No SpriteRenderer found on prefab (root or children).");
                        skippedCount++;
                        continue;
                    }

                    spriteRenderer.sprite = sprite;

                    // Rename root object inside the prefab.
                    contentsRoot.name = sprite.name;

                    bool success;
                    PrefabUtility.SaveAsPrefabAsset(contentsRoot, targetPath, out success);

                    if (success)
                    {
                        createdCount++;
                    }
                    else
                    {
                        Debug.LogError($"Failed to save prefab for sprite '{sprite.name}' to '{targetPath}'.");
                        skippedCount++;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contentsRoot);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"Prefab batch complete. Created: {createdCount}, Skipped: {skippedCount}. Output: {folderPath}");
    }

    private static string MakeSafeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "NewPrefab";
        }

        char[] invalidChars = Path.GetInvalidFileNameChars();
        for (int i = 0; i < invalidChars.Length; i++)
        {
            name = name.Replace(invalidChars[i].ToString(), "_");
        }

        name = name.Trim().TrimEnd('.');
        if (string.IsNullOrEmpty(name))
        {
            name = "NewPrefab";
        }

        return name;
    }
}