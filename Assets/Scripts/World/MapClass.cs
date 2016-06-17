using System.Collections.Generic;
using System.IO;
using System.Linq;

public abstract class Map : IBinaryReadableWriteable
{
    public readonly ushort Width;
    public readonly ushort Height;

    public TerrainType[,] TerrainMatrix;

    public byte?[,] HexSpriteID_Matrix;

    public Map(ushort width, ushort height)
    {
        Width = width;
        Height = height;
        TerrainMatrix = new TerrainType[height, width];
        HexSpriteID_Matrix = new byte?[height, width];
    }

    public virtual void Write(BinaryWriter writer)
    {
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write((int)TerrainMatrix[y, x]);

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write((short)(HexSpriteID_Matrix[y, x] ?? -1));
    }

    public virtual void Read(SymBinaryReader reader)
    {
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                int buf;
                reader.Read(out buf);
                TerrainMatrix[y, x] = (TerrainType)buf;
            }

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                short buf;
                reader.Read(out buf);
                HexSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
            }
    }

    public bool Contains(GlobalPos coords)//C#6.0 EBD
    {
        return coords.Y >= 0 && coords.Y < Height && coords.X >= 0 && coords.X < Width;
    }
}

public sealed class LocalMap : Map
{
    bool Active;
    ushort freeID;
    Dictionary<ushort, Entity>[,] ObjectMatrix;

    public LocalMap(ushort width, ushort height)
        : base(width, height)
    {
        ObjectMatrix = new Dictionary<ushort, Entity>[height, width];
        for (ushort y = 0; y < height; ++y)
            for (ushort x = 0; x < width; ++x)
                ObjectMatrix[y, x] = new Dictionary<ushort, Entity>();
    }

    ~LocalMap()
    {
        if (Active)
            Deactivate();
        //TODO Уничтожать Entity
    }

    public override void Write(BinaryWriter writer)
    {
        base.Write(writer);
        writer.Write(freeID);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                writer.Write((byte)ObjectMatrix[y, x].Count);
                foreach (KeyValuePair<ushort, Entity> obj in ObjectMatrix[y, x])
                {
                    writer.Write(obj.Key);
                    obj.Value.Write(writer);
                }
            }
    }

    public override void Read(SymBinaryReader reader)
    {
        base.Read(reader);
        reader.Read(out freeID);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                byte objCount;
                reader.Read(out objCount);
                ObjectMatrix[y, x] = new Dictionary<ushort, Entity>(objCount);
                for (byte i = 0; i < objCount; ++i)
                {
                    UnityEngine.GameObject entBuf = new UnityEngine.GameObject(); //TODO
                    ushort idBuf;
                    reader.Read(out idBuf);
                    entBuf.AddComponent<Entity>().Read(reader);
                    ObjectMatrix[y, x].Add(idBuf, entBuf.GetComponent<Entity>()
                    );
                }
            }
    }

    public void Activate()
    {
        Active = true;
        EventManager.EntitySpawned += AddObject;
        EventManager.CreatureMoved += MoveObject;
        EventManager.EntityDestroyed += RemoveObject;

        foreach (Dictionary<ushort, Entity> cell in ObjectMatrix)
            foreach (KeyValuePair<ushort, Entity> obj in cell)
                obj.Value.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        Active = false;
        EventManager.EntitySpawned -= AddObject;
        EventManager.CreatureMoved -= MoveObject;
        EventManager.EntityDestroyed -= RemoveObject;

        foreach (Dictionary<ushort, Entity> cell in ObjectMatrix)
            foreach (KeyValuePair<ushort, Entity> obj in cell)
                obj.Value.gameObject.SetActive(false);
    }

    public bool IsBlocked(LocalPos coords)//C#6.0 EBD
    {
        return ObjectMatrix[coords.Y, coords.X].Any(o => o.Value.Blocking);
    }

    public bool[,] GetBlockMatrix()
    {
        bool[,] bm = new bool[Height, Width];
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                bm[y, x] = ObjectMatrix[y, x].Any(o => o.Value.Blocking);
        return bm;
    }

    public void AddObject(Entity obj)
    {
        ObjectMatrix[obj.Pos.Y, obj.Pos.X].Add(freeID, obj);
        freeID++;
    }

    void MoveObject(LocalPos from, LocalPos to)
    {
        KeyValuePair<ushort, Entity> obj = ObjectMatrix[from.Y, from.X].Single(o => o.Value.Pos != from);
        ObjectMatrix[to.Y, to.X].Add(obj.Key, obj.Value);
        ObjectMatrix[from.Y, from.X].Remove(obj.Key);
    }

    void RemoveObject(Entity e)
    {
        KeyValuePair<ushort, Entity> obj = ObjectMatrix[e.Pos.Y, e.Pos.X].Single(o => o.Value == e);
        ObjectMatrix[e.Pos.Y, e.Pos.X].Remove(obj.Key);
    }

    public List<LivingBeing> GetAllLivingBeings()
    {
        List<LivingBeing> lBList = new List<LivingBeing>();
        foreach (Dictionary<ushort, Entity> hex in ObjectMatrix)
            foreach (Entity obj in hex.Values)
                if (obj is LivingBeing)
                    lBList.Add((LivingBeing)obj);
        return lBList;
    }
}

