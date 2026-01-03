[System.Serializable]
public class NPCPacket
{
    public string id;
    public string npcType;
    public int level;

    public Vec3 position;
    public Vec3 targetPosition;

    public float angle;
    public int health;
}

[System.Serializable]
public class NPCKillPacket
{
    public string id;
}

[System.Serializable]
public class NPCDamagePacket
{
    public string id;
    public int amount;
}