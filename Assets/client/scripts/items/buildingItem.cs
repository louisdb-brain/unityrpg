using System.Collections.Generic;
using UnityEngine;
using System;
public enum BuildCategory
{
    Outdoor,
    Indoor,
    Upgrade
}


[Flags]
public enum BuildPropertyType
{
    None   = 0,
    Base =1 <<0, 
    Window = 1 << 1,
    Door   = 1 << 2,
    Roof   = 1 << 3,
    Wall   = 1 << 4,
    Plant  = 1 << 5,
    Decoration = 1 << 6,
    anvil=1<<0,
    
}



[CreateAssetMenu(fileName = "NewBuildingItem", menuName = "Game/Items/Building item")]
public class buildingItem : Item
{
    [Header("build properties")]
    public BuildCategory buildCategory;
    public BuildPropertyType properties;
}