using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class inventoryButton : MonoBehaviour,IPointerClickHandler
{
     public Image thisImage;
    public GameObject inventory;

    public void Start()
    {
        thisImage=GetComponent<Image>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("clicked");
        if (inventory != null )
        {
            Debug.Log("activating "+inventory.name);
            inventory.SetActive(!inventory.activeSelf);
            thisImage.enabled = !thisImage.enabled;
            
        }
    }
    
}
