using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCCombatConfig
{
    public float detectionRange = 8f;
    public float meleeRange = 1.5f;
    public float preferredDistance = 2f;

    public float fleeHealthThreshold = 0.2f;
    public float retreatDistance = 2f;
    public float retreatCooldown = 2f;
    public float attackCooldown = 1.2f;

    public bool canMelee = true;

    public float bravery = 0.5f;
    public float retreatChance = 0.25f;

    public List<SpellPrototype> spells = new List<SpellPrototype>();
}
public enum NPCCombatState
{
    IdleRoam,
    Engage,
    Attack,
    Retreat,
    Flee,
    Block,
    Dodge
}
