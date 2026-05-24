using UnityEngine;

public class nodeBehaviour: MonoBehaviour
{
        private bool showInteractBool = false;
        private bool showFailedBool = false;
        public GameObject interactText;
        public GameObject notUnlockedText;
        public Item item;
        public TalentNodeSO talent;

        private TalentTreeState GetTalentState()
        {
            TalentTreeRuntime runtime = TalentTreeRuntime.Instance;
            if (runtime == null)
            {
                GameObject networkClient = GameObject.Find("NETWORKCLIENT");
                if (networkClient != null)
                {
                    runtime = networkClient.GetComponent<TalentTreeRuntime>();
                }
            }

            return runtime != null ? runtime.State : null;
        }

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
            Debug.Log("Node requires talent: " + talent.nodeId);
            Debug.Log("Copper unlocked? " + TalentTreeRuntime.Instance.State.IsUnlocked(talent));
            TalentTreeState state = GetTalentState();
            if (state != null && talent != null && state.IsUnlocked(talent))
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
