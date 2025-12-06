using System;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public item data;       // ✅ ScriptableObject lives here
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        SetItem(data);
    }

    private void OnValidate()
    {
        if(image == null)
            image = GetComponent<Image>();
        if(data !=null)
            SetItem(data);
    }

    public void SetItem(item newItem)
    {
        data = newItem;

        if (data != null)
            image.sprite = data.icon;
        else
            image.sprite = null;
    }
}