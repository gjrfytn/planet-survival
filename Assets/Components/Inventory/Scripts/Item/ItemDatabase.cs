using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase<T> : ScriptableObject, IEnumerable
{
    public uint Count { get { return (uint)Items.Count; } }

	[SerializeField,HideInInspector]
	List<uint> IDs = new List<uint>();
	[SerializeField,HideInInspector]
	List<T> Items = new List<T>();
	[SerializeField,HideInInspector]
    List<uint> FreeIDs = new List<uint>();

	public T this[uint id]
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

	public void Add(T item)
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

	public uint GetID(T item)
	{
		int index=Items.IndexOf(item);
		if (index == -1)
			throw new System.ArgumentException("Item not found.","item");

		return IDs[index];
	}
}
