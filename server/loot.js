export class loot {
    constructor(id, itemId, level, position, net) {
        this.id = id;
        this.itemId = itemId;
        this.level = level;
        this.position = { ...position };
        this.net = net;

        this.broadcastSpawn();
    }

    broadcastSpawn() {
        if (!this.net) return;
        console.log("BroadcastSpawn", this.itemId);
        this.net.broadcast("loot-spawn", {
            id: this.id,
            itemId: this.itemId,
            position: this.position,
            level: this.level
        });
    }
}
