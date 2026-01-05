using UnityEngine;
[System.Serializable]
public class LootPickedPacket
{
    public string id;
}
[System.Serializable]
public class LootSpawnPacket
{
    public string id;
    public string itemName;
    public Vec3 position;
    public int level;
    public Sprite icon;
}

