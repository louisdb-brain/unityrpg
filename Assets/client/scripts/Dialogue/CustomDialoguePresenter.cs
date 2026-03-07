using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn;
using Yarn.Unity;

public class CustomDialoguePresenter : DialoguePresenterBase
{
    [Header("Panels")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Line")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private Button continueButton;

    [Header("Options")]
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private Button optionButtonPrefab;

    private bool continuePressed;

    private void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinuePressed);
    }

    private void OnContinuePressed()
    {
        continuePressed = true;
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        ClearOptions();

        if (speakerNameText != null)
            speakerNameText.text = string.Empty;

        if (lineText != null)
            lineText.text = string.Empty;

        return YarnTask.CompletedTask;
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken cancellationToken)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        string fullText = line.Text.Text;
        string speaker = "";
        string body = fullText;

        int colonIndex = fullText.IndexOf(':');
        if (colonIndex > 0)
        {
            speaker = fullText.Substring(0, colonIndex).Trim();
            body = fullText.Substring(colonIndex + 1).Trim();
        }

        if (speakerNameText != null)
            speakerNameText.text = speaker;

        if (lineText != null)
            lineText.text = body;

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);

        continuePressed = false;

        while (!continuePressed && !cancellationToken.NextLineToken.IsCancellationRequested)
        {
            await YarnTask.Yield();
        }
    }

    public override async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        ClearOptions();

        DialogueOption? selectedOption = null;

        for (int i = 0; i < dialogueOptions.Length; i++)
        {
            DialogueOption option = dialogueOptions[i];

            Button buttonInstance = Instantiate(optionButtonPrefab, optionsContainer);
            buttonInstance.gameObject.SetActive(true);

            TMP_Text buttonText = buttonInstance.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = option.Line.Text.Text;

            buttonInstance.onClick.AddListener(() =>
            {
                selectedOption = option;
            });
        }

        while (selectedOption == null && !cancellationToken.IsCancellationRequested)
        {
            await YarnTask.Yield();
        }

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        ClearOptions();

        return selectedOption;
    }

    private void ClearOptions()
    {
        if (optionsContainer == null)
            return;

        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(optionsContainer.GetChild(i).gameObject);
        }
    }
}