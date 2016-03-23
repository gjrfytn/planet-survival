using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public abstract class Map : IBinaryReadableWriteable
{
    public readonly ushort Width;
    public readonly ushort Height;

    public float[,] HeightMatrix;
    public byte?[,] HexSpriteID_Matrix;

    public float[,] ForestMatrix;

    public virtual void Write(BinaryWriter writer)
    {
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(HeightMatrix[y, x]);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write((short)(HexSpriteID_Matrix[y, x] ?? -1));

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(ForestMatrix[y, x]);
    }

    public virtual void Read(BinaryReader reader)
    {
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                HeightMatrix[y, x] = reader.ReadSingle();
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                short buf = reader.ReadInt16();
                HexSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
            }

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                ForestMatrix[y, x] = reader.ReadSingle();
    }

    public bool Contains(Vector2 coords)
    {
        return coords.y >= 0 && coords.y < Height && coords.x >= 0 && coords.x < Width;
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

    public Map(ushort width, ushort height)
    {
        Width = width;
        Height = height;
        HeightMatrix = new float[height, width];
        HexSpriteID_Matrix = new byte?[height, width];
        ForestMatrix = new float[height, width];
    }
}

public sealed class LocalMap : Map
{
    public bool[,] BlockMatrix;

    public override void Write(BinaryWriter writer)
    {
        base.Write(writer);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(BlockMatrix[y, x]);
    }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                BlockMatrix[y, x] = reader.ReadBoolean();
    }

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

    public TerrainType[,] TerrainMatrix;

    public override void Write(BinaryWriter writer)
    {
        base.Write(writer);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(RiverMatrix[y, x]);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write((short)(RiverSpriteID_Matrix[y, x] ?? -1));
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(RiverSpriteRotationMatrix[y, x]);

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(ClusterMatrix[y, x]);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write((short)(ClusterSpriteID_Matrix[y, x] ?? -1));

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(RoadMatrix[y, x]);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write((short)(RoadSpriteID_Matrix[y, x] ?? -1));
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(RoadSpriteRotationMatrix[y, x]);

        writer.Write(Rivers.Count);
        for (byte i = 0; i < Rivers.Count; ++i)
        {
            writer.Write((ushort)Rivers[i].Count);
            for (byte j = 0; j < Rivers[i].Count; ++j)
            {
                writer.Write(Rivers[i][j].x);
                writer.Write(Rivers[i][j].y);
            }
        }
        writer.Write(Clusters.Count);
        for (byte i = 0; i < Clusters.Count; ++i)
        {
            writer.Write((ushort)Clusters[i].Count);
            for (byte j = 0; j < Clusters[i].Count; ++j)
            {
                writer.Write(Clusters[i][j].x);
                writer.Write(Clusters[i][j].y);
            }
        }
        writer.Write(Roads.Count);
        for (byte i = 0; i < Roads.Count; ++i)
        {
            writer.Write((ushort)Roads[i].Count);
            for (byte j = 0; j < Roads[i].Count; ++j)
            {
                writer.Write(Roads[i][j].x);
                writer.Write(Roads[i][j].y);
            }
        }

        //TODO TerrainType
    }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                RiverMatrix[y, x] = reader.ReadBoolean();
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                short buf = reader.ReadInt16();
                RiverSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
            }
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                RiverSpriteRotationMatrix[y, x] = reader.ReadInt16();

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                ClusterMatrix[y, x] = reader.ReadBoolean();
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                short buf = reader.ReadInt16();
                ClusterSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
            }

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                RoadMatrix[y, x] = reader.ReadBoolean();
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                short buf = reader.ReadInt16();
                RoadSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
            }
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                RoadSpriteRotationMatrix[y, x] = reader.ReadInt16();



        byte riversCount = reader.ReadByte();
        Rivers = new List<List<Vector2>>(riversCount);
        for (byte i = 0; i < riversCount; ++i)
        {
            ushort riverLength = reader.ReadUInt16();
            Rivers[i] = new List<Vector2>(riverLength);
            for (byte j = 0; j < riverLength; ++j)
            {
                Rivers[i].Add(new Vector2(
                    reader.ReadSingle(),
                    reader.ReadSingle()
                    ));
            }
        }

        byte clusterCount = reader.ReadByte();
        Clusters = new List<List<Vector2>>(clusterCount);
        for (byte i = 0; i < clusterCount; ++i)
        {
            ushort clusterSize = reader.ReadUInt16();
            Clusters[i] = new List<Vector2>(clusterSize);
            for (byte j = 0; j < clusterSize; ++j)
            {
                Clusters[i].Add(new Vector2(
                    reader.ReadSingle(),
                    reader.ReadSingle()
                    ));
            }
        }
        byte roadCount = reader.ReadByte();
        Roads = new List<List<Vector2>>(roadCount);
        for (byte i = 0; i < roadCount; ++i)
        {
            ushort roadLength = reader.ReadUInt16();
            Roads[i] = new List<Vector2>(roadLength);
            for (byte j = 0; j < roadLength; ++j)
            {
                Roads[i].Add(new Vector2(
                    reader.ReadSingle(),
                    reader.ReadSingle()
                    ));
            }
        }
    }

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

    public TerrainType GetTerrainType(Vector2 coords)
    {
        return TerrainMatrix[(int)coords.y, (int)coords.x];
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
        TerrainMatrix = new TerrainType[height, width];
    }
}
