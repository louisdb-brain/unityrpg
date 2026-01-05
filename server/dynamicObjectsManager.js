import { loot } from "./loot.js";
import { playermanager } from "./playermanager.js";

export class dynamicObjectsManager {
    constructor(net) {
        this.net = net;
        this.loot = {};
        this.chests = {};
        this.nodes = {};
    }

    // =========================
    // LOOT
    // =========================

    spawnLoot(id, itemName, position, level ) {
        if (this.loot[id]) return; // dedupe safety

        const lootObj = new loot(
            id,
            itemName,
            level,
            position,
            this.net
        );

        this.loot[id] = lootObj;
    }

    pickupLoot(id, socketid) {
        const lootObj = this.loot[id];
        if (!lootObj) return;

        playermanager.additem(socketid, lootObj.name);
        delete this.loot[id];

        this.net.broadcast("loot-picked", { id });
    }

    // =========================
    // CHESTS
    // =========================

    addChest(chest, id) {
        const chestId = id ?? chest.id;
        if (!chestId || this.chests[chestId]) return;
        this.chests[chestId] = chest;
    }

    openChest(id, socketid) {
        const chest = this.chests[id];
        if (!chest) return;
        chest.open(socketid, this);
    }

    // =========================
    // NODES
    // =========================

    addNode(node, id) {
        const nodeId = id ?? node.id;
        if (!nodeId || this.nodes[nodeId]) return;
        this.nodes[nodeId] = node;
    }

    clickNode(id, socketid) {
        const node = this.nodes[id];
        if (!node) return;
        node.click(socketid, this);
    }
}
