using System;
using UnityEngine;
using Yarn.Unity;

public class DialogueStarter : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string startNode = "TutorialQuest";
    private bool showInteractBool = false;
    public GameObject interactText;

    private void Start()
    {
        dialogueRunner = GameObject.Find("Dialogue_ui").GetComponent<DialogueRunner>();
        Debug.Log("DIALOGUERUNNER IS "+dialogueRunner);
    }

    private void Update()
    {
        if (showInteractBool)
        {
            interactText.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.E) && !dialogueRunner.IsDialogueRunning &&showInteractBool)
        {
            Debug.Log("speaking");
            Debug.Log("conversation started :"+startNode );
            dialogueRunner.StartDialogue(startNode);
        }
        
    }

    public void ShowInteract()
    {
        Debug.Log("show interact "+gameObject.name);
        showInteractBool = true;
    }
}