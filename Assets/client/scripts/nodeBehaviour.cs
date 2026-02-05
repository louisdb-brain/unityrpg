using System;
using UnityEngine;

public class nodeBehaviour: MonoBehaviour
{
        private bool showInteractBool = false;
        public GameObject interactText;
        public Item item;

        

        public void Update()
        {
            
            if(interactText!=null)
            {
                interactText.SetActive(showInteractBool);
            }
            else
            {
                Debug.LogError("interactText is null "+gameObject.name );
            }

            if (Input.GetKeyDown(KeyCode.F) &&showInteractBool)
            {
                gameObject.GetComponent<LootSpawner>().spawnLoot(item);
            }
            showInteractBool = false;
        }

        public void ShowInteract()
        {
            showInteractBool = true;
        }
}
