using UnityEngine;
using Yarn.Unity;

public class YarnCommands : MonoBehaviour
{
    //[SerializeField] private QuestManager questManager;
    [SerializeField] private InventoryManager inventoryManager;
    
    /*[YarnCommand("start_quest")]
    public void StartQuest(string questId)
    {
        questManager.StartQuest(questId);
    }

    [YarnCommand("complete_quest")]
    public void CompleteQuest(string questId)
    {
        questManager.CompleteQuest(questId);
    }*/

    [YarnCommand("give_item")]
    public void GiveItem(string itemId, int amount)
    {
        NetworkClient.Instance.Send("add-item", new AddItemRequest
        {
            itemName = itemId
        });
    }
    [YarnCommand("remove_item")]
    public void RemoveItem(string itemId)
    {
        NetworkClient.Instance.Send("remove-item", new RemoveItemRequest
        {
            itemName = itemId
        });
    }/*
    [YarnFunction("search_item")]
    public bool GetItem(string itemName)
    {
       return InventoryManager.Instance.SearchForItemByName(itemName)!=-1;
    }*/
}
