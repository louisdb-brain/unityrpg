// SERVER NPC
import { toVec3 } from "./utilities.js";
import * as THREE from "three";

export class npc {
    constructor(pNpcID, positionObj, pName, net, onDestroy, spawnCallback, loot, level) {
        this.net = net;
        this.npcid = pNpcID;

        this.position = new THREE.Vector3(positionObj.x, positionObj.y, positionObj.z);
        this.level = level;
        this.name = pName;

        this.health = 10;
        this.attack = 2;
        this.detectionRadius = 10;
        this.boidsRadius = 10;
        this.attackRadius = 3;
        this.hitboxRadius = 1.5;

        this.detectionsphere = new THREE.Sphere(this.position, this.detectionRadius);
        this.onDestroy = onDestroy;

        this.hitTime = 0;
        this.hitTimer = 13;
        this.speed = 3;
        this.attackspeed = 3;
        this.loot = loot;
        this.rareloot = "mithrilsword";
        this.spawnCallback = spawnCallback;

        this.cooldown = 50;
        this.targetPosition = this.position.clone();

        // Player target (disabled until playermanager is integrated)
        this.targetPlayerId = null;

        this.decisiontimer = 0;
        this.decisiontreshhold = 20;

        this.angle = 0;
    }

    update(delta, playersIgnored, allNpcs) {
        if (this._destroyed) return;

        if (this.hitTimer > 0) this.hitTime--;

        this.aiupdate(delta);

        // FOLLOW PLAYER → DISABLED
        // this.checkFollow(playersIgnored);

        // COMBAT → DISABLED
        // this.handleCombat(playersIgnored);

        this.checkAvoid(allNpcs);
        this.move(delta);
    }

    setTarget(position) {
        let temppos = position.clone();
        temppos.y = 0;
        this.targetPosition.copy(temppos);
    }

    move(delta) {
        if (this.hitTime > 0) return;

        const direction = new THREE.Vector3().subVectors(this.targetPosition, this.position);
        const distance = direction.length();

        if (distance > 0.1) {
            direction.normalize();
            const step = this.speed * delta;
            this.position.add(direction.clone().multiplyScalar(step));
            this.angle = Math.atan2(direction.x, direction.z);
        }

        this.detectionsphere.center.copy(this.position);
    }

    // FOLLOWING PLAYERS — DISABLED
    /*
    checkFollow(players) {
        for (const id in players) {
            const player = players[id];
            const pos = new THREE.Vector3(player.position.x, player.position.y, player.position.z);
            if (this.detectionsphere.containsPoint(pos)) {
                this.targetPlayerId = id;
                this.setTarget(pos);
                return;
            }
        }
        this.targetPlayerId = null;
    }
    */

    checkAvoid(allNpcs) {
        let avoid = new THREE.Vector3(0, 0, 0);
        let count = 0;

        for (const other of allNpcs) {
            if (other === this) continue;

            const dist = this.position.distanceTo(other.position);
            if (dist < this.boidsRadius && dist > 0.001) {
                let push = this.position.clone().sub(other.position);
                push.normalize().divideScalar(dist) * this.boidsRadius;
                avoid.add(push);
                count++;
            }
        }

        if (count > 0) {
            avoid.divideScalar(count);
            this.targetPosition.add(avoid);
        }
    }

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

    // COMBAT — DISABLED UNTIL PLAYER SYSTEM EXISTS
    /*
    handleCombat(players) {
        if (!this.targetPlayerId) return;
        const player = players[this.targetPlayerId];
        if (!player) return;

        const dist = this.position.distanceTo(new THREE.Vector3(player.position.x, player.position.y, player.position.z));
        if (dist <= this.attackRadius) {
            this.performAttack(player);
        }
    }

    performAttack(player) {
        if (this.cooldown > 0) {
            this.cooldown -= this.attackspeed;
            return;
        }
        this.cooldown = 50;
        player.takeDamage(this.attack);

        this.net.broadcast("npc-attack", { npc: this.npcid });
    }
    */

    takeDamage(amount) {
        this.health -= amount;
        this.hitTime = this.hitTimer;

        // Broadcast damage
        this.net.broadcast("npc-takedamage", {
            id: this.npcid,
            name: this.name,
            amount,
            health: this.health
        });

        if (this.health <= 0) this.destroy();
    }

    destroy() {
        if (this._destroyed) return;
        this._destroyed = true;
        this.targetPlayerId = null;

        const lootId = `${this.name}_loot_${Date.now()}_${Math.floor(Math.random() * 10000)}`;
        this.spawnCallback(lootId, this.loot, this.position, this.level);

        console.log(`NPC ${this.name} (${this.npcid}) destroyed`);

        this.net.broadcast("npc-kill", { id: this.npcid, name: this.name });

        if (typeof this.onDestroy === "function") {
            this.onDestroy(this);
        }
    }
}
