using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlueprintDatabase : ScriptableObject, IEnumerable
{
    public uint Count { get { return (uint)Blueprints.Count; } }

    [SerializeField]
    List<Blueprint> Blueprints = new List<Blueprint>();

    List<uint> FreeIDs = new List<uint>();

    public Blueprint this[uint id]
    {
        get
        {
            Blueprint buf = Blueprints.Find(i => i.Id == id);
            if (buf == null)
                throw new KeyNotFoundException();
            return buf;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Blueprints.GetEnumerator();
    }

    public void Add(Blueprint blueprint) //TODO Принимать параметры конструктора?
    {
        if (FreeIDs.Count == 0)
            blueprint.Id = (uint)Blueprints.Count;
        else
        {
            blueprint.Id = FreeIDs[0];
            FreeIDs.RemoveAt(0);
        }
        Blueprints.Add(blueprint);
    }

    public void Remove(uint id)
    {
        Blueprint buf = Blueprints.Find(i => i.Id == id);
        if (buf == null)
            throw new KeyNotFoundException();
        Blueprints.Remove(buf);
        FreeIDs.Add(id);
    }

    public Blueprint FindBlueprint(int id)
    {
        for (int i = 0; i < Blueprints.Count; i++)
        {
            if (Blueprints[i].Id == id)
            {
                return Blueprints[i];
            }
        }
        return new Blueprint();
    }
}
