using UnityEngine;
using UnityEngine.EventSystems;

public class TalentNodeClick : MonoBehaviour, IPointerClickHandler
{
    public TalentTreeRuntime runtime;
    public TalentNodeSO node;

    [Header("Overlay that shows when locked")]
    public GameObject disableObject;

    private void OnEnable()
    {
        Refresh();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (runtime == null || runtime.State == null || node == null)
            return;

        if (!runtime.State.CanUnlock(node))
            return;

        runtime.TryUnlock(node);

        TalentNodeClick[] all = FindObjectsOfType<TalentNodeClick>(true);
        for (int i = 0; i < all.Length; i++)
            all[i].Refresh();
    }

    public void Refresh()
    {
        if (runtime == null || runtime.State == null || node == null)
            return;

        bool unlocked = runtime.State.IsUnlocked(node);

        if (disableObject != null)
            disableObject.SetActive(unlocked);
    }
}