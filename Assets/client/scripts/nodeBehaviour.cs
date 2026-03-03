using System;
using UnityEngine;

public class nodeBehaviour: MonoBehaviour
{
        private bool showInteractBool = false;
        private bool showFailedBool = false;
        public GameObject interactText;
        public GameObject notUnlockedText;
        public Item item;
        public TalentNodeSO talent;
        public TalentTreeState talentTreeState;

        

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
            if(notUnlockedText!=null)
            {
                notUnlockedText.SetActive(showFailedBool);
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
            showFailedBool = false;
        }

        public void ShowInteract()
        {
            if (talentTreeState.IsUnlocked(talent))
            {
                showFailedBool = false;
                showInteractBool = true;
            }
            else
            {
                showFailedBool = true;
                showInteractBool = false;
            }
            
            
            
        }
}
