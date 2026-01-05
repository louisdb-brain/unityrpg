// SERVER NPC
import * as THREE from "three";
import crypto from "crypto";

export class npc {
    constructor(pNpcID, positionObj, pName, net, objectManager, loot,rareloot, level) {
        // === CORE ===
        this.net = net;
        this.objectManager = objectManager;

        this.npcid = pNpcID;
        this.name = pName;
        this.level = level;

        this.position = new THREE.Vector3(
            positionObj.x,
            positionObj.y,
            positionObj.z
        );

        // === STATS ===
        this.health = 10;
        this.attack = 2;
        this.speed = 3;
        this.attackspeed = 3;

        this.detectionRadius = 10;
        this.boidsRadius = 10;
        this.attackRadius = 3;
        this.hitboxRadius = 1.5;

        this.detectionsphere = new THREE.Sphere(
            this.position,
            this.detectionRadius
        );

        // === TIMERS ===
        this.hitTime = 0;
        this.hitTimer = 13;

        // === LOOT ===
        this.loot = loot;
        this.rareloot = rareloot;
        this.lootChance = 0.10;

        // === AI / MOVEMENT ===
        this.targetPosition = this.position.clone();
        this.targetPlayerId = null;

        this.decisiontimer = 0;
        this.decisiontreshhold = 20;

        this.cooldown = 50;
        this.angle = 0;

        this._destroyed = false;
    }

    update(delta, playersIgnored, allNpcs) {
        if (this._destroyed) return;

        if (this.hitTime > 0) this.hitTime--;

        this.aiupdate(delta);

        // FOLLOW PLAYER (still disabled by design)
        this.checkFollow(playersIgnored);

        // COMBAT (disabled)
        // this.handleCombat(playersIgnored);

        // BOIDS AVOIDANCE
        this.checkAvoid(allNpcs);

        // MOVE
        this.move(delta);
    }

    setTarget(position) {
        const temppos = position.clone();
        temppos.y = 0;
        this.targetPosition.copy(temppos);
    }

    move(delta) {
        if (this.hitTime > 0) return;

        const direction = new THREE.Vector3()
            .subVectors(this.targetPosition, this.position);

        const distance = direction.length();

        if (distance > 0.1) {
            direction.normalize();
            const step = this.speed * delta;
            this.position.add(direction.clone().multiplyScalar(step));
            this.angle = Math.atan2(direction.x, direction.z);
        }

        this.detectionsphere.center.copy(this.position);
    }

    // ==========================
    // FOLLOW PLAYERS (DISABLED)
    // ==========================
    checkFollow(players) {
        for (const id in players) {
            const player = players[id];
            const pos = new THREE.Vector3(
                player.position.x,
                player.position.y,
                player.position.z
            );

            if (this.detectionsphere.containsPoint(pos)) {
                this.targetPlayerId = id;
                this.setTarget(pos);
                return;
            }
        }
        this.targetPlayerId = null;
    }

    // ==========================
    // BOIDS AVOIDANCE
    // ==========================
    checkAvoid(allNpcs) {
        let avoid = new THREE.Vector3(0, 0, 0);
        let count = 0;

        for (const other of allNpcs) {
            if (!other || other === this) continue;

            const dist = this.position.distanceTo(other.position);
            if (dist < this.boidsRadius && dist > 0.001) {
                const push = this.position.clone().sub(other.position);
                push.normalize().divideScalar(dist);
                avoid.add(push);
                count++;
            }
        }

        if (count > 0) {
            avoid.divideScalar(count);
            this.targetPosition.add(avoid);
        }
    }

    // ==========================
    // AI WANDERING
    // ==========================
    aiupdate(delta) {
        if (this.decisiontimer < this.decisiontreshhold) {
            this.decisiontimer++;
            return;
        }

        this.decisiontimer = 0;

        const randomVec = new THREE.Vector3(
            Math.random() * 20 - 10,
            0,
            Math.random() * 20 - 10
        );

        const targetpos = randomVec.clone().add(this.position);
        this.setTarget(targetpos);
    }

    // ==========================
    // DAMAGE / DEATH
    // ==========================
    takeDamage(amount) {
        if (this._destroyed) return;

        this.health -= amount;
        this.hitTime = this.hitTimer;

        if (this.net) {
            this.net.broadcast("npc-takedamage", {
                id: this.npcid,
                name: this.name,
                amount,
                health: this.health
            });
        }

        if (this.health <= 0) this.destroy();
    }

    destroy() {
        if (this._destroyed) return;
        this._destroyed = true;

        this.targetPlayerId = null;

        // === SPAWN LOOT (FIXED) ===
        if (
            this.objectManager &&
            typeof this.objectManager.spawnLoot === "function"
        ) {
            const isRare = Math.random() < this.lootChance;
            const itemName = isRare ? this.rareloot : this.loot;
            const lootId = crypto.randomUUID();

            this.objectManager.spawnLoot(
                lootId,
                itemName,
                this.position,
                this.level
            );
        } else {
            console.error(
                "NPC destroy(): invalid objectManager",
                this.objectManager
            );
        }

        console.log(`NPC ${this.name} (${this.npcid}) destroyed`);

        if (this.net) {
            this.net.broadcast("npc-kill", {
                id: this.npcid,
                name: this.name
            });
        }
    }
}
