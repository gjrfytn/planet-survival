using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Blueprint {

    public uint Id;
    public Item Item;
    public List<Item> ItemsForCraft = new List<Item>();
    public List<uint> ItemIds = new List<uint>();
    public List<int> RequiredAmount = new List<int>();
    public ushort CraftTime;
    public byte CraftLevel;

}
