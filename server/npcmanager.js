import { npc } from "./npc.js";
import { playermanager } from "./playermanager.js";
import { objectManager } from "./dynamicObjectsManager.js";
import { loot } from "./loot.js";
import { QuestGiver } from "./questgiver.js";

export class npcManager {
    constructor(objectmanager, net) {
        this.spawnCallback = null;
        this.npcs = {};
        this.objectmanager = objectmanager;
        this.net = net;
        this.respawnQueue = {};
    }

    update(delta) {
        const npcs = Object.values(this.npcs);
        const players = playermanager.getAllPlayers();

        this.updateRespawns();

        for (const npc of npcs) {
            if (!npc) {
                console.warn("⚠️ Skipping undefined NPC entry in npcManager");
                continue;
            }

            if (npc._destroyed) {
                this.removeNPC(npc);
                continue;
            }

            try {
                npc.update(delta, players, npcs);
            } catch (err) {
                console.error(`💥 Error updating NPC '${npc.name}' (${npc.npcid}):`, err);
            }
        }
    }



    addNpc(pNPC) {
        if (!this.npcs[pNPC.npcid]) {
            this.npcs[pNPC.npcid] = pNPC;
        }
    }

    getNpcList() {
        return this.npcs;
    }

    removeNPC(npcOrId) {
        const id = typeof npcOrId === "string" ? npcOrId : npcOrId.npcid;
        const npcInstance = this.npcs[id];
        if (!npcInstance) return;

        console.log(`Removing NPC ${id} and adding it to the queue`);

        this.respawnQueue[npcInstance.npcid] = {
            id: npcInstance.npcid,
            name: npcInstance.name,
            loot: npcInstance.loot,
            level: npcInstance.level,
            respawnTime: Date.now() + 200
        };

        delete this.npcs[id];
    }

    updateRespawns() {
        const now = Date.now();
        if (Object.keys(this.respawnQueue).length === 0) {}

        for (const npcid in this.respawnQueue) {
            const data = this.respawnQueue[npcid];

            if (data.respawnTime <= now) {
                if (this.npcs[npcid]) {
                    console.warn(
                        `⚠️ Tried to respawn NPC ${npcid}, but it already exists! Skipping.`
                    );
                    delete this.respawnQueue[npcid];
                    continue;
                }

                console.log(`✨ Respawning NPC ${npcid}`);

                const newNpc = new npc(
                    data.id,
                    { x: 0, y: 0, z: 0 },
                    data.name,
                    this.net,
                    data.loot,
                    data.level
                );

                console.log(newNpc);

                this.addNpc(newNpc);
                delete this.respawnQueue[npcid];
            }
        }
    }

    getNpc(pID) {
        return this.npcs[pID];
    }
}
