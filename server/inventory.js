import {dynamicObjectsManager} from "./dynamicObjectsManager.js";

export class inventory{
    constructor(playerid,emitCallback,holdMax){
        this.playerid = playerid;
        this.emit=emitCallback;

        this.itemslist="items/itemlist.json"
        this.holdmax=holdMax;
        this.items=[]
    }

    additem(itemId)
    {
        if(this.items.length<this.holdmax){
            this.items.push(itemId);
            const payload={
                id:this.playerid,
                itemId:itemId
            }
            console.log("[SERVER] Inventory additem:", itemId);
            this.emit('add-item', payload);
            return true;
        }
        else{
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

    removeitem(itemId) {
        const index = this.items.indexOf(itemId);

        if (index !== -1) {
            this.items.splice(index, 1);
            const payload={playerId:this.playerid, itemId:itemId};
            this.emit('remove-item', payload);
            return true;
        }
        console.log(this.items);
        return false;
    }

    removeItemNoEmit(itemId){
        const index = this.items.indexOf(itemId);
        if (index !== -1) {
            this.items.splice(index, 1);
            console.log("loot dropped :"+itemId+ " remaining inv: "+this.items);
            return true;
        }
        return false;
    }

    getItems(){
        return this.items;
    }
}
