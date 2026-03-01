using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Talents/Talent Node")]
public sealed class TalentNodeSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique id used for save data. Example: 'dash_1'")]
    public string nodeId;

    [Header("Display")]
    public string title;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Rules")]
    public int pointCost = 1;
    public List<TalentNodeSO> prerequisites = new List<TalentNodeSO>();

    [Header("Optional: Gameplay Hook")]
    public string effectId;
    public float effectValue;
}