using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CraftedItem {

    public Item Item;
    public List<Item> ItemsForCraft;
    public List<int> ItemIds;
    public List<int> RequiredAmount;
    public byte CraftTime;
    public byte CraftLevel;

}
