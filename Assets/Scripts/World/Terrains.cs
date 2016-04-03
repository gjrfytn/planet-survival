using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Terrains : MonoBehaviour
{
    [System.Serializable]
    public class TerrainProperties
    {
        public TerrainType Terrain;
        public ushort TravelTime;

    }

    public TerrainProperties[] TerrainsArray;

    void Awake()
    {
        for (byte i = 0; i < TerrainsArray.Length; ++i)
            for (byte j = 0; j < TerrainsArray.Length; ++j)
                if (TerrainsArray[i].Terrain == TerrainsArray[j].Terrain && i != j)
                    throw new System.Exception("Duplicatated terrain types in TerrainsList.");
    }
}
