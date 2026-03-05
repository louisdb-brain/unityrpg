using UnityEngine;

[System.Serializable]
public class AddItemPacket
{
    public string id;
    public string name;
}
[System.Serializable]
public class RemoveItemPacket
{
    public string id;
    public string name;
}