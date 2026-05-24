using UnityEngine;

public sealed class TalentTreeRuntime : MonoBehaviour
{
    public static TalentTreeRuntime Instance { get; private set; }

    //this class keeps tabs on the skilltree SCRIPTABLE OBJECT
    public TalentTreeSO tree;
    public int startingPoints = 5;
    public TalentTreeState State { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        State = new TalentTreeState(tree, startingPoints);
    }

    public bool TryUnlock(TalentNodeSO node)
    {
        return State != null && State.TryUnlock(node);
    }
}