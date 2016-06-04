using UnityEngine;

public class WorldWrapper : MonoBehaviour
{
    public float LandscapeRoughness;
    public float ForestRoughness;

    public WorldGenerator.RiversParameters RiverParam;
    public WorldGenerator.ClustersParameters ClusterParam;
    public WorldGenerator.RoadsParameters RoadParam;

    public ushort GlobalMapChunkSize;
    public LocalPos LocalMapSize;

    public byte ForestDensity;
    public byte TreeCountForForestTerrain;

    public LivingBeing[] Enemies;

    public World World { get; private set; }

    void Awake()
    {
        World = new World(
            LandscapeRoughness, ForestRoughness,
            RiverParam,
            ClusterParam,
            RoadParam,
            GlobalMapChunkSize,
            LocalMapSize,
            ForestDensity,
            TreeCountForForestTerrain,
            Enemies
            );
    }

    public void SwitchMap()//C#6.0 EBD
    {
        World.SwitchMap();
    }
}
