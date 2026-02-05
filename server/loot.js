export class loot {
    constructor(id, name, level, position, net) {
        this.id = id;
        this.name = name;
        this.level = level;
        this.position = { ...position };
        this.net = net;

        this.broadcastSpawn();
    }

    broadcastSpawn() {
        if (!this.net) return;
        console.log("BroadcastSpawn");
        this.net.broadcast("loot-spawn", {
            id: this.id,
            itemName: this.name,
            position: this.position,
            level: this.level
        });

    }
}
