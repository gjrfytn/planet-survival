using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : ScriptableObject, IEnumerable
{
    public uint Count { get { return (uint)Items.Count; } }

    [SerializeField]
    List<Item> Items = new List<Item>();

    List<uint> FreeIDs = new List<uint>();

    //public List<Blueprint> Blueprints = new List<Blueprint>();

    public Item this[uint id]
    {
        get
        {
            Item buf = Items.Find(i => i.Id == id);
            if (buf == null)
                throw new KeyNotFoundException();
            return buf;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    public void Add(Item item) //TODO Принимать параметры конструктора?
    {
        if (FreeIDs.Count == 0)
            item.Id = (uint)Items.Count;
        else
        {
            item.Id = FreeIDs[0];
            FreeIDs.RemoveAt(0);
        }
        Items.Add(item);
    }

    public void Remove(uint id)
    {
        Item buf = Items.Find(i => i.Id == id);
        if (buf == null)
            throw new KeyNotFoundException();
        Items.Remove(buf);
        FreeIDs.Add(id);
    }
}
