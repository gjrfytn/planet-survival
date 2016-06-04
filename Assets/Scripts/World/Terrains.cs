using UnityEngine;
using System.Collections.Generic;

public class Terrains : MonoBehaviour
{
    [System.Serializable]
    public class TerrainProperties
    {
        public TerrainType Terrain;
        public TimedAction Travel;
    }

    public TerrainProperties[] TerrainsArray;

    void Awake()
    {
        for (byte i = 0; i < TerrainsArray.Length; ++i)
        {
            if (TerrainsArray[i].Terrain == TerrainType.NONE)
                throw new System.Exception("You should not initialize TerrainType.NONE.");
            for (byte j = 0; j < TerrainsArray.Length; ++j)
                if (TerrainsArray[i].Terrain == TerrainsArray[j].Terrain && i != j)
                    throw new System.Exception("Duplicatated terrain types in TerrainsArray.");
        }
    }

    public List<TerrainProperties> GetTerrainProperties(TerrainType type)
    {
        List<TerrainProperties> terrProp = new List<TerrainProperties>();
        foreach (TerrainProperties p in TerrainsArray)
            if ((p.Terrain & type) != TerrainType.NONE)
                terrProp.Add(p);
        return terrProp;
    }
}
