// SERVER NPC
import * as THREE from "three";
import crypto from "crypto";

const NPCCombatState = {
    IDLE: "idle",
    ENGAGE: "engage",
    ATTACK: "attack",
    RETREAT: "retreat",
    FLEE: "flee"
};

const NPCAttackChoice = {
    NONE: "none",
    MELEE: "melee",
    SPELL: "spell"
};

export class npc {
    constructor(pNpcID, positionObj, pName, net, objectManager, loot, rareloot, level) {
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

        this.spawnPosition = this.position.clone();

        // === STATS ===
        this.maxHealth = 30;
        this.health = 30;
        this.attack = 2;
        this.speed = 3;
        this.attackspeed = 3;

        this.knockback = new THREE.Vector3(0, 0, 0);
        this.knockbackDecay = 0.85;

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

        this.attackCooldownTimer = 0;
        this.retreatCooldownTimer = 0;
        this.stateTimer = 0;

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

        // === COMBAT STATE ===
        this.combatState = NPCCombatState.IDLE;
        this.attackChoice = NPCAttackChoice.NONE;
        this.pendingSpell = null;

        // === COMBAT CONFIG ===
        this.combatConfig = {
            detectionRange: this.detectionRadius,
            meleeRange: this.attackRadius,
            preferredDistance: 2,
            fleeHealthThreshold: 0.2,
            retreatDistance: 2,
            retreatCooldown: 50,
            attackCooldown: 50,
            canMelee: true,
            canBlock: false,
            canDodge: false,
            bravery: 0.5,
            retreatChance: 0.25,
            spells: []
        };

        this._destroyed = false;
    }

    update(delta, players, allNpcs) {
        if (this._destroyed) return;

        this.tickTimers();
        if(this.net)
        {
            this.net.broadcast("DEBUG-npcstate",{
                id:this.npcid,
                state:this.combatState
            })
        }
        switch (this.combatState) {
            case NPCCombatState.IDLE:
                this.tickIdle(delta, players);
                break;

            case NPCCombatState.ENGAGE:
                this.tickEngage(players);
                break;

            case NPCCombatState.ATTACK:
                this.tickAttack(players);
                break;

            case NPCCombatState.RETREAT:
                this.tickRetreat(players);
                break;

            case NPCCombatState.FLEE:
                this.tickFlee(players);
                break;

            default:
                this.combatState = NPCCombatState.IDLE;
                break;
        }

        this.checkAvoid(allNpcs);
        this.move(delta);
    }

    tickTimers() {
        if (this.hitTime > 0) this.hitTime--;
        if (this.attackCooldownTimer > 0) this.attackCooldownTimer--;
        if (this.retreatCooldownTimer > 0) this.retreatCooldownTimer--;
        if (this.stateTimer > 0) this.stateTimer--;
    }

    setTarget(position) {
        const temppos = position.clone();
        temppos.y = 0;
        this.targetPosition.copy(temppos);
    }