public sealed class Chunk : Map
{
    public float[,] HeightMatrix;

    public float[,] ForestMatrix;

    public byte?[,] RiverSpriteID_Matrix;
    public short[,] RiverSpriteRotationMatrix;

    public byte?[,] ClusterSpriteID_Matrix;

    public byte?[,] RoadSpriteID_Matrix;
    public short[,] RoadSpriteRotationMatrix;

    public List<List<LocalPos>> Rivers;
    public List<List<LocalPos>> Clusters;
    public List<List<LocalPos>> Roads;

    public Chunk(ushort width, ushort height)
        : base(width, height)
    {
        HeightMatrix = new float[height, width];
        ForestMatrix = new float[height, width];
        RiverSpriteID_Matrix = new byte?[height, width];
        RiverSpriteRotationMatrix = new short[height, width];
        ClusterSpriteID_Matrix = new byte?[height, width];
        RoadSpriteID_Matrix = new byte?[height, width];
        RoadSpriteRotationMatrix = new short[height, width];
    }

    public override void Write(BinaryWriter writer)
    {
        base.Write(writer);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(HeightMatrix[y, x]);

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(ForestMatrix[y, x]);

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write((short)(RiverSpriteID_Matrix[y, x] ?? -1));
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(RiverSpriteRotationMatrix[y, x]);

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write((short)(ClusterSpriteID_Matrix[y, x] ?? -1));

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write((short)(RoadSpriteID_Matrix[y, x] ?? -1));
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                writer.Write(RoadSpriteRotationMatrix[y, x]);

        writer.Write((byte)Rivers.Count);
        for (byte i = 0; i < Rivers.Count; ++i)
        {
            writer.Write((ushort)Rivers[i].Count);
            for (ushort j = 0; j < Rivers[i].Count; ++j)
            {
                writer.Write(Rivers[i][j].X);
                writer.Write(Rivers[i][j].Y);
            }
        }
        writer.Write((byte)Clusters.Count);
        for (byte i = 0; i < Clusters.Count; ++i)
        {
            writer.Write((byte)Clusters[i].Count);
            for (byte j = 0; j < Clusters[i].Count; ++j)
            {
                writer.Write(Clusters[i][j].X);
                writer.Write(Clusters[i][j].Y);
            }
        }
        writer.Write((byte)Roads.Count);
        for (byte i = 0; i < Roads.Count; ++i)
        {
            writer.Write((ushort)Roads[i].Count);
            for (ushort j = 0; j < Roads[i].Count; ++j)
            {
                writer.Write(Roads[i][j].X);
                writer.Write(Roads[i][j].Y);
            }
        }
    }

    public override void Read(SymBinaryReader reader)
    {
        base.Read(reader);
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                reader.Read(out HeightMatrix[y, x]);

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                reader.Read(out ForestMatrix[y, x]);

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                short buf;
                reader.Read(out buf);
                RiverSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
            }
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                reader.Read(out RiverSpriteRotationMatrix[y, x]);

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                short buf;
                reader.Read(out buf);
                ClusterSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
            }

        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
            {
                short buf;
                reader.Read(out buf);
                RoadSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
            }
        for (ushort y = 0; y < Height; ++y)
            for (ushort x = 0; x < Width; ++x)
                reader.Read(out RoadSpriteRotationMatrix[y, x]);

        byte riversCount;
        reader.Read(out riversCount);
        Rivers = new List<List<LocalPos>>(riversCount);
        for (byte i = 0; i < riversCount; ++i)
        {
            ushort riverLength;
            reader.Read(out riverLength);
            Rivers.Add(new List<LocalPos>(riverLength));
            for (ushort j = 0; j < riverLength; ++j)
            {
                LocalPos buf;
                reader.Read(out buf.X);
                reader.Read(out buf.Y);
                Rivers[i].Add(buf);
            }
        }
        byte clusterCount;
        reader.Read(out clusterCount);
        Clusters = new List<List<LocalPos>>(clusterCount);
        for (byte i = 0; i < clusterCount; ++i)
        {
            byte clusterSize;
            reader.Read(out clusterSize);
            Clusters.Add(new List<LocalPos>(clusterSize));
            for (byte j = 0; j < clusterSize; ++j)
            {
                LocalPos buf;
                reader.Read(out buf.X);
                reader.Read(out buf.Y);
                Clusters[i].Add(buf);
            }
        }
        byte roadCount;
        reader.Read(out roadCount);
        Roads = new List<List<LocalPos>>(roadCount);
        for (byte i = 0; i < roadCount; ++i)
        {
            ushort roadLength;
            reader.Read(out roadLength);
            Roads.Add(new List<LocalPos>(roadLength));
            for (ushort j = 0; j < roadLength; ++j)
            {
                LocalPos buf;
                reader.Read(out buf.X);
                reader.Read(out buf.Y);
                Roads[i].Add(buf);
            }
        }
    }
}
