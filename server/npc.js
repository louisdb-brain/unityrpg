import * as THREE from "three";

export class npc {
    constructor(pNpcID, positionObj, pName, net, gamestate, loot, level) {
        this.net = net;
        this.gamestate = gamestate;

        this.npcid = pNpcID;
        this.name = pName;
        this.level = level;

        this.position = new THREE.Vector3(
            positionObj.x,
            positionObj.y,
            positionObj.z
        );

        // STATS
        this.health = 20;
        this.attack = 2;
        this.speed = 3;

        this.detectionRadius = 10;
        this.boidsRadius = 10;

        this.detectionsphere = new THREE.Sphere(
            this.position,
            this.detectionRadius
        );

        // TIMERS
        this.hitTime = 0;
        this.hitTimer = 13;

        // LOOT (STRINGS)
        this.loot = loot;
        this.rareloot = "mithrilsword";
        this.lootChance = 0.10;

        // MOVEMENT
        this.targetPosition = this.position.clone();
        this.angle = 0;

        this._destroyed = false;
    }

    update(delta, playersIgnored, allNpcs) {
        if (this._destroyed) return;
        if (this.hitTime > 0) this.hitTime--;

        this.move(delta);
    }

    move(delta) {
        if (this.hitTime > 0) return;

        const dir = new THREE.Vector3()
            .subVectors(this.targetPosition, this.position);

        if (dir.length() > 0.1) {
            dir.normalize();
            this.position.add(dir.multiplyScalar(this.speed * delta));
            this.angle = Math.atan2(dir.x, dir.z);
        }
    }

    takeDamage(amount) {
        if (this._destroyed) return;

        this.health -= amount;
        this.hitTime = this.hitTimer;

        this.net.broadcast("npc-takedamage", {
            id: this.npcid,
            amount,
            health: this.health
        });

        if (this.health <= 0) {
            this.destroy();
        }
    }

    destroy() {
        if (this._destroyed) return;
        this._destroyed = true;

        const isRare = Math.random() < this.lootChance;
        const itemName = isRare ? this.rareloot : this.loot;

        this.gamestate.objectManager.spawnLoot(
            itemName,
            this.position,
            this.level
        );

        this.net.broadcast("npc-kill", {
            id: this.npcid,
            name: this.name
        });
    }
}
