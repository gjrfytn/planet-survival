using UnityEngine;
using System.Collections.Generic;

public class Terrains : MonoBehaviour
{
    GameObject TravelCashe;

    [System.Serializable]
    class TerrainProperties
    {
        [SerializeField]
        TerrainType Terrain_;
        public TerrainType Terrain { get { return Terrain_; } }
        [SerializeField]
        TimedAction Travel_;
        public TimedAction Travel { get { return Travel_; } }
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

    void Start()
    {
        TravelCashe = new GameObject("TravelCashe");
        TravelCashe.AddComponent<TimedAction>();
    }

    public TimedAction GetTerrainProperties(TerrainType type)
    {
        TimedAction prop = TravelCashe.GetComponent<TimedAction>();
        prop.Duration = 0;
        prop.WaterConsumption = 0;
        prop.FoodConsumption = 0;
        prop.StaminaConsumption = 0;
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
