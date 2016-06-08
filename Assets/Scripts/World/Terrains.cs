using UnityEngine;
using System.Collections.Generic;

public class Terrains : MonoBehaviour
{
    [System.Serializable]
    class TerrainProperties
    {
        [SerializeField]
        TerrainType Terrain_;
        public TerrainType Terrain { get { return Terrain_; } private set { Terrain_ = value; } }
        [SerializeField]
        TimedAction Travel_;
        public TimedAction Travel { get { return Travel_; } private set { Travel_ = value; } }
    }

    [SerializeField]
    TerrainProperties[] TerrainsArray;

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

    public TimedAction GetTerrainProperties(TerrainType type)
    {
        TimedAction prop = new TimedAction();
        foreach (TerrainProperties p in TerrainsArray)
            if ((p.Terrain & type) != TerrainType.NONE)
            {
                prop.Duration += p.Travel.Duration;
                prop.WaterConsumption += p.Travel.WaterConsumption;
                prop.FoodConsumption += p.Travel.FoodConsumption;
                prop.StaminaConsumption += p.Travel.StaminaConsumption;
            }
        return prop;
    }
}
