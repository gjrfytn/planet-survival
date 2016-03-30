using UnityEngine;
using System.Collections;

public class WorldWrapper : MonoBehaviour
{
    public float LandscapeRoughness;
    public float ForestRoughness;

    public RiversParameters RiverParam;
    public ClustersParameters ClusterParam;

    public ushort GlobalMapChunkSize;
    public Vector2 LocalMapSize;

    public byte ForestDensity;
    public byte TreeCountForForestTerrain;

    public GameObject[] Enemies;

    public World World { get; private set; }

    void Awake()
    {
        World = new World(
            LandscapeRoughness, ForestRoughness,
            RiverParam,
            ClusterParam,
            GlobalMapChunkSize,
            LocalMapSize,
            ForestDensity,
            TreeCountForForestTerrain,
            Enemies
            );
    }

    public void SwitchMap()
    {
        World.SwitchMap();
    }
}
