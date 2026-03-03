using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class TalentTreeState
{
    [SerializeField] private TalentTreeSO tree;
    [SerializeField] private int availablePoints;
    [SerializeField] private List<string> unlockedNodeIds = new List<string>();

    public TalentTreeSO Tree => tree;
    public int AvailablePoints => availablePoints;
    public IReadOnlyList<string> UnlockedNodeIds => unlockedNodeIds;

    public TalentTreeState(TalentTreeSO tree, int startingPoints)
    {
        this.tree = tree;
        availablePoints = Mathf.Max(0, startingPoints);
        unlockedNodeIds = new List<string>();
    }

   
    public bool IsUnlocked(TalentNodeSO node)
    {
        if (node == null || string.IsNullOrWhiteSpace(node.nodeId))
        {
            return false;
        }

        return unlockedNodeIds.Contains(node.nodeId);
    }

    public bool CanUnlock(TalentNodeSO node)
    {
        if (tree == null || node == null)
        {
            Debug.LogWarning("Can't unlock null node");
            return false;
        }

        if (string.IsNullOrWhiteSpace(node.nodeId))
        {
            Debug.LogWarning("Can't unlock emptyid node");
            return false;
        }

        if (IsUnlocked(node))
        {
            Debug.LogWarning("node unlocked");
            return false;
        }


        if (availablePoints < node.pointCost)
        {
            return false;
        }

        for (int i = 0; i < node.prerequisites.Count; i++)
        {
            TalentNodeSO prereq = node.prerequisites[i];
            if (!IsUnlocked(prereq))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryUnlock(TalentNodeSO node)
    {
        if (!CanUnlock(node))
        {
            return false;
        }

        availablePoints -= node.pointCost;
        unlockedNodeIds.Add(node.nodeId);
        Debug.Log("node unlocked"+node.nodeId);
        Debug.Log(unlockedNodeIds.Count);
        return true;
    }

    public void AddPoints(int amount)
    {
        availablePoints = Mathf.Max(0, availablePoints + amount);
    }
}