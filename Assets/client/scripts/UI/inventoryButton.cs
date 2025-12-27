using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class inventoryButton : MonoBehaviour,IPointerClickHandler
{
    [HideInInspector] public Image thisImage;
    public GameObject inventory;

    public void Start()
    {
        thisImage=GetComponent<Image>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventory != null )
        {
            inventory.SetActive(!inventory.activeSelf);
            thisImage.enabled = !thisImage.enabled;
            
        }
    }
    
}
