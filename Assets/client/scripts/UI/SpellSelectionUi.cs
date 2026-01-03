using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellSelectionUi : MonoBehaviour
{
    public SpellDatabase spellDatabase;
    public Button spellButtonPrefab;

    private LocalPlayer localPlayer;
    private readonly List<Button> buttons = new();

    void Start()
    {
        BuildSpellButtons();
    }

    void Update()
    {
        // LocalPlayer is spawned by server → bind late
        if (localPlayer == null)
        {
            localPlayer = PlayerManager.Instance.GetLocalPlayer();
            if (localPlayer != null && spellDatabase.spells.Count > 0)
            {
                SelectSpell(0);
            }
            return;
        }

        HandleHotkeys();
    }

    // =========================
    // BUILD UI FROM DATABASE
    // =========================
    void BuildSpellButtons()
    {
        for (int i = 0; i < spellDatabase.spells.Count; i++)
        {
            int index = i;
            SpellPrototype spell = spellDatabase.spells[i];

            Button btn = Instantiate(spellButtonPrefab, transform);
            buttons.Add(btn);

            Text text = btn.GetComponentInChildren<Text>();
            if (text != null)
                text.text = $"{index + 1}. {spell.prefabName}";

            btn.onClick.AddListener(() => SelectSpell(index));
        }
    }

    // =========================
    // HOTKEYS (1–9)
    // =========================
    void HandleHotkeys()
    {
        for (int i = 0; i < spellDatabase.spells.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSpell(i);
            }
        }
    }

    // =========================
    // SELECTION
    // =========================
    void SelectSpell(int index)
    {
        if (localPlayer == null)
            return;

        if (index < 0 || index >= spellDatabase.spells.Count)
            return;

        localPlayer.activeSpell = spellDatabase.spells[index];
        UpdateButtonHighlight(index);

        Debug.Log($"Selected spell: {spellDatabase.spells[index].prefabName}");
    }

    void UpdateButtonHighlight(int selectedIndex)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            ColorBlock colors = buttons[i].colors;
            colors.normalColor = (i == selectedIndex)
                ? Color.yellow
                : Color.white;
            buttons[i].colors = colors;
        }
    }
}
