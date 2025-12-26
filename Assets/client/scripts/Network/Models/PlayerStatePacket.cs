[System.Serializable]
public class PlayerStatePacket
{
    public string id;
    public Vec3 pos;
    public Vec3 targetpos;
    public Vec3 lockedpos;
    public float angle;
    public bool locked;
    public int level;
}