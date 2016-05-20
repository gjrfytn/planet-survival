﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class World
{
    public readonly float LandscapeRoughness;
    public readonly float ForestRoughness;

    public readonly RiversParameters RiverParam;
    public readonly ClustersParameters ClusterParam;

    public readonly ushort GlobalMapChunkSize; //Должен быть 2 в n-ой степени
    public readonly Vector2 LocalMapSize; //Должен быть 2 в n-ой степени

    public readonly byte ForestDensity;
    public readonly byte TreeCountForForestTerrain;

    public readonly GameObject[] Enemies;

    public Map CurrentMap { get; private set; } //TODO Возможно, можно будет убрать. Карта, на которой находится игрок.

    GlobalMap[,] CashedGlobalMapChunks = new GlobalMap[3, 3];
    LocalMap[,] LocalMaps;

    WorldVisualiser Visualiser; //Временно

    Vector2 GlobalMapCoords;
    GameObject Player;
    int ChunkX, ChunkY;
    const string ChunksDirectoryName = "chunks";
    string ChunksDirectoryPath;

    public World(float landscapeRoughness, float forestRoughness, RiversParameters riverParam, ClustersParameters clusterParam, ushort globalMapChunkSize, Vector2 localMapSize, byte forestDensity, byte treeCountForForestTerrain, GameObject[] enemies)
    {
        LandscapeRoughness = landscapeRoughness;
        ForestRoughness = forestRoughness;
        RiverParam = riverParam;
        ClusterParam = clusterParam;
        GlobalMapChunkSize = globalMapChunkSize;
        LocalMapSize = localMapSize;
        ForestDensity = forestDensity;
        TreeCountForForestTerrain = treeCountForForestTerrain;
        Enemies = enemies;
        //--
        Debug.Assert(Mathf.IsPowerOfTwo(GlobalMapChunkSize));
        Debug.Assert(Mathf.IsPowerOfTwo((int)LocalMapSize.x));
        Debug.Assert(Mathf.IsPowerOfTwo((int)LocalMapSize.y));

        GlobalMapChunkSize++; //TODO !!!
        LocalMapSize.x++; //TODO !!!
        LocalMapSize.y++; //TODO !!!

        //--
        ChunksDirectoryPath = Path.Combine(Application.streamingAssetsPath, ChunksDirectoryName); //UNDONE Не будет работать на Android?

        if (Directory.Exists(ChunksDirectoryPath))
            Directory.Delete(ChunksDirectoryPath, true);

        Visualiser = GameObject.FindWithTag("World").GetComponent<WorldVisualiser>(); //Временно

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

        LocalMaps = new LocalMap[GlobalMapChunkSize, GlobalMapChunkSize];

        CurrentMap = CashedGlobalMapChunks[1, 1];

        Player = GameObject.FindWithTag("Player");
        Player.GetComponent<Player>().MapCoords = new Vector2(5, 5);
        Player.transform.position = WorldVisualiser.GetTransformPosFromMapCoords(Player.GetComponent<Player>().MapCoords, false);
        Camera.main.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y + Camera.main.transform.position.z * (Mathf.Tan((360 - Camera.main.transform.rotation.eulerAngles.x) / 57.3f)), Camera.main.transform.position.z);
        Visualiser.RenderVisibleHexes(Player.GetComponent<Player>().MapCoords, Player.GetComponent<Player>().ViewDistance, CashedGlobalMapChunks, ChunkY, ChunkX);
        for (byte i = 0; i < 6; ++i)
        {
            Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(Player.GetComponent<Player>().MapCoords, (HexDirection)i), Visualiser.BlueHexSprite, false);
        }
        //--

        EventManager.PlayerMoved += OnPlayerGotoHex;
    }

    ~World()
    {
        EventManager.PlayerMoved -= OnPlayerGotoHex;
    }

    /// <summary>
    /// Определяет, является ли текущая карта локальной.
    /// </summary>
    /// <returns><c>true</c> если является, иначе <c>false</c>.</returns>
    public bool IsCurrentMapLocal()
    {
        return CurrentMap == CashedGlobalMapChunks[1, 1] ? false : true;
    }

    /// <summary>
    /// Необходимо вызывать, когда игрок переходит на другой хекс.
    /// </summary>
    void OnPlayerGotoHex(Vector2 mapCoords)
    {
        if (!IsCurrentMapLocal())
        {
            int chunkX = Mathf.FloorToInt(mapCoords.x / GlobalMapChunkSize), chunkY = Mathf.FloorToInt(mapCoords.y / GlobalMapChunkSize);

            if (chunkX != ChunkX || chunkY != ChunkY)
            {
                Debug.Log("Cashing chunks.");
                SaveCurrentChunkLocalMaps();
                if (!TryLoadFiledChunkLocalMaps(chunkY, chunkX))
                {
                    for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                        for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                            LocalMaps[y, x] = null;
                }

                GlobalMap[,] bufCashe = new GlobalMap[3, 3];
                for (sbyte y = -1; y < 2; ++y)
                    for (sbyte x = -1; x < 2; ++x)
                        bufCashe[1 + y, 1 + x] = GetChunk(chunkY - y, chunkX - x);

                CashedGlobalMapChunks = bufCashe;

                ChunkY = chunkY;
                ChunkX = chunkX;

                CurrentMap = CashedGlobalMapChunks[1, 1];
            }

            Visualiser.RenderVisibleHexes(mapCoords, Player.GetComponent<Player>().ViewDistance, CashedGlobalMapChunks, ChunkY, ChunkX);
            Visualiser.DestroyAllObjects();//TODO Временно
            for (byte i = 0; i < 6; ++i)
            {
                Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(mapCoords, (HexDirection)i), Visualiser.BlueHexSprite, false);
            }
        }
        else
        {
            Visualiser.DestroyAllObjects();//TODO Временно
            for (byte i = 0; i < 6; ++i)
            {
                if (IsHexFree(HexNavigHelper.GetNeighborMapCoords(mapCoords, (TurnedHexDirection)i)))
                    Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(mapCoords, (TurnedHexDirection)i), Visualiser.BlueHexSprite, true);
            }
        }
    }

    /// <summary>
    /// Переключение карт.
    /// </summary>
    public void SwitchMap()
    {
        Visualiser.DestroyAllHexes(); //TODO Это лучше делать до генерации карты, чтобы не было видно подвисания (или нужно отображение загрузки).
        if (!IsCurrentMapLocal())
        {
            GotoLocalMap();
            SpawnRandomEnemy();
            Player.transform.position = WorldVisualiser.GetTransformPosFromMapCoords(Player.GetComponent<Player>().MapCoords, true);
        }
        else
        {
            GotoGlobalMap();
            EventManager.OnLocalMapLeave();
            Player.transform.position = WorldVisualiser.GetTransformPosFromMapCoords(Player.GetComponent<Player>().MapCoords, false);
        }
        EventManager.OnPlayerObjectMoved();
    }

    /// <summary>
    /// Переход на локальную карту.
    /// </summary>
    void GotoLocalMap()
    {
        GlobalMapCoords = Player.GetComponent<Player>().MapCoords;
        Vector2 mapCoords = new Vector2(GlobalMapCoords.x - ChunkX * GlobalMapChunkSize, GlobalMapCoords.y - ChunkY * GlobalMapChunkSize);
        if (LocalMaps[(int)mapCoords.y, (int)mapCoords.x] == null)
            LocalMaps[(int)mapCoords.y, (int)mapCoords.x] = CreateLocalMap(CashedGlobalMapChunks[1, 1].GetHeight(mapCoords), CashedGlobalMapChunks[1, 1].GetForest(mapCoords), CashedGlobalMapChunks[1, 1].HasRiver(mapCoords), CashedGlobalMapChunks[1, 1].HasRoad(mapCoords), CashedGlobalMapChunks[1, 1].HasCluster(mapCoords));
        CurrentMap = LocalMaps[(int)mapCoords.y, (int)mapCoords.x];

        //TEMP
        Player.GetComponent<Player>().MapCoords.x = (ushort)LocalMapSize.x / 2;
        Player.GetComponent<Player>().MapCoords.y = (ushort)LocalMapSize.y / 2;
        //
        (CurrentMap as LocalMap).AddObject(Player.GetComponent<Entity>());

        Visualiser.DestroyAllObjects();
        Visualiser.RenderWholeMap(CurrentMap as LocalMap);

        for (byte i = 0; i < 6; ++i)
        {
            Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(Player.GetComponent<Player>().MapCoords, (TurnedHexDirection)i), Visualiser.BlueHexSprite, true);
        }
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
        for (byte i = 0; i < 6; ++i)
        {
            Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(Player.GetComponent<Player>().MapCoords, (HexDirection)i), Visualiser.BlueHexSprite, false);
        }
    }

    /// <summary>
    /// Создаёт локальную карту.
    /// </summary>
    /// <param name="coords">Координаты новой карты на глобальной.</param>
    LocalMap CreateLocalMap(float height, float forest, bool river, bool road, bool ruins)
    {
        LocalMap map = new LocalMap((ushort)LocalMapSize.x, (ushort)LocalMapSize.y);

        Vector2[] riverPath = new Vector2[] { new Vector2(0, (ushort)LocalMapSize.y / 2), new Vector2(LocalMapSize.x - 1, (ushort)LocalMapSize.y / 2) }; //UNDONE
        if (river)
            WorldGenerator.MakeEqualHeightLine(map.HeightMatrix, riverPath, Visualiser.LocalMapParam.Terrains[0].StartingHeight - 0.0001f);
        float?[,] buf = new float?[(ushort)LocalMapSize.y, (ushort)LocalMapSize.x];
        for (ushort y = 0; y < LocalMapSize.y; ++y)
            for (ushort x = 0; x < LocalMapSize.x; ++x)
                if (map.HeightMatrix[y, x] != 0)
                    buf[y, x] = map.HeightMatrix[y, x];
        WorldGenerator.CreateHeightmap(buf, LandscapeRoughness, height, height, height, height);
        for (ushort y = 0; y < LocalMapSize.y; ++y)
            for (ushort x = 0; x < LocalMapSize.x; ++x)
                map.HeightMatrix[y, x] = buf[y, x].Value;
        WorldGenerator.CreateHeightmap(map.ForestMatrix, ForestRoughness, forest, forest, forest, forest);

        return map;
    }

    /// <summary>
    /// Возвращает чанк по координатам.
    /// </summary>
    /// <returns>Чанк.</returns>
    /// <param name="chunkY">Координата по y.</param>
    /// <param name="chunkX">Координата по x.</param>
    GlobalMap GetChunk(int chunkY, int chunkX)
    {
        GlobalMap chunk;
        if (TryGetChunk(chunkY, chunkX, out chunk))
            return chunk;
        else
        {
            GlobalMap top, right, bottom, left;
            TryGetChunk(chunkY + 1, chunkX, out top);
            TryGetChunk(chunkY, chunkX + 1, out right);
            TryGetChunk(chunkY - 1, chunkX, out bottom);
            TryGetChunk(chunkY, chunkX - 1, out left);
            return AllocAndGenerateChunk(top, right, bottom, left);
        }
    }

    /// <summary>
    /// Пытается загрузить чанк по координатам.
    /// </summary>
    /// <returns><c>true</c>, если чанк загружен, иначе <c>false</c>.</returns>
    /// <param name="chunkY">Координата по y.</param>
    /// <param name="chunkX">Координата по x.</param>
    /// <param name="chunk">[out] Чанк.</param>
    bool TryGetChunk(int chunkY, int chunkX, out GlobalMap chunk)
    {
        if (Mathf.Abs(chunkY - ChunkY) <= 1 && Mathf.Abs(chunkX - ChunkX) <= 1)
        {
            for (sbyte y = -1; y < 2; ++y)
                for (sbyte x = -1; x < 2; ++x)
                    if (chunkY == ChunkY + y && chunkX == ChunkX + x)
                    {
                        chunk = CashedGlobalMapChunks[y + 1, x + 1];
                        return true;
                    }
            throw new System.Exception("This code line should not be reached!");
        }
        else
        {
            if (TryLoadFiledChunk(chunkY, chunkX, out chunk))
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// Пытается загрузить из файла чанк по координатам.
    /// </summary>
    /// <returns><c>true</c>, если чанк загружен, иначе <c>false</c>.</returns>
    /// <param name="chunkY">Координата по y.</param>
    /// <param name="chunkX">Координата по x.</param>
    /// <param name="chunk">[out] Чанк.</param>
    bool TryLoadFiledChunk(int chunkY, int chunkX, out GlobalMap chunk)
    {
        string filePath = Path.Combine(ChunksDirectoryPath, chunkY + "_" + chunkX);
        if (File.Exists(filePath))
        {
            chunk = new GlobalMap(GlobalMapChunkSize, GlobalMapChunkSize);
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                chunk.Read(reader);
            }
            return true;
        }
        chunk = null;
        return false;
    }

    /// <summary>
    /// Сохраняет чанк в файл.
    /// </summary>
    /// <param name="chunkY">Координата по y.</param>
    /// <param name="chunkX">Координата по x.</param>
    /// <param name="chunk">Чанк.</param>
    void SaveChunk(int chunkY, int chunkX, GlobalMap chunk)
    {
        Directory.CreateDirectory(ChunksDirectoryPath);
        using (BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine(ChunksDirectoryPath, chunkY + "_" + chunkX), FileMode.Create)))
        {
            chunk.Write(writer);
        }
    }

    /// <summary>
    /// Пытается загрузить локальные карты чанка из файла.
    /// </summary>
    /// <returns><c>true</c>, если карты загружены, иначе <c>false</c>.</returns>
    /// <param name="chunkY">Координата по y.</param>
    /// <param name="chunkX">Координата по x.</param>
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
                            LocalMaps[y, x].Read(reader);
                        //...

                    }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Сохраняет локальные карты текущего чанка в файл.
    /// </summary>
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
                        LocalMaps[y, x].Write(writer);
                    }

                    //...
                }
        }
    }

    void SpawnRandomEnemy()
    {
        Vector2 mapCoords = new Vector2(Random.Range(0, (int)LocalMapSize.x), Random.Range(0, (int)LocalMapSize.y));
        GameObject enemy = MonoBehaviour.Instantiate(Enemies[Random.Range(0, Enemies.Length)], WorldVisualiser.GetTransformPosFromMapCoords(mapCoords, true), Quaternion.identity) as GameObject;
        enemy.GetComponent<Creature>().MapCoords = mapCoords;
        enemy.GetComponent<Creature>().Attack(Player);
        (CurrentMap as LocalMap).AddObject(enemy.GetComponent<Entity>());
    }

    public bool IsHexFree(Vector2 mapCoords)
    {
        if (IsCurrentMapLocal())//Временно
        {
            if (mapCoords.x >= 0 && mapCoords.x < LocalMapSize.x && mapCoords.y >= 0 && mapCoords.y < LocalMapSize.y)
                return !(CurrentMap as LocalMap).IsBlocked(mapCoords);
            return false;
        }
        return true;
    }

    GlobalMap AllocAndGenerateChunk(GlobalMap topChunk, GlobalMap rightChunk, GlobalMap bottomChunk, GlobalMap leftChunk)
    {
        // TODO Ждём MonoDevelop 6.0 ( ?., ?[ ):
        GlobalMap chunk = new GlobalMap(GlobalMapChunkSize, GlobalMapChunkSize);
        WorldGenerator.CreateHeightmap(chunk.HeightMatrix, LandscapeRoughness, ((leftChunk != null ? leftChunk.HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1] : Random.value) + (topChunk != null ? topChunk.HeightMatrix[0, 0] : Random.value)) / 2f, ((topChunk != null ? topChunk.HeightMatrix[0, GlobalMapChunkSize - 1] : Random.value) + (rightChunk != null ? rightChunk.HeightMatrix[GlobalMapChunkSize - 1, 0] : Random.value)) / 2f, ((leftChunk != null ? leftChunk.HeightMatrix[0, GlobalMapChunkSize - 1] : Random.value) + (bottomChunk != null ? bottomChunk.HeightMatrix[GlobalMapChunkSize - 1, 0] : Random.value)) / 2f, ((bottomChunk != null ? bottomChunk.HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1] : Random.value) + (rightChunk != null ? rightChunk.HeightMatrix[0, 0] : Random.value)) / 2f);
        for (ushort y = 0; y < GlobalMapChunkSize; ++y)
            for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                chunk.HeightMatrix[y, x] = Mathf.Clamp(chunk.HeightMatrix[y, x], 0, Mathf.Abs(chunk.HeightMatrix[y, x]));
        WorldGenerator.CreateHeightmap(chunk.ForestMatrix, ForestRoughness, ((leftChunk != null ? leftChunk.ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1] : Random.value) + (topChunk != null ? topChunk.ForestMatrix[0, 0] : Random.value)) / 2f, ((topChunk != null ? topChunk.ForestMatrix[0, GlobalMapChunkSize - 1] : Random.value) + (rightChunk != null ? rightChunk.ForestMatrix[GlobalMapChunkSize - 1, 0] : Random.value)) / 2f, ((leftChunk != null ? leftChunk.ForestMatrix[0, GlobalMapChunkSize - 1] : Random.value) + (bottomChunk != null ? bottomChunk.ForestMatrix[GlobalMapChunkSize - 1, 0] : Random.value)) / 2f, ((bottomChunk != null ? bottomChunk.ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1] : Random.value) + (rightChunk != null ? rightChunk.ForestMatrix[0, 0] : Random.value)) / 2f);
        for (ushort y = 0; y < GlobalMapChunkSize; ++y)
            for (ushort x = 0; x < GlobalMapChunkSize; ++x)
            {
                chunk.ForestMatrix[y, x] *= ForestDensity;
                chunk.ForestMatrix[y, x] = Mathf.Clamp(chunk.ForestMatrix[y, x], 0, Mathf.Abs(chunk.ForestMatrix[y, x]));
            }
        chunk.Rivers = WorldGenerator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix, RiverParam);
        chunk.Clusters = WorldGenerator.CreateClusters(chunk, ClusterParam);
        chunk.Roads = WorldGenerator.CreateRoads(chunk.HeightMatrix, chunk.RoadMatrix, chunk.Clusters);
        CalculateChunkTerrains(chunk);
        return chunk;
    }

    void CalculateChunkTerrains(GlobalMap chunk)
    {
        for (ushort y = 0; y < GlobalMapChunkSize; ++y)
            for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                chunk.TerrainMatrix[y, x] = MakeTerrainFromHeight(chunk.HeightMatrix[y, x]) | (chunk.ForestMatrix[y, x] > TreeCountForForestTerrain ? TerrainType.FOREST : TerrainType.NONE) | (chunk.RiverMatrix[y, x] ? TerrainType.RIVER : TerrainType.NONE);
    }

    TerrainType MakeTerrainFromHeight(float height)
    {
        return TerrainType.MEADOW; //UNDONE
    }

    //TEST
    public void EnemyAttack()
    {
        SwitchMap();
    }

    public TerrainType GetHexTerrain(Vector2 hexCoords)
    {
        return (CurrentMap as GlobalMap).GetTerrainType(new Vector2(hexCoords.x - ChunkX * GlobalMapChunkSize, hexCoords.y - ChunkY * GlobalMapChunkSize));
    }
}
