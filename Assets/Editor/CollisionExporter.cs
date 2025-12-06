using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CollisionExporter : EditorWindow
{
    [MenuItem("Tools/Export Collision")]
    public static void ExportCollision()
    {
        BoxCollider[] boxes = GameObject.FindObjectsOfType<BoxCollider>();
        List<BoxEntry> boxList = new();

        foreach (var box in boxes)
        {
            if (!box.enabled) continue;

            Vector3 pos = box.transform.position;
            Vector3 size = box.size;

            boxList.Add(new BoxEntry()
            {
                x = pos.x,
                y = pos.y,
                z = pos.z,
                w = size.x * box.transform.localScale.x,
                h = size.y * box.transform.localScale.y,
                d = size.z * box.transform.localScale.z
            });
        }

        CollisionRoot root = new CollisionRoot();
        root.boxes = boxList.ToArray();

        string json = JsonUtility.ToJson(root, true);

        string path = Application.dataPath + "/../server/data/collision.json";
        File.WriteAllText(path, json);

        Debug.Log("Exported collision to " + path);
    }
}

[System.Serializable]
public class CollisionRoot
{
    public BoxEntry[] boxes;
}

[System.Serializable]
public class BoxEntry
{
    public float x, y, z;
    public float w, h, d;
}
