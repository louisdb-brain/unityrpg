import { gamestateClass } from "./server_gamestate.js";
import { WebSocketServer } from "ws";
import {npc} from "./npc.js";
import {playermanager} from "./playermanager.js";
import { dynamicObjectsManager } from "./dynamicObjectsManager.js";
import crypto from "crypto";



// --------------------
// CREATE WEBSOCKET SERVER
// --------------------
const wss = new WebSocketServer({ port: 3000 });

// --------------------------
// NETWORK INTERFACE (REPLACES io.emit)
// --------------------------
function normalizeData(data) {
    // Your Unity client expects `data` to be a JSON string.
    return (typeof data === "string") ? data : JSON.stringify(data);
}

function broadcast(type, data) {
    const msg = JSON.stringify({ type, data: normalizeData(data) });
    wss.clients.forEach(client => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(msg);
        }
    });
}

function sendTo(ws, type, data) {
    if (ws.readyState === WebSocket.OPEN) {
        ws.send(JSON.stringify({ type, data: normalizeData(data) }));
    }
}


const net = { broadcast, sendTo };

// --------------------
// GAMESTATE
// --------------------
const gamestate = new gamestateClass(net);
gamestate.start();
// TEST SPAWN GOBLIN
const goblin = new npc(
    "goblin_1",
    { x: 0, y: 0, z: 0 },
    "goblin",
    net,
    gamestate,
    "butter",
    1
);

gamestate.addnpc(goblin);
// --------------------
// CLIENT CONNECTION HANDLER
// --------------------
wss.on("connection", (ws) => {
    ws.id = crypto.randomUUID(); // ✅ create authoritative id
    console.log("Player connected "+ws.id);

    sendTo(ws, "socket-id", JSON.stringify({ id: ws.id }));
    ws.on("message", (raw) => {
        try {
            let text;

            if (Buffer.isBuffer(raw)) {
                text = raw.toString("utf8");
            } else if (raw instanceof ArrayBuffer) {
                text = new TextDecoder().decode(raw);
            } else {
                text = raw.toString();
            }

            const msg = JSON.parse(text);
            handleClientMessage(ws, msg);

        } catch (err) {
            console.error("Invalid message from client:", err);
        }
    });


    ws.on("close", () => {
        console.log("Player disconnected");
        net.broadcast("player-left", { id: ws.id });
    });
});

// --------------------
// MESSAGE ROUTER
// --------------------
function handleClientMessage(ws, msg) {
    switch (msg.type) {
        case "create-player":
            // server decides the ID
            playermanager.addPlayer(
                ws.id,
                0,                    // level
                (type, data) => sendTo(ws, type, data)
            );

            net.broadcast("spawn-player", {
                id: ws.id,
                x: 0,
                y: 0,
                z: 0
            });
            console.log("data sent player spawned")
            break;

        case "player-move": {
            if (!msg.data) {
                console.warn("player-move missing data", msg);
                return;
            }

            const { x, y, z, angle } = JSON.parse(msg.data);

            const player = playermanager.getPlayer(ws.id);
            if (!player) return;

            player.position.set(x, y, z);
            player.targetPosition.set(x,y,z);
            player.angle = angle;
            console.log("player position", x, y, z);
            console.log(player.position)

            break;
        }


        case "chat":
            net.broadcast("chat", {
                id: ws.id,
                message: msg.data
            });
            break;

        case "spellcast":
            console.log("cast spell "+msg.data.id)
            gamestate.spellManager.castSpell(ws.id,msg.data);

            break;
        case "loot-spawn-request": {
            const data = typeof msg.data === "string"
                ? JSON.parse(msg.data)
                : msg.data;

            if (gamestate.objectManager.loot[data.id]) return;

            gamestate.objectManager.spawnLoot(
                data.id,
                data.itemName,
                data.position,
                data.level ?? 1
            );
            break;
        }
        case "loot-pickup": {
            const data = typeof msg.data === "string"
                ? JSON.parse(msg.data)
                : msg.data;

            gamestate.objectManager.pickupLoot(
                data.id,
                ws.id
            );
            break;
        }

        default:
            console.warn("Unknown message type:", msg.type);
    }
}

console.log("WebSocket server running on ws://localhost:3000");
