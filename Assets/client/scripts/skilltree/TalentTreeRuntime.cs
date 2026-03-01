using UnityEngine;

public sealed class TalentTreeRuntime : MonoBehaviour
{
    public TalentTreeSO tree;
    public int startingPoints = 5;

    public TalentTreeState State { get; private set; }

    private void Awake()
    {
        State = new TalentTreeState(tree, startingPoints);
    }

    public bool TryUnlock(TalentNodeSO node)
    {
        return State != null && State.TryUnlock(node);
    }
}