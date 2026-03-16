
import {Spell} from "./spell.js";


export class SpellManager {
    constructor(npcManager, playerManager, net) {
        this.net = net;
        this.npcManager = npcManager;
        this.playerManager = playerManager;
        this.activeSpells = {};
    }

    castSpell(casterId, rawData) {
        const d = typeof rawData === "string"
            ? JSON.parse(rawData)
            : rawData;

        const spell = new Spell(
            d.spellId,
            casterId,
            d.prefabName,     // used as sprite/prefab id
            d.radius,
            d.damage,
            d.lifetime,
            d.position,
            d.direction,
            d.speed,
            d.knockback
        );

        this.activeSpells[d.spellId]=spell;

        this.net.broadcast("spell-spawn", {
            id: spell.id,
            caster: casterId,
            prefabName: d.prefabName,
            position: spell.position,
            direction: spell.direction,
            speed: spell.speed,
            radius: spell.radius,
            lifetime: spell.time
        });
    }


    update(delta) {
        for (const id in this.activeSpells) {
            const spell = this.activeSpells[id];

            if (!spell) continue;

            spell.update(delta);

            this.net.broadcast("spell-update", {
                id: spell.id,
                position: {
                    x: spell.position.x,
                    y: spell.position.y,
                    z: spell.position.z
                }
            });

            if (spell.time <= 0) {
                this.net.broadcast("spell-despawn", { id: spell.id });
                delete this.activeSpells[id];
            }
        }
    }



    collideSpell(id, targetId) {

        if (!this.npcManager.npcs[targetId]) {
            console.log("collided spell target not found");
            return;
        }

        if (!this.activeSpells[id]) {
            console.log("active spell doesn't exist");
            return;
        }

        const spell = this.activeSpells[id];
        const npc = this.npcManager.npcs[targetId];

        npc.takeDamage(spell.damage);
        npc.applyKnockback(spell.position,spell.knockback)

        this.net.broadcast("npc-takedamage", {
            id: targetId,
            amount: spell.damage
        });

        this.net.broadcast("spell-despawn", { id: spell.id });

        delete this.activeSpells[id];
    }
    checkCollisions(spell) {
        // Prevent multiple hits
        if (spell.dealtdamage) return false;

        // ---------- NPC COLLISIONS ----------
        for (const npcId in this.npcManager.npcs) {
            const npc = this.npcManager.npcs[npcId];

            const dx = spell.position.x - npc.position.x;
            const dz = spell.position.z - npc.position.z;
            const dist = Math.sqrt(dx * dx + dz * dz);

            if (dist <= spell.radius) {
                // Apply damage
                npc.takeDamage(spell.damage);

                // Notify clients
                this.net.broadcast("npc-takedamage", {
                    id: npcId,
                    amount: spell.damage
                });

                spell.dealtdamage = true;
                return true; // 🔥 hit → despawn spell
            }
        }

        // ---------- PLAYER COLLISIONS ----------
        const players = this.playerManager.getAllPlayers();
        for (const player of Object.values(players)) {

            // Optional: prevent self-hit
            if (player.id === spell.caster) continue;

            const dx = spell.position.x - player.position.x;
            const dz = spell.position.z - player.position.z;
            const dist = Math.sqrt(dx * dx + dz * dz);

            // Small extra radius for player hitbox
            if (dist <= spell.radius + 0.5) {
                this.net.broadcast("player-takedamage", {
                    id: player.id,
                    amount: spell.damage
                });

                spell.dealtdamage = true;
                return true; // 🔥 hit → despawn spell
            }
        }

        // No collision
        return false;
    }

}
