using UnityEngine;
using Yarn.Unity;

public class YarnCommands : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    //[SerializeField] private QuestManager questManager;

    private void Awake()
    {
        if (dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner missing on YarnCommands");
            return;
        }

        dialogueRunner.AddCommandHandler<string, int>("give_item", GiveItem);
        dialogueRunner.AddCommandHandler<string>("remove_item", RemoveItem);
       // dialogueRunner.AddCommandHandler<string>("start_quest", StartQuest);
       // dialogueRunner.AddCommandHandler<string>("complete_quest", CompleteQuest);
    }

    private void GiveItem(string itemId, int amount)
    {
        Debug.Log("give_item: " + itemId + " x" + amount);

        for (int i = 0; i < amount; i++)
        {
            NetworkClient.Instance.Send("add-item", new AddItemRequest
            {
                itemId = itemId
            });
        }
    }

    private void RemoveItem(string itemId)
    {
        NetworkClient.Instance.Send("remove-item", new RemoveItemRequest
        {
            itemId = itemId
        });
    }

    /*private void StartQuest(string questId)
    {
        if (questManager != null)
            questManager.StartQuest(questId);
    }

    private void CompleteQuest(string questId)
    {
        if (questManager != null)
            questManager.CompleteQuest(questId);
    }*/
}