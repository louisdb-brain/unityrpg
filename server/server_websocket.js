import { gamestateClass } from "./server_gamestate.js";
import { WebSocketServer } from "ws";
import {npc} from "./npc.js";


// --------------------
// CREATE WEBSOCKET SERVER
// --------------------
const wss = new WebSocketServer({ port: 3000 });

// --------------------------
// NETWORK INTERFACE (REPLACES io.emit)
// --------------------------
function broadcast(type, data) {
    const msg = JSON.stringify({ type, data });
    wss.clients.forEach(client => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(msg);
        }
    });
}

function sendTo(ws, type, data) {
    if (ws.readyState === WebSocket.OPEN) {
        ws.send(JSON.stringify({ type, data }));
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
    null,
    () => {},
    () => {},
    "loot",
    1
);

gamestate.addnpc(goblin);
// --------------------
// CLIENT CONNECTION HANDLER
// --------------------
wss.on("connection", (ws) => {
    console.log("Player connected");

    sendTo(ws, "welcome", { msg: "hello from server!" });

    ws.on("message", (raw) => {
        try {
            const msg = JSON.parse(raw);
            handleClientMessage(ws, msg);
        } catch (err) {
            console.error("Invalid message from client:", raw.toString());
        }
    });

    ws.on("close", () => {
        console.log("Player disconnected");
    });
});

// --------------------
// MESSAGE ROUTER
// --------------------
function handleClientMessage(ws, msg) {
    switch (msg.type) {

        case "move":
            // TODO integrate with playermanager
            break;

        case "chat":
            net.broadcast("chat", {
                id: ws.id,    // you’ll replace this with your player id later
                message: msg.data
            });
            break;

        case "spellcast":
            gamestate.castSpell(msg.data);
            break;

        default:
            console.warn("Unknown message type:", msg.type);
    }
}

console.log("WebSocket server running on ws://localhost:3000");
