using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : ScriptableObject, IEnumerable
{
    public uint Count
    {
        get
        {
            return (uint)Items.Count;
        }
    }

    Dictionary<uint, Item> Items = new Dictionary<uint, Item>();
    //List<CraftedItem> CraftItems;

    List<uint> FreeIDs = new List<uint>();

    public Item this[uint id]
    {
        get
        {
            return Items[id].ItemCopy();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Items.Values.GetEnumerator();
    }

    /*public Item FindItemByName(string name)
    {
		//C# 6.0
		Item item=Items.Find(i=>i.Name==name);
		return item!=null?item.ItemCopy():null;
    }*/

    public void Add(Item item) //TODO Принимать параметры конструктора?
    {
        if (FreeIDs.Count == 0)
            item.Id = (uint)Items.Count;
        else
        {
            item.Id = FreeIDs[0];
            FreeIDs.RemoveAt(0);
        }
        Items.Add(item.Id, item);
    }

    public void Remove(uint id) //C# 6.0
    {
        Items.Remove(id);
        FreeIDs.Add(id);
    }
}
