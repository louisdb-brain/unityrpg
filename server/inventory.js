import {dynamicObjectsManager} from "./dynamicObjectsManager.js";
import {randInt} from "three/src/math/MathUtils.js";

export class inventory{
    constructor(playerid,emitCallback,holdMax){
        this.playerid = playerid;
        this.emit=emitCallback;

        this.itemslist="items/itemlist.json"
        this.holdmax=holdMax;
        this.items=[]
    }

    additem(itemname)
    {

        if(this.items.length<this.holdmax){
            this.items.push(itemname);
            const payload={
                id:this.playerid,
                name:itemname
            }
            console.log("[SERVER] Inventory additem:", itemname);
            this.emit('add-item', payload);
            return true;
        }
        else{
            //return false FOR DROPPING LOOT ON FLOOR;
            return false;
        }


    }
    emitInventory()
    {
        const payload={
            playerId:this.playerid,
            items:this.items
        }
        this.emit('emit-inventory',payload);
    }
    removeitem(itemName) {
        const index = this.items.findIndex(item => item.name === itemName);

        if (index !== -1) {
            this.items.splice(index, 1);
            this.emit('remove-item', itemName);
            return true;  // removed successfully
        }
        console.log(this.items);
        return false; // nothing removed
    }
    getItems(){
        return this.items;
    }
}