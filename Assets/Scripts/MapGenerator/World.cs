using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class World : MonoBehaviour
{
    public ushort GlobalMapChunkSize; //Должен быть 2 в n-ой степени
    public Vector2 LocalMapSize; //Должен быть 2 в n-ой степени

    public GameObject[] Enemies;

    Map CurrentMap; //TODO Возможно, можно будет убрать. Карта, на которой находится игрок.

    //const byte CashedChunksSize=3;

    Map[,] CashedGlobalMapChunks = new Map[3, 3];
    Map[,] LocalMaps;
    WorldGenerator Generator; //readonly?

    WorldVisualiser Visualiser; //Временно

    Vector2 GlobalMapCoords;
    GameObject Player;
    int ChunkX, ChunkY;
    const string ChunksDirectoryName = "chunks";
    string ChunksDirectoryPath;

    void OnEnable()
    {
        EventManager.CreatureMoved += ChangeBlockMatrix;
    }

    void OnDisable()
    {
        EventManager.CreatureMoved -= ChangeBlockMatrix;
    }

    void Awake()
    {
        Debug.Assert(Mathf.IsPowerOfTwo(GlobalMapChunkSize));
        Debug.Assert(Mathf.IsPowerOfTwo((int)LocalMapSize.x));
        Debug.Assert(Mathf.IsPowerOfTwo((int)LocalMapSize.y));

        GlobalMapChunkSize++; //TODO !!!
        LocalMapSize.x++; //TODO !!!
        LocalMapSize.y++; //TODO !!!
    }

    void Start()
    {
        ChunksDirectoryPath = Path.Combine(Application.dataPath, ChunksDirectoryName);

        if (Directory.Exists(ChunksDirectoryPath))
            Directory.Delete(ChunksDirectoryPath, true);

        Generator = GetComponent<WorldGenerator>();
        Visualiser = GetComponent<WorldVisualiser>(); //Временно

        // Это всё временно, как пример. На самом деле карта должна создаваться только при начале новой игры, иначе загружаться из сохранения.
        //--
        ChunkX = ChunkY = 0;

        CashedGlobalMapChunks[1, 1] = AllocAndGenerateChunk(CashedGlobalMapChunks[2, 1], CashedGlobalMapChunks[1, 2], CashedGlobalMapChunks[0, 1], CashedGlobalMapChunks[1, 0]);
        CashedGlobalMapChunks[0, 0] = AllocAndGenerateChunk(CashedGlobalMapChunks[1, 0], CashedGlobalMapChunks[0, 1], null, null);
        CashedGlobalMapChunks[0, 1] = AllocAndGenerateChunk(CashedGlobalMapChunks[1, 1], CashedGlobalMapChunks[0, 2], null, CashedGlobalMapChunks[0, 0]);
        CashedGlobalMapChunks[0, 2] = AllocAndGenerateChunk(CashedGlobalMapChunks[1, 2], null, null, CashedGlobalMapChunks[0, 1]);
        CashedGlobalMapChunks[1, 0] = AllocAndGenerateChunk(CashedGlobalMapChunks[2, 0], CashedGlobalMapChunks[0, 0], CashedGlobalMapChunks[0, 1], null);
        CashedGlobalMapChunks[1, 2] = AllocAndGenerateChunk(CashedGlobalMapChunks[2, 2], null, CashedGlobalMapChunks[0, 2], CashedGlobalMapChunks[1, 1]);
        CashedGlobalMapChunks[2, 0] = AllocAndGenerateChunk(null, CashedGlobalMapChunks[2, 1], CashedGlobalMapChunks[1, 0], null);
        CashedGlobalMapChunks[2, 1] = AllocAndGenerateChunk(null, CashedGlobalMapChunks[2, 2], CashedGlobalMapChunks[1, 1], CashedGlobalMapChunks[2, 0]);
        CashedGlobalMapChunks[2, 2] = AllocAndGenerateChunk(null, null, CashedGlobalMapChunks[1, 2], CashedGlobalMapChunks[2, 1]);

        LocalMaps = new Map[GlobalMapChunkSize, GlobalMapChunkSize];

        CurrentMap = CashedGlobalMapChunks[1, 1];

        Player = GameObject.FindWithTag("Player");
        Player.GetComponent<Player>().MapCoords = new Vector2(5, 5);
        Player.transform.position = WorldVisualiser.GetTransformPosFromMapCoords(Player.GetComponent<Player>().MapCoords);
        Camera.main.transform.position = transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y + Camera.main.transform.position.z * (Mathf.Tan((360 - Camera.main.transform.rotation.eulerAngles.x) / 57.3f)), Camera.main.transform.position.z);
        Visualiser.RenderVisibleHexes(Player.GetComponent<Player>().MapCoords, Player.GetComponent<Player>().ViewDistance, CashedGlobalMapChunks, ChunkY, ChunkX);
        Visualiser.HighlightHex(GetTopLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetTopMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetTopRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetBottomRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetBottomMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetBottomLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
    }

    public bool IsCurrentMapLocal()
    {
        return CurrentMap == CashedGlobalMapChunks[1, 1] ? false : true;
    }

    public void OnGotoHex()
    {
        if (!IsCurrentMapLocal())
        {
            float chunkX = Player.GetComponent<Player>().MapCoords.x / GlobalMapChunkSize, chunkY = Player.GetComponent<Player>().MapCoords.y / GlobalMapChunkSize;
            chunkX = Mathf.Floor(chunkX);
            chunkY = Mathf.Floor(chunkY);
            sbyte dx = (sbyte)(chunkX - ChunkX), dy = (sbyte)(chunkY - ChunkY);

            if (dx != 0 || dy != 0)
            {
                Debug.Log("Cashing chunks.");
                SaveCurrentChunkLocalMaps();
                if (!TryLoadFiledChunkLocalMaps((int)chunkY, (int)chunkX))
                {
                    for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                        for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                            LocalMaps[y, x] = null;
                }
            }

            if (dx != 0)
                for (sbyte i = -1; i < 2; ++i)
                {

                    SaveChunk(ChunkY + i, ChunkX - dx, CashedGlobalMapChunks[1 + i, 1 - dx]);
                    CashedGlobalMapChunks[1 + i, 1 - dx] = CashedGlobalMapChunks[1 + i, 1];
                    CashedGlobalMapChunks[1 + i, 1] = CashedGlobalMapChunks[1 + i, 1 + dx];
                    CashedGlobalMapChunks[1 + i, 1 + dx] = GetChunk(ChunkY + i, ChunkX + 2 * dx);
                }
            if (dy != 0)
                for (sbyte i = -1; i < 2; ++i)
                {
                    SaveChunk(ChunkY - dy, ChunkX + i, CashedGlobalMapChunks[1 - dy, 1 + i]);
                    CashedGlobalMapChunks[1 - dy, 1 + i] = CashedGlobalMapChunks[1, 1 + i];
                    CashedGlobalMapChunks[1, 1 + i] = CashedGlobalMapChunks[1 + dy, 1 + i];
                    CashedGlobalMapChunks[1 + dy, 1 + i] = GetChunk(ChunkY + 2 * dy, ChunkX + i);
                }

            ChunkX += dx;
            ChunkY += dy;
            CurrentMap = CashedGlobalMapChunks[1, 1];

            Visualiser.RenderVisibleHexes(Player.GetComponent<Player>().MapCoords, Player.GetComponent<Player>().ViewDistance, CashedGlobalMapChunks, ChunkY, ChunkX);
            Visualiser.DestroyAllObjects();//TODO Временно
            Visualiser.HighlightHex(GetTopLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            Visualiser.HighlightHex(GetTopMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            Visualiser.HighlightHex(GetTopRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            Visualiser.HighlightHex(GetBottomRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            Visualiser.HighlightHex(GetBottomMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            Visualiser.HighlightHex(GetBottomLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        }
        else
        {
            Visualiser.DestroyAllObjects();//TODO Временно
            if (IsHexFree(GetTopLeftMapCoords(Player.GetComponent<Player>().MapCoords)))
                Visualiser.HighlightHex(GetTopLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            if (IsHexFree(GetTopMapCoords(Player.GetComponent<Player>().MapCoords)))
                Visualiser.HighlightHex(GetTopMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            if (IsHexFree(GetTopRightMapCoords(Player.GetComponent<Player>().MapCoords)))
                Visualiser.HighlightHex(GetTopRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            if (IsHexFree(GetBottomRightMapCoords(Player.GetComponent<Player>().MapCoords)))
                Visualiser.HighlightHex(GetBottomRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            if (IsHexFree(GetBottomMapCoords(Player.GetComponent<Player>().MapCoords)))
                Visualiser.HighlightHex(GetBottomMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
            if (IsHexFree(GetBottomLeftMapCoords(Player.GetComponent<Player>().MapCoords)))
                Visualiser.HighlightHex(GetBottomLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        }
    }

    /// <summary>
    /// Переключение карт.
    /// </summary>
    public void SwitchMap()
    {
        if (!IsCurrentMapLocal())
        {
            GotoLocalMap();
            SpawnRandomEnemy();
        }
        else
        {
            GotoGlobalMap();
            EventManager.OnLocalMapLeave();
        }
    }

    /// <summary>
    /// Переход на локальную карту.
    /// </summary>
    void GotoLocalMap()
    {
        Vector2 mapCoords = Player.GetComponent<Player>().MapCoords;
        if (LocalMaps[(int)mapCoords.y, (int)mapCoords.x] == null)
            CreateLocalMap(mapCoords, CashedGlobalMapChunks[1, 1].GetHeight(mapCoords), CashedGlobalMapChunks[1, 1].ForestMatrix[(int)mapCoords.y, (int)mapCoords.x]);
        CurrentMap = LocalMaps[(int)mapCoords.y, (int)mapCoords.x];

        GlobalMapCoords = mapCoords;

        //TEMP
        Player.GetComponent<Player>().MapCoords.x = (ushort)LocalMapSize.x / 2;
        Player.GetComponent<Player>().MapCoords.y = (ushort)LocalMapSize.y / 2;
        //
        Visualiser.DestroyAllObjects();
        //Visualiser.RenderWholeMap (CurrentMap);
        Visualiser.RenderLocalMap();//

        Visualiser.HighlightHex(GetTopLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetTopMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetTopRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetBottomRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetBottomMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetBottomLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
    }

    /// <summary>
    /// Переход на глобальную карту.
    /// </summary>
    void GotoGlobalMap()
    {
        CurrentMap = CashedGlobalMapChunks[1, 1];
        Player.GetComponent<Player>().MapCoords = GlobalMapCoords;
        Visualiser.RenderVisibleHexes(Player.GetComponent<Player>().MapCoords, Player.GetComponent<Player>().ViewDistance, CashedGlobalMapChunks, ChunkY, ChunkX);

        Visualiser.DestroyAllObjects();
        Visualiser.HighlightHex(GetTopLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetTopMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetTopRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetBottomRightMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetBottomMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);
        Visualiser.HighlightHex(GetBottomLeftMapCoords(Player.GetComponent<Player>().MapCoords), Visualiser.BlueHexSprite);

        Visualiser.DestroyBackgound();//
    }

    /// <summary>
    /// Создаёт локальную карту.
    /// </summary>
    /// <param name="coords">Координаты новой карты на глобальной.</param>
    void CreateLocalMap(Vector2 mapCoords, float height, float forest)
    {
        Map map = new Map((ushort)LocalMapSize.x, (ushort)LocalMapSize.y);

        //Generator.CreateHeightmap (map.HeightMatrix, Generator.LandscapeRoughness, height, height, height, height);
        //Generator.CreateHeightmap (map.ForestMatrix, Generator.ForestRoughness, forest, forest, forest, forest);
        //Generator.CreateRivers (map.HeightMatrix, map.RiverMatrix);

        LocalMaps[(int)mapCoords.y, (int)mapCoords.x] = map;
    }

    Map GetChunk(int chunkY, int chunkX)
    {
        Map chunk = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        if (TryGetChunk(chunkY, chunkX, chunk))
            return chunk;
        else
        {
            Map top = new Map(GlobalMapChunkSize, GlobalMapChunkSize), right = new Map(GlobalMapChunkSize, GlobalMapChunkSize), bottom = new Map(GlobalMapChunkSize, GlobalMapChunkSize), left = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
            TryGetChunk(chunkY + 1, chunkX, top);
            TryGetChunk(chunkY, chunkX + 1, right);
            TryGetChunk(chunkY - 1, chunkX, bottom);
            TryGetChunk(chunkY, chunkX - 1, left);
            return AllocAndGenerateChunk(top, right, bottom, left);
        }
    }

    bool TryGetChunk(int chunkY, int chunkX, Map chunk)
    {
        if (Mathf.Abs(Mathf.Abs(chunkY) - Mathf.Abs(ChunkY)) <= 1 && Mathf.Abs(Mathf.Abs(chunkX) - Mathf.Abs(ChunkX)) <= 1)
        {
            for (sbyte y = -1; y <= 1; ++y)
                for (sbyte x = -1; x <= 1; ++x)
                    if (chunkY == ChunkY + y && chunkX == ChunkX + x)
                    {
                        chunk = CashedGlobalMapChunks[y + 1, x + 1];
                        return true;
                    }
            return false; // Компиляция?
        }
        else
        {
            if (TryLoadFiledChunk(chunkY, chunkX, chunk))
                return true;
            else
                return false;
        }
    }

    bool TryLoadFiledChunk(int chunkY, int chunkX, Map chunk)
    {
        string filePath = Path.Combine(ChunksDirectoryPath, chunkY + "_" + chunkX);
        if (File.Exists(filePath))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                        chunk.HeightMatrix[y, x] = reader.ReadSingle();
                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    {
                        short buf = reader.ReadInt16();
                        chunk.HexSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
                    }

                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                        chunk.ForestMatrix[y, x] = reader.ReadSingle();

                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                        chunk.RiverMatrix[y, x] = reader.ReadBoolean();
                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    {
                        short buf = reader.ReadInt16();
                        chunk.RiverSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
                    }
                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                        chunk.RiverSpriteRotationMatrix[y, x] = reader.ReadInt16();

                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                        chunk.ClusterMatrix[y, x] = reader.ReadBoolean();
                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    {
                        short buf = reader.ReadInt16();
                        chunk.ClusterSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
                    }

                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                        chunk.RoadMatrix[y, x] = reader.ReadBoolean();
                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    {
                        short buf = reader.ReadInt16();
                        chunk.RoadSpriteID_Matrix[y, x] = buf == -1 ? null : (byte?)buf;
                    }
                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                        chunk.RoadSpriteRotationMatrix[y, x] = reader.ReadInt16();



                byte riversCount = reader.ReadByte();
                chunk.Rivers = new List<List<Vector2>>(riversCount);
                for (byte i = 0; i < riversCount; ++i)
                {
                    ushort riverLength = reader.ReadUInt16();
                    chunk.Rivers[i] = new List<Vector2>(riverLength);
                    for (byte j = 0; j < riverLength; ++j)
                    {
                        chunk.Rivers[i].Add(new Vector2(
                            reader.ReadSingle(),
                            reader.ReadSingle()
                            ));
                    }
                }

                byte clusterCount = reader.ReadByte();
                chunk.Clusters = new List<List<Vector2>>(clusterCount);
                for (byte i = 0; i < clusterCount; ++i)
                {
                    ushort clusterSize = reader.ReadUInt16();
                    chunk.Clusters[i] = new List<Vector2>(clusterSize);
                    for (byte j = 0; j < clusterSize; ++j)
                    {
                        chunk.Clusters[i].Add(new Vector2(
                            reader.ReadSingle(),
                            reader.ReadSingle()
                            ));
                    }
                }
                byte roadCount = reader.ReadByte();
                chunk.Roads = new List<List<Vector2>>(roadCount);
                for (byte i = 0; i < roadCount; ++i)
                {
                    ushort roadLength = reader.ReadUInt16();
                    chunk.Roads[i] = new List<Vector2>(roadLength);
                    for (byte j = 0; j < roadLength; ++j)
                    {
                        chunk.Roads[i].Add(new Vector2(
                            reader.ReadSingle(),
                            reader.ReadSingle()
                            ));
                    }
                }
            }
            return true;
        }
        return false;
    }

    void SaveChunk(int chunkY, int chunkX, Map chunk)
    {
        Directory.CreateDirectory(ChunksDirectoryPath);
        using (BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine(ChunksDirectoryPath, chunkY + "_" + chunkX), FileMode.Create)))
        {
            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write(chunk.HeightMatrix[y, x]);
            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write((short)(chunk.HexSpriteID_Matrix[y, x] ?? -1));

            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write(chunk.ForestMatrix[y, x]);

            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write(chunk.RiverMatrix[y, x]);
            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write((short)(chunk.RiverSpriteID_Matrix[y, x] ?? -1));
            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write(chunk.RiverSpriteRotationMatrix[y, x]);

            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write(chunk.ClusterMatrix[y, x]);
            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write((short)(chunk.ClusterSpriteID_Matrix[y, x] ?? -1));

            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write(chunk.RoadMatrix[y, x]);
            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write((short)(chunk.RoadSpriteID_Matrix[y, x] ?? -1));
            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write(chunk.RoadSpriteRotationMatrix[y, x]);

            writer.Write(chunk.Rivers.Count);
            for (byte i = 0; i < chunk.Rivers.Count; ++i)
            {
                writer.Write((ushort)chunk.Rivers[i].Count);
                for (byte j = 0; j < chunk.Rivers[i].Count; ++j)
                {
                    writer.Write(chunk.Rivers[i][j].x);
                    writer.Write(chunk.Rivers[i][j].y);
                }
            }
            writer.Write(chunk.Clusters.Count);
            for (byte i = 0; i < chunk.Clusters.Count; ++i)
            {
                writer.Write((ushort)chunk.Clusters[i].Count);
                for (byte j = 0; j < chunk.Clusters[i].Count; ++j)
                {
                    writer.Write(chunk.Clusters[i][j].x);
                    writer.Write(chunk.Clusters[i][j].y);
                }
            }
            writer.Write(chunk.Roads.Count);
            for (byte i = 0; i < chunk.Roads.Count; ++i)
            {
                writer.Write((ushort)chunk.Roads[i].Count);
                for (byte j = 0; j < chunk.Roads[i].Count; ++j)
                {
                    writer.Write(chunk.Roads[i][j].x);
                    writer.Write(chunk.Roads[i][j].y);
                }
            }
        }
    }

    bool TryLoadFiledChunkLocalMaps(int chunkY, int chunkX)
    {
        string filePath = Path.Combine(ChunksDirectoryPath, chunkY + "_" + chunkX + "lm");
        if (File.Exists(filePath))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    {
                        if (reader.ReadString() == "null")
                            LocalMaps[y, x] = null;
                        else
                        {
                            for (ushort lmY = 0; lmY < LocalMapSize.y; ++lmY)
                                for (ushort lmX = 0; lmX < LocalMapSize.x; ++lmX)
                                    LocalMaps[y, x].BlockMatrix[lmY, lmX] = reader.ReadBoolean();
                        }
                        //...

                    }
            }
            return true;
        }
        return false;
    }

    void SaveCurrentChunkLocalMaps()
    {
        Directory.CreateDirectory(ChunksDirectoryPath);
        using (BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine(ChunksDirectoryPath, ChunkY + "_" + ChunkX + "lm"), FileMode.Create)))
        {
            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                {
                    if (LocalMaps[y, x] == null)
                        writer.Write("null");
                    else
                    {
                        writer.Write("notnull");
                        for (ushort lmY = 0; lmY < LocalMapSize.y; ++lmY)
                            for (ushort lmX = 0; lmX < LocalMapSize.x; ++lmX)
                                writer.Write(LocalMaps[y, x].BlockMatrix[lmY, lmX]);
                    }

                    //...
                }
        }
    }

    public static Vector2 GetTopLeftMapCoords(Vector2 mapCoords)
    {
        return new Vector2(mapCoords.x - 1, mapCoords.y + 1 - ((mapCoords.x % 2) != 0 ? 0 : 1));
    }

    public static Vector2 GetTopMapCoords(Vector2 mapCoords)
    {
        return new Vector2(mapCoords.x, mapCoords.y + 1);
    }

    public static Vector2 GetTopRightMapCoords(Vector2 mapCoords)
    {
        return new Vector2(mapCoords.x + 1, mapCoords.y + 1 - ((mapCoords.x % 2) != 0 ? 0 : 1));
    }

    public static Vector2 GetBottomRightMapCoords(Vector2 mapCoords)
    {
        return new Vector2(mapCoords.x + 1, mapCoords.y - ((mapCoords.x % 2) != 0 ? 0 : 1));
    }

    public static Vector2 GetBottomMapCoords(Vector2 mapCoords)
    {
        return new Vector2(mapCoords.x, mapCoords.y - 1);
    }

    public static Vector2 GetBottomLeftMapCoords(Vector2 mapCoords)
    {
        return new Vector2(mapCoords.x - 1, mapCoords.y - ((mapCoords.x % 2) != 0 ? 0 : 1));
    }

    /// <summary>
    /// Определяет, прилегают ли к друг другу данные координаты.
    /// </summary>
    /// <returns><c>true</c> если прилегают, иначе <c>false</c>.</returns>
    /// <param name="mapCoords1">1 координаты.</param>
    /// <param name="mapCoords2">2 координаты.</param>
    public static bool IsMapCoordsAdjacent(Vector2 mapCoords1, Vector2 mapCoords2)
    {
        byte k = (byte)((mapCoords1.x % 2) != 0 ? 1 : 0);
        if (mapCoords1.x - 1 == mapCoords2.x || mapCoords1.x + 1 == mapCoords2.x)
        {
            if (mapCoords1.y - 1 + k == mapCoords2.y)
                return true;
            if (mapCoords1.y + k == mapCoords2.y)
                return true;
            return false;
        }
        if (mapCoords1.x == mapCoords2.x)
        {
            if (mapCoords1.y - 1 == mapCoords2.y)
                return true;
            if (mapCoords1.y + 1 == mapCoords2.y)
                return true;
            return false;
        }
        return false;
    }

    void SpawnRandomEnemy()
    {
        Vector2 mapCoords = new Vector2(Random.Range(0, (int)LocalMapSize.x), Random.Range(0, (int)LocalMapSize.y));
        GameObject enemy = Instantiate(Enemies[Random.Range(0, Enemies.Length)], WorldVisualiser.GetTransformPosFromMapCoords(mapCoords), Quaternion.identity) as GameObject;
        enemy.GetComponent<Creature>().MapCoords = mapCoords;
        enemy.GetComponent<Creature>().Attack(Player);
        CurrentMap.BlockMatrix[(int)mapCoords.y, (int)mapCoords.x] = true;
    }

    public bool IsHexFree(Vector2 mapCoords)
    {
        if (IsCurrentMapLocal())//Временно
        {
            if (mapCoords.x >= 0 && mapCoords.x < LocalMapSize.x && mapCoords.y >= 0 && mapCoords.y < LocalMapSize.y)
                return !CurrentMap.IsBlocked(mapCoords);
            return false;
        }
        return true;
    }

    void ChangeBlockMatrix(Vector2 free, Vector2 blocked)
    {
        if (IsCurrentMapLocal())//Временно
        {
            CurrentMap.BlockMatrix[(int)free.y, (int)free.x] = false;
            CurrentMap.BlockMatrix[(int)blocked.y, (int)blocked.x] = true;
        }
    }

    Map AllocAndGenerateChunk(Map topChunk, Map rightChunk, Map bottomChunk, Map leftChunk)
    {
        // TODO Ждём MonoDevelop 6.0 ( ?., ?[ ):
        Map chunk = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, ((leftChunk != null ? leftChunk.HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1] : Random.value) + (topChunk != null ? topChunk.HeightMatrix[0, 0] : Random.value)) / 2f, ((topChunk != null ? topChunk.HeightMatrix[0, GlobalMapChunkSize - 1] : Random.value) + (rightChunk != null ? rightChunk.HeightMatrix[GlobalMapChunkSize - 1, 0] : Random.value)) / 2f, ((leftChunk != null ? leftChunk.HeightMatrix[0, GlobalMapChunkSize - 1] : Random.value) + (bottomChunk != null ? bottomChunk.HeightMatrix[GlobalMapChunkSize - 1, 0] : Random.value)) / 2f, ((bottomChunk != null ? bottomChunk.HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1] : Random.value) + (rightChunk != null ? rightChunk.HeightMatrix[0, 0] : Random.value)) / 2f);
        Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, ((leftChunk != null ? leftChunk.ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1] : Random.value) + (topChunk != null ? topChunk.ForestMatrix[0, 0] : Random.value)) / 2f, ((topChunk != null ? topChunk.ForestMatrix[0, GlobalMapChunkSize - 1] : Random.value) + (rightChunk != null ? rightChunk.ForestMatrix[GlobalMapChunkSize - 1, 0] : Random.value)) / 2f, ((leftChunk != null ? leftChunk.ForestMatrix[0, GlobalMapChunkSize - 1] : Random.value) + (bottomChunk != null ? bottomChunk.ForestMatrix[GlobalMapChunkSize - 1, 0] : Random.value)) / 2f, ((bottomChunk != null ? bottomChunk.ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1] : Random.value) + (rightChunk != null ? rightChunk.ForestMatrix[0, 0] : Random.value)) / 2f);
        chunk.Rivers = Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
        chunk.Clusters = Generator.CreateClusters(chunk);
        chunk.Roads = new List<List<Vector2>>(); //UNDONE
        return chunk;
    }
}