    move(delta) {
        // apply knockback first
        if (this.knockback.lengthSq() > 0.0001) {
            this.position.add(this.knockback.clone().multiplyScalar(delta));
            this.knockback.multiplyScalar(this.knockbackDecay);
            this.detectionsphere.center.copy(this.position);
            return;
        }

        if (this.hitTime > 0) {
            this.detectionsphere.center.copy(this.position);
            return;
        }

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

    applyKnockback(fromPosition, force = 8) {
        const dir = new THREE.Vector3()
            .subVectors(this.position, fromPosition)
            .normalize();

        this.knockback.copy(dir.multiplyScalar(force));
    }

    // ==========================
    // TARGETING
    // ==========================
    getTargetPlayer(players) {
        let closest = null;
        let closestDistance = Infinity;

        for (const id in players) {
            const player = players[id];
            if (!player || !player.position) continue;

            const pos = new THREE.Vector3(
                player.position.x,
                player.position.y,
                player.position.z
            );

            const distance = this.position.distanceTo(pos);

            if (distance <= this.combatConfig.detectionRange && distance < closestDistance) {
                closest = {
                    id,
                    player,
                    position: pos
                };
                closestDistance = distance;
            }
        }

        return closest;
    }

    // Kept for compatibility / transition.
    // Now it only acquires a target and does not force chase movement.
    checkFollow(players) {
        const target = this.getTargetPlayer(players);

        if (target) {
            this.targetPlayerId = target.id;
            return true;
        }

        this.targetPlayerId = null;
        return false;
    }

    // ==========================
    // COMBAT STATES
    // ==========================
    tickIdle(delta, players) {
        const foundTarget = this.checkFollow(players);

        if (foundTarget) {
            this.combatState = NPCCombatState.ENGAGE;
            return;
        }

        this.attackChoice = NPCAttackChoice.NONE;
        this.pendingSpell = null;

        this.aiupdate(delta);
    }

    tickEngage(players) {
        const target = this.getTargetPlayer(players);

        if (!target) {
            this.targetPlayerId = null;
            this.attackChoice = NPCAttackChoice.NONE;
            this.pendingSpell = null;
            this.combatState = NPCCombatState.IDLE;
            return;
        }

        this.targetPlayerId = target.id;

        const distance = this.position.distanceTo(target.position);
        const healthPercent = this.maxHealth > 0 ? this.health / this.maxHealth : 0;

        if (healthPercent <= this.combatConfig.fleeHealthThreshold) {
            this.combatState = NPCCombatState.FLEE;
            return;
        }

        if (this.shouldRetreat(distance)) {
            this.combatState = NPCCombatState.RETREAT;
            this.stateTimer = 12;
            this.retreatCooldownTimer = this.combatConfig.retreatCooldown;
            return;
        }

        if (this.tryChooseAttack(distance)) {
            this.combatState = NPCCombatState.ATTACK;
            this.stateTimer = 10;
            return;
        }

        this.moveToCombatPosition(target.position, distance);
    }

    tickAttack(players) {
        const target = this.getTargetPlayer(players);

        if (!target) {
            this.targetPlayerId = null;
            this.attackChoice = NPCAttackChoice.NONE;
            this.pendingSpell = null;
            this.combatState = NPCCombatState.IDLE;
            return;
        }

        this.targetPlayerId = target.id;

        // stop while winding up
        this.setTarget(this.position.clone());

        if (this.stateTimer > 0) {
            return;
        }

        switch (this.attackChoice) {
            case NPCAttackChoice.MELEE:
                this.performMeleeAttack(players);
                this.attackCooldownTimer = this.combatConfig.attackCooldown;
                break;

            case NPCAttackChoice.SPELL:
                this.performSpellAttack(players);
                break;
        }

        this.attackChoice = NPCAttackChoice.NONE;
        this.pendingSpell = null;
        this.combatState = NPCCombatState.ENGAGE;
    }

    tickRetreat(players) {
        const target = this.getTargetPlayer(players);

        if (!target) {
            this.targetPlayerId = null;
            this.combatState = NPCCombatState.IDLE;
            return;
        }

        this.targetPlayerId = target.id;

        const away = this.position.clone().sub(target.position);
        away.y = 0;

        if (away.lengthSq() <= 0.0001) {
            away.set(1, 0, 0);
        } else {
            away.normalize();
        }

        const retreatTarget = this.position.clone().add(
            away.multiplyScalar(this.combatConfig.retreatDistance)
        );

        this.setTarget(retreatTarget);

        if (this.stateTimer <= 0) {
            this.combatState = NPCCombatState.ENGAGE;
        }
    }

    tickFlee(players) {
        const target = this.getTargetPlayer(players);

        if (!target) {
            this.targetPlayerId = null;
            this.setTarget(this.spawnPosition.clone());
            this.combatState = NPCCombatState.IDLE;
            return;
        }

        this.targetPlayerId = target.id;

        const distance = this.position.distanceTo(target.position);

        if (distance > this.combatConfig.detectionRange * 1.5) {
            this.targetPlayerId = null;
            this.setTarget(this.spawnPosition.clone());
            this.combatState = NPCCombatState.IDLE;

            return;
        }

        const away = this.position.clone().sub(target.position);
        away.y = 0;

        if (away.lengthSq() <= 0.0001) {
            away.set(1, 0, 0);
        } else {
            away.normalize();
        }

        const fleeTarget = this.position.clone().add(away.multiplyScalar(8));
        this.setTarget(fleeTarget);
    }

    // ==========================
    // COMBAT DECISIONS
    // ==========================
    shouldRetreat(distance) {
        if (this.retreatCooldownTimer > 0) return false;
        if (distance > this.combatConfig.meleeRange * 0.8) return false;

        return Math.random() < this.combatConfig.retreatChance;
    }

    tryChooseAttack(distance) {
        this.attackChoice = NPCAttackChoice.NONE;
        this.pendingSpell = null;

        if (
            this.combatConfig.canMelee &&
            this.attackCooldownTimer <= 0 &&
            distance <= this.combatConfig.meleeRange
        ) {
            this.attackChoice = NPCAttackChoice.MELEE;
            return true;
        }

        const spell = this.tryChooseSpell(distance);
        if (spell) {
            this.pendingSpell = spell;
            this.attackChoice = NPCAttackChoice.SPELL;
            return true;
        }

        return false;
    }

    tryChooseSpell(distance) {
        if (!this.combatConfig.spells || this.combatConfig.spells.length === 0) {
            return null;
        }

        for (const spell of this.combatConfig.spells) {
            if (!spell) continue;

            // SpellPrototype has prefabName and combat stats already on the Unity side.
            // For now we only use radius if it exists as a rough range fallback.
            if (spell.range != null && distance > spell.range) continue;

            return spell;
        }

        return null;
    }

    moveToCombatPosition(targetPosition, distance) {
        const preferred = this.combatConfig.preferredDistance;

        if (distance > preferred + 0.5) {
            this.setTarget(targetPosition);
            return;
        }

        if (distance < preferred - 0.5) {
            const away = this.position.clone().sub(targetPosition);
            away.y = 0;

            if (away.lengthSq() <= 0.0001) {
                away.set(1, 0, 0);
            } else {
                away.normalize();
            }

            const stepBackTarget = this.position.clone().add(away.multiplyScalar(1.5));
            this.setTarget(stepBackTarget);
            return;
        }

        this.setTarget(this.position.clone());
    }

    // ==========================
    // ATTACK EXECUTION
    // ==========================
    performMeleeAttack(players) {
        if (!this.targetPlayerId) return;

        const player = players[this.targetPlayerId];
        if (!player || !player.position) return;

        const playerPos = new THREE.Vector3(
            player.position.x,
            player.position.y,
            player.position.z
        );

        const distance = this.position.distanceTo(playerPos);
        if (distance > this.combatConfig.meleeRange) return;

        if (typeof player.takeDamage === "function") {
            player.takeDamage(this.attack);
        }

        if (this.net) {
            this.net.broadcast("npc-attack", {
                id: this.npcid,
                name: this.name,
                targetId: this.targetPlayerId,
                type: "melee",
                damage: this.attack
            });
        }
    }

    performSpellAttack(players) {
        if (!this.pendingSpell) return;

        if (this.net) {
            this.net.broadcast("npc-cast", {
                id: this.npcid,
                name: this.name,
                targetId: this.targetPlayerId,
                prefabName: this.pendingSpell.prefabName || null
            });
        }

        this.attackCooldownTimer = this.combatConfig.attackCooldown;
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
        this.attackChoice = NPCAttackChoice.NONE;
        this.pendingSpell = null;

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