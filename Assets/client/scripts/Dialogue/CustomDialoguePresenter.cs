using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

    private CancellationTokenSource continueTokenSource;

    private void Awake()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        if (speakerNameText != null)
            speakerNameText.text = string.IsNullOrEmpty(line.CharacterName) ? "" : line.CharacterName;

        if (lineText != null)
            lineText.text = line.Text.Text;

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);

        // Wait for player to press Continue or for Yarn to request advancing
        continueTokenSource?.Cancel();
        continueTokenSource = new CancellationTokenSource();

        void OnContinue()
        {
            if (!continueTokenSource.IsCancellationRequested)
                continueTokenSource.Cancel();
        }

        continueButton.onClick.AddListener(OnContinue);

        try
        {
            while (!token.IsNextLineRequested && !continueTokenSource.IsCancellationRequested)
                await YarnTask.Yield();
        }
        finally
        {
            continueButton.onClick.RemoveListener(OnContinue);
        }
    }

    public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] options, CancellationToken cancellationToken)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(true);

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        // Clear old option buttons
        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
            Destroy(optionsContainer.GetChild(i).gameObject);

        DialogueOption selected = null;

        for (int i = 0; i < options.Length; i++)
        {
            DialogueOption option = options[i];

            Button b = Instantiate(optionButtonPrefab, optionsContainer);
            b.gameObject.SetActive(true);

            TMP_Text t = b.GetComponentInChildren<TMP_Text>();
            if (t != null)
                t.text = option.Line.Text.Text;

            b.onClick.AddListener(() => selected = option);
        }

        // Wait until a button is clicked or cancelled
        while (selected == null && !cancellationToken.IsCancellationRequested)
            await YarnTask.Yield();

        if (optionsPanel != null) optionsPanel.SetActive(false);
        return selected;
    }

    public override void DialogueComplete()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }
}