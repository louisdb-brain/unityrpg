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
    
    public string playerId;
    public string item;
}

[System.Serializable]
public class dropInventoryLootPacket
{
    public string playerId;
    public string itemName;
}