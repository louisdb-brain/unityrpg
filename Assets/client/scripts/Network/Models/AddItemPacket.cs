using UnityEngine;

[System.Serializable]
public class AddItemPacket
{
    public string id;
    public string itemId;
}

[System.Serializable]
public class RemoveItemPacket
{
    public string playerId;
    public string itemId;
}

[System.Serializable]
public class dropInventoryLootPacket
{
    public string playerId;
    public string itemId;
}
