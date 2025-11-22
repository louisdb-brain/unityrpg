import { npcManager } from "./npcmanager.js";
import * as THREE from 'three';
import { playermanager } from "./playermanager.js";
import { objectManager } from "./dynamicObjectsManager.js";
import { SpellManager } from "./spellmanager.js";

export class gamestateClass {
    constructor(net) {
        // net = { broadcast(type,data), sendTo(ws,type,data) }
        this.net = net;

        this.ticks = 0;
        this.clock = new THREE.Clock();

        this.maps = {};
        this.maps[1] = true;

        this.objectManager = new objectManager();
        this.npcManager = new npcManager(this.objectManager, this.net);
        this.spellManager = new SpellManager(
            this.npcManager,
            playermanager,
            "placeholdersocketid",
            this.net
        );

        this.onNpcUpdate = null;
    }

    start() {
        let lastTime = Date.now();

        setInterval(() => {
            const now = Date.now();
            const delta = (now - lastTime) / 1000;
            lastTime = now;

            this.tick(delta);

            this.ticks++;
            if (this.ticks % 60 === 0) {
                console.log("Server tick:", this.ticks);
            }
        }, 50); // 20 FPS
    }

    tick(delta) {
        if (!this.maps[1]) return;

        //playermanager.update(delta);
        this.npcManager.update(delta);
        this.emitNpc();
        //this.emitPlayers();
        //this.objectManager.update(delta);
        //this.emitNodes();

        //this.spellManager.update();
    }

    loot(loot) {
        console.log("addloot called :");
        this.objectManager.addloot(loot, loot.id);
    }

    removeloot(id) {
        this.objectManager.removeloot(id);
    }

    addnpc(pNPC) {
        this.npcManager.addNpc(pNPC);
        this.emitNpc();
    }

    emitNodes() {
        for (const key in this.objectManager.nodes) {
            const node = this.objectManager.nodes[key];
            if (node && typeof node.emitNode === "function") {
                node.emitNode(); // each node will use net.broadcast inside itself
            }
        }
    }

    emitPlayers() {
        const playerMap = playermanager.getAllPlayers();

        const payload = Object.values(playerMap).map(player => ({
            id: player.id,
            pos: { ...player.position },
            targetpos: { ...player.targetPosition },
            lockedpos: { ...player.lockedPosition },
            angle: player.angle,
            locked: player.locked,
            level: player.level
        }));

        this.net.broadcast("player-positionupdate", payload);
    }

    emitNpc() {
        const npcMap = this.npcManager.getNpcList();

        const payload = Object.values(npcMap).map(npc => ({
            id: npc.npcid,
            npcType: npc.name,
            position: { ...npc.position },
            targetPosition: { ...npc.targetPosition },
            angle: npc.angle,
            health: npc.health,
            target: npc.targetPlayerId,
            level: npc.level
        }));

        this.net.broadcast("npc-position-update", JSON.stringify(payload));
    }





    returnPos(pObj) {
        return { x: pObj.position.x, y: pObj.position.y, z: pObj.position.z };
    }
/*
    castSpell(data) {
        this.spellManager.castSpell("noid", data);

        console.log(data.sprite);
        this.net.broadcast("spellcast", data);
    }*/
}
