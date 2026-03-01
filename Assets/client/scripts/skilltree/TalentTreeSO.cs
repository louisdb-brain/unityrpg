using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Talents/Single Talent Tree")]
public sealed class TalentTreeSO : ScriptableObject
{
    public List<TalentNodeSO> nodes = new List<TalentNodeSO>();

    public TalentNodeSO FindById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            TalentNodeSO node = nodes[i];
            if (node != null && node.nodeId == id)
            {
                return node;
            }
        }

        return null;
    }
}