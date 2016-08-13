using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : ScriptableObject, IEnumerable
{
    public uint Count { get { return (uint)Items.Count; } }

	[SerializeField,HideInInspector]
	List<uint> IDs = new List<uint>();
	[SerializeField,HideInInspector]
    List<Item> Items = new List<Item>();
	[SerializeField,HideInInspector]
    List<uint> FreeIDs = new List<uint>();

    public Item this[uint id]
    {
        get
        {
			int index=IDs.IndexOf(id);
			if (index == -1)
				throw new KeyNotFoundException();
			
			return Items[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    public void Add(Item item)
    {
        if (FreeIDs.Count == 0)
			IDs.Add((uint)Items.Count);
        else
        {
			IDs.Add(FreeIDs[0]);
            FreeIDs.RemoveAt(0);
        }
        Items.Add(item);
    }

    public void Remove(uint id)
    {
		int index=IDs.IndexOf(id);
		if (index == -1)
			throw new KeyNotFoundException();
		IDs.RemoveAt(index);
		Items.RemoveAt(index);
        FreeIDs.Add(id);
    }

	public uint GetID(Item item)
	{
		int index=Items.IndexOf(item);
		if (index == -1)
			throw new System.ArgumentException("Item not found.","item");

		return IDs[index];
	}
}
