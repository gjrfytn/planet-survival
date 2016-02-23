using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Map
{
    public float[,] HeightMatrix;
    public byte?[,] HexSpriteID_Matrix;

    public float[,] ForestMatrix;

    public TerrainType[,] TerrainMatrix;

    public bool Contains(Vector2 coords)
    {
        return coords.y >= 0 && coords.y < HeightMatrix.GetLength(0) && coords.x >= 0 && coords.x < HeightMatrix.GetLength(1);
    }

    public float GetHeight(Vector2 coords)
    {
        return HeightMatrix[(int)coords.y, (int)coords.x];
    }

    public byte? GetHexSpriteID(Vector2 coords)
    {
        return HexSpriteID_Matrix[(int)coords.y, (int)coords.x];
    }

    public float GetForest(Vector2 coords)
    {
        return ForestMatrix[(int)coords.y, (int)coords.x];
    }

    public TerrainType GetTerrainType(Vector2 coords)
    {
        return TerrainMatrix[(int)coords.y, (int)coords.x];
    }

    public Map(ushort width, ushort height)
    {
        HeightMatrix = new float[height, width];
        HexSpriteID_Matrix = new byte?[height, width];
        ForestMatrix = new float[height, width];
        TerrainMatrix = new TerrainType[height, width];
    }
}

public sealed class LocalMap : Map
{
    public bool[,] BlockMatrix;

    public bool IsBlocked(Vector2 coords)
    {
        return BlockMatrix[(int)coords.y, (int)coords.x];
    }

    public LocalMap(ushort width, ushort height)
        : base(width, height)
    {
        BlockMatrix = new bool[height, width];
    }
}

public sealed class GlobalMap : Map
{
    public bool[,] RiverMatrix;
    public byte?[,] RiverSpriteID_Matrix;
    public short[,] RiverSpriteRotationMatrix;

    public bool[,] ClusterMatrix;
    public byte?[,] ClusterSpriteID_Matrix;

    public bool[,] RoadMatrix;
    public byte?[,] RoadSpriteID_Matrix;
    public short[,] RoadSpriteRotationMatrix;

    public List<List<Vector2>> Rivers;
    public List<List<Vector2>> Clusters;
    public List<List<Vector2>> Roads;

    public bool HasRiver(Vector2 coords)
    {
        return RiverMatrix[(int)coords.y, (int)coords.x];
    }

    public byte? GetRiverSpriteID(Vector2 coords)
    {
        return RiverSpriteID_Matrix[(int)coords.y, (int)coords.x];
    }

    public short GetRiverSpriteRotation(Vector2 coords)
    {
        return RiverSpriteRotationMatrix[(int)coords.y, (int)coords.x];
    }

    public bool HasCluster(Vector2 coords)
    {
        return ClusterMatrix[(int)coords.y, (int)coords.x];
    }

    public byte? GetClusterSpriteID(Vector2 coords)
    {
        return ClusterSpriteID_Matrix[(int)coords.y, (int)coords.x];
    }

    public bool HasRoad(Vector2 coords)
    {
        return RoadMatrix[(int)coords.y, (int)coords.x];
    }

    public byte? GetRoadSpriteID(Vector2 coords)
    {
        return RoadSpriteID_Matrix[(int)coords.y, (int)coords.x];
    }

    public short GetRoadSpriteRotation(Vector2 coords)
    {
        return RoadSpriteRotationMatrix[(int)coords.y, (int)coords.x];
    }

    public GlobalMap(ushort width, ushort height)
        : base(width, height)
    {
        RiverMatrix = new bool[height, width];
        RiverSpriteID_Matrix = new byte?[height, width];
        RiverSpriteRotationMatrix = new short[height, width];
        ClusterMatrix = new bool[height, width];
        ClusterSpriteID_Matrix = new byte?[height, width];
        RoadMatrix = new bool[height, width];
        RoadSpriteID_Matrix = new byte?[height, width];
        RoadSpriteRotationMatrix = new short[height, width];
    }
}
