using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TalentNodeClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TalentTreeRuntime runtime;
    public TalentNodeSO node;

    [Header("Overlay that shows when locked")]
    public GameObject disableObject;

    [Header("Tooltip root object name in the scene")]
    public string tooltipObjectName = "TALENTTOOLTIP";

    private GameObject tooltipObject;
    private RectTransform tooltipRectTransform;
    private TextMeshProUGUI tooltipTextPro;
    private TextMesh tooltipTextMesh;
    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip clickSound;
    
    

    private void Awake()
    {
        
        runtime = GameObject.Find("NETWORKCLIENT").GetComponent<TalentTreeRuntime>();
        tooltipObject = GameObject.Find(tooltipObjectName);
        if (tooltipObject == null)
        {
            Debug.LogError($"{tooltipObjectName} not found");
            return;
        }

        // Make tooltip ignore pointer so it does not break hover on the node
        var cg = tooltipObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = tooltipObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        tooltipRectTransform = tooltipObject.GetComponent<RectTransform>();
        tooltipTextPro = tooltipObject.GetComponentInChildren<TextMeshProUGUI>(true);
        tooltipTextMesh = tooltipObject.GetComponentInChildren<TextMesh>(true);
        tooltipRectTransform.pivot = new Vector2(-0.2f, 0.8f);

        //tooltipObject.SetActive(false);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void Update()
    {
        // Keep tooltip following mouse while it is visible
        if (tooltipObject != null && tooltipObject.activeSelf)
        {
            if (tooltipRectTransform != null)
            {
                tooltipRectTransform.position = Input.mousePosition;
            }
            else
            {
                tooltipObject.transform.position = Input.mousePosition;
            }
        }
    }

    private void ShowTooltip()
    {
        

        tooltipObject.SetActive(true);

        if (tooltipTextPro != null)
        {
            tooltipTextPro.text = node.description;
        }
        else if (tooltipTextMesh != null)
        {
            tooltipTextMesh.text = node.description;
        }
    }

    private void HideTooltip()
    {
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"CLICK on {gameObject.name} | button={eventData.button} | node={(node != null ? node.name : "NULL")} | runtime={(runtime != null ? "OK" : "NULL")}");

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            Debug.Log("Not left click, ignoring.");
            return;
        }

        if (runtime == null)
        {
            Debug.LogWarning("runtime is NULL on this TalentNodeClick.");
            return;
        }

        if (runtime.State == null)
        {
            Debug.LogWarning("runtime.State is NULL.");
            return;
        }

        if (node == null)
        {
            Debug.LogWarning("node is NULL on this TalentNodeClick.");
            return;
        }

        bool canUnlock = runtime.State.CanUnlock(node);
        Debug.Log($"CanUnlock({node.name}) = {canUnlock}");

        if (!canUnlock)
        {
            Debug.Log("Cannot unlock (requirements not met).");
            return;
        }

        runtime.TryUnlock(node);
        Debug.Log($"TryUnlock called for {node.name}. Now unlocked? {runtime.State.IsUnlocked(node)}");

        TalentNodeClick[] all = FindObjectsOfType<TalentNodeClick>(true);
        for (int i = 0; i < all.Length; i++)
        {
            all[i].Refresh();
        }
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    public void Refresh()
    {
        if (runtime == null || runtime.State == null || node == null)
        {
            return;
        }

        bool unlocked = runtime.State.IsUnlocked(node);
        Debug.Log("node "+node.name+" "+unlocked);

        if (disableObject != null)
        {
            Debug.Log("disabling object cover");
            disableObject.SetActive(!unlocked);
        }
        else
        {
            Debug.Log("object is null");
        }
    }
}