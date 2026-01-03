using System;
using UnityEngine;

/*
 * CLIENT → SERVER
 * Player requests to cast a spell
 */
[Serializable]
public class CastSpellPacket
{
    public string spellId;
    public string prefabName;

    public Vector3 position;
    public Vector3 direction;

    public float speed;
    public float radius;
    public int damage;
    public float lifetime;
}

/*
 * SERVER → CLIENT
 * Spawn a spell instance
 */
[Serializable]
public class SpellSpawnPacket
{
    public string id;
    public string caster;

    public string prefabName;   // ✅ ADD THIS

    public Vector3 position;
    public Vector3 direction;

    public float speed;
    public float lifetime;
    public float radius;
}


/*
 * SERVER → CLIENT
 * Continuous position update
 */
[Serializable]
public class SpellUpdatePacket
{
    public string id;
    public Vector3 position;
}

/*
 * SERVER → CLIENT
 * Remove spell instance
 */
[Serializable]
public class SpellDespawnPacket
{
    public string id;
}

/*
 * CLIENT → SERVER (OPTIONAL / PROTOTYPE)
 * Client reports a spell hit
 */
[Serializable]
public class SpellHitPacket
{
    public string spellId;
    public string targetId;
}


/*
 * SERVER → CLIENT
 * Server confirms damage
 */
[Serializable]
public class SpellDamagePacket
{
    public string spellId;
    public string targetId;
    public int amount;
}