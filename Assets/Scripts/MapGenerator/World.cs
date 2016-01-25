using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Это класс карты, хранит в себе слои в виде матриц
public class Map
{
    public float[,] HeightMatrix;
    public float[,] ForestMatrix;
    public bool[,] RiverMatrix;
    public bool[,] BlockMatrix;

    public Map(ushort width, ushort height)
    {
        HeightMatrix = new float[height, width];
        ForestMatrix = new float[height, width];
        RiverMatrix = new bool[height, width];
        BlockMatrix = new bool[height, width];
    }
}

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

    EventManager EventManager;

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
        EventManager = GameObject.Find("EventManager").GetComponent<EventManager>();

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

        CashedGlobalMapChunks[1, 1] = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(CashedGlobalMapChunks[1, 1].HeightMatrix, Generator.LandscapeRoughness, Random.value, Random.value, Random.value, Random.value);
        Generator.CreateHeightmap(CashedGlobalMapChunks[1, 1].ForestMatrix, Generator.ForestRoughness, Random.value, Random.value, Random.value, Random.value);
        Generator.CreateRivers(CashedGlobalMapChunks[1, 1].HeightMatrix, CashedGlobalMapChunks[1, 1].RiverMatrix);

        CashedGlobalMapChunks[0, 0] = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(CashedGlobalMapChunks[0, 0].HeightMatrix, Generator.LandscapeRoughness, Random.value, CashedGlobalMapChunks[1, 1].HeightMatrix[0, 0], Random.value, Random.value);
        Generator.CreateHeightmap(CashedGlobalMapChunks[0, 0].ForestMatrix, Generator.ForestRoughness, Random.value, CashedGlobalMapChunks[1, 1].ForestMatrix[0, 0], Random.value, Random.value);
        Generator.CreateRivers(CashedGlobalMapChunks[0, 0].HeightMatrix, CashedGlobalMapChunks[0, 0].RiverMatrix);

        CashedGlobalMapChunks[0, 1] = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(CashedGlobalMapChunks[0, 1].HeightMatrix, Generator.LandscapeRoughness, CashedGlobalMapChunks[1, 1].HeightMatrix[0, 0], CashedGlobalMapChunks[1, 1].HeightMatrix[0, GlobalMapChunkSize - 1], CashedGlobalMapChunks[0, 0].HeightMatrix[0, GlobalMapChunkSize - 1], Random.value);
        Generator.CreateHeightmap(CashedGlobalMapChunks[0, 1].ForestMatrix, Generator.ForestRoughness, CashedGlobalMapChunks[1, 1].ForestMatrix[0, 0], CashedGlobalMapChunks[1, 1].ForestMatrix[0, GlobalMapChunkSize - 1], CashedGlobalMapChunks[0, 0].ForestMatrix[0, GlobalMapChunkSize - 1], Random.value);
        Generator.CreateRivers(CashedGlobalMapChunks[0, 1].HeightMatrix, CashedGlobalMapChunks[0, 1].RiverMatrix);

        CashedGlobalMapChunks[0, 2] = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(CashedGlobalMapChunks[0, 2].HeightMatrix, Generator.LandscapeRoughness, CashedGlobalMapChunks[1, 1].HeightMatrix[0, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks[0, 1].HeightMatrix[0, GlobalMapChunkSize - 1], Random.value);
        Generator.CreateHeightmap(CashedGlobalMapChunks[0, 2].ForestMatrix, Generator.ForestRoughness, CashedGlobalMapChunks[1, 1].ForestMatrix[0, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks[0, 1].ForestMatrix[0, GlobalMapChunkSize - 1], Random.value);
        Generator.CreateRivers(CashedGlobalMapChunks[0, 2].HeightMatrix, CashedGlobalMapChunks[0, 2].RiverMatrix);

        CashedGlobalMapChunks[1, 0] = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(CashedGlobalMapChunks[1, 0].HeightMatrix, Generator.LandscapeRoughness, Random.value, CashedGlobalMapChunks[1, 1].HeightMatrix[GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks[0, 0].HeightMatrix[GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks[1, 1].HeightMatrix[0, 0]);
        Generator.CreateHeightmap(CashedGlobalMapChunks[1, 0].ForestMatrix, Generator.ForestRoughness, Random.value, CashedGlobalMapChunks[1, 1].ForestMatrix[GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks[0, 0].ForestMatrix[GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks[1, 1].ForestMatrix[0, 0]);
        Generator.CreateRivers(CashedGlobalMapChunks[1, 0].HeightMatrix, CashedGlobalMapChunks[1, 0].RiverMatrix);

        CashedGlobalMapChunks[1, 2] = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(CashedGlobalMapChunks[1, 2].HeightMatrix, Generator.LandscapeRoughness, CashedGlobalMapChunks[1, 1].HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks[1, 1].HeightMatrix[0, GlobalMapChunkSize - 1], CashedGlobalMapChunks[0, 2].HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
        Generator.CreateHeightmap(CashedGlobalMapChunks[1, 2].ForestMatrix, Generator.ForestRoughness, CashedGlobalMapChunks[1, 1].ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks[1, 1].ForestMatrix[0, GlobalMapChunkSize - 1], CashedGlobalMapChunks[0, 2].ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
        Generator.CreateRivers(CashedGlobalMapChunks[1, 2].HeightMatrix, CashedGlobalMapChunks[1, 2].RiverMatrix);

        CashedGlobalMapChunks[2, 0] = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(CashedGlobalMapChunks[2, 0].HeightMatrix, Generator.LandscapeRoughness, Random.value, Random.value, CashedGlobalMapChunks[1, 0].HeightMatrix[GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks[1, 0].HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
        Generator.CreateHeightmap(CashedGlobalMapChunks[2, 0].ForestMatrix, Generator.ForestRoughness, Random.value, Random.value, CashedGlobalMapChunks[1, 0].ForestMatrix[GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks[1, 0].ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
        Generator.CreateRivers(CashedGlobalMapChunks[2, 0].HeightMatrix, CashedGlobalMapChunks[2, 0].RiverMatrix);

        CashedGlobalMapChunks[2, 1] = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(CashedGlobalMapChunks[2, 1].HeightMatrix, Generator.LandscapeRoughness, CashedGlobalMapChunks[2, 0].HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks[1, 1].HeightMatrix[GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks[1, 1].HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
        Generator.CreateHeightmap(CashedGlobalMapChunks[2, 1].ForestMatrix, Generator.ForestRoughness, CashedGlobalMapChunks[2, 0].ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks[1, 1].ForestMatrix[GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks[1, 1].ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
        Generator.CreateRivers(CashedGlobalMapChunks[2, 1].HeightMatrix, CashedGlobalMapChunks[2, 1].RiverMatrix);

        CashedGlobalMapChunks[2, 2] = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
        Generator.CreateHeightmap(CashedGlobalMapChunks[2, 2].HeightMatrix, Generator.LandscapeRoughness, CashedGlobalMapChunks[2, 1].HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks[2, 1].HeightMatrix[0, GlobalMapChunkSize - 1], CashedGlobalMapChunks[1, 2].HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
        Generator.CreateHeightmap(CashedGlobalMapChunks[2, 2].ForestMatrix, Generator.ForestRoughness, Random.value, Random.value, Random.value, Random.value);
        Generator.CreateRivers(CashedGlobalMapChunks[2, 2].HeightMatrix, CashedGlobalMapChunks[2, 2].RiverMatrix);

        LocalMaps = new Map[GlobalMapChunkSize, GlobalMapChunkSize];

        CurrentMap = CashedGlobalMapChunks[1, 1];

        Player = GameObject.FindWithTag("Player");
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
            //GameObject.FindWithTag("AI_Controller").GetComponent<AI_Controller>().AddEnemy(new Vector2(1,1));
        }
        else
        {
            GotoGlobalMap();
            EventManager.OnLocalMapLeave();
            //GameObject.FindWithTag("AI_Controller").GetComponent<AI_Controller>().DeleteAll();

        }
    }

    /// <summary>
    /// Переход на локальную карту.
    /// </summary>
    void GotoLocalMap()
    {
        Vector2 mapCoords = Player.GetComponent<Player>().MapCoords;
        if (LocalMaps[(int)mapCoords.y, (int)mapCoords.x] == null)
            CreateLocalMap(mapCoords, CashedGlobalMapChunks[1, 1].HeightMatrix[(int)mapCoords.y, (int)mapCoords.x], CashedGlobalMapChunks[1, 1].ForestMatrix[(int)mapCoords.y, (int)mapCoords.x]);
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
            Vector2 topLeft, topRight, bottomLeft, bottomRight; // x - Height, y - Forest, River - ? TODO?
            Map neighbChunk = new Map(GlobalMapChunkSize, GlobalMapChunkSize);
            if (TryGetChunk(chunkY, chunkX - 1, neighbChunk))
            {
                topLeft.x = neighbChunk.HeightMatrix[0, GlobalMapChunkSize - 1];
                topLeft.y = neighbChunk.ForestMatrix[0, GlobalMapChunkSize - 1];
                bottomLeft.x = neighbChunk.HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
                bottomLeft.y = neighbChunk.ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
                if (TryGetChunk(chunkY, chunkX + 1, neighbChunk))
                {
                    topRight.x = neighbChunk.HeightMatrix[0, 0];
                    topRight.y = neighbChunk.ForestMatrix[0, 0];
                    bottomRight.x = neighbChunk.HeightMatrix[GlobalMapChunkSize - 1, 0];
                    bottomRight.y = neighbChunk.ForestMatrix[GlobalMapChunkSize - 1, 0];
                    Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                    Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                    Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                    return chunk;
                }
                else
                {
                    if (TryGetChunk(chunkY + 1, chunkX, neighbChunk))
                    {
                        topRight.x = neighbChunk.HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
                        topRight.y = neighbChunk.ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
                        if (TryGetChunk(chunkY - 1, chunkX, neighbChunk))
                        {
                            bottomRight.x = neighbChunk.HeightMatrix[0, GlobalMapChunkSize - 1];
                            bottomRight.y = neighbChunk.ForestMatrix[0, GlobalMapChunkSize - 1];
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                        else
                        {
                            bottomRight.x = Random.value;
                            bottomRight.y = Random.value;
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                    }
                    else
                    {
                        topRight.x = Random.value;
                        topRight.y = Random.value;
                        if (TryGetChunk(chunkY - 1, chunkX, neighbChunk))
                        {
                            bottomRight.x = neighbChunk.HeightMatrix[0, GlobalMapChunkSize - 1];
                            bottomRight.y = neighbChunk.ForestMatrix[0, GlobalMapChunkSize - 1];
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                        else
                        {
                            bottomRight.x = Random.value;
                            bottomRight.y = Random.value;
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                    }
                }
            }
            else
            {
                if (TryGetChunk(chunkY, chunkX + 1, neighbChunk))
                {
                    topRight.x = neighbChunk.HeightMatrix[0, 0];
                    topRight.y = neighbChunk.ForestMatrix[0, 0];
                    bottomRight.x = neighbChunk.HeightMatrix[GlobalMapChunkSize - 1, 0];
                    bottomRight.y = neighbChunk.ForestMatrix[GlobalMapChunkSize - 1, 0];
                    if (TryGetChunk(chunkY + 1, chunkX, neighbChunk))
                    {
                        topLeft.x = neighbChunk.HeightMatrix[GlobalMapChunkSize - 1, 0];
                        topLeft.y = neighbChunk.ForestMatrix[GlobalMapChunkSize - 1, 0];
                        if (TryGetChunk(chunkY - 1, chunkX, neighbChunk))
                        {
                            bottomLeft.x = neighbChunk.HeightMatrix[0, 0];
                            bottomLeft.y = neighbChunk.ForestMatrix[0, 0];
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                        else
                        {
                            bottomLeft.x = Random.value;
                            bottomLeft.y = Random.value;
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                    }
                    else
                    {
                        topLeft.x = Random.value;
                        topLeft.y = Random.value;
                        if (TryGetChunk(chunkY - 1, chunkX, neighbChunk))
                        {
                            bottomLeft.x = neighbChunk.HeightMatrix[0, 0];
                            bottomLeft.y = neighbChunk.ForestMatrix[0, 0];
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                        else
                        {
                            bottomLeft.x = Random.value;
                            bottomLeft.y = Random.value;
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                    }
                }
                else
                {
                    if (TryGetChunk(chunkY + 1, chunkX, neighbChunk))
                    {
                        topLeft.x = neighbChunk.HeightMatrix[GlobalMapChunkSize - 1, 0];
                        topLeft.y = neighbChunk.ForestMatrix[GlobalMapChunkSize - 1, 0];
                        topRight.x = neighbChunk.HeightMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
                        topRight.y = neighbChunk.ForestMatrix[GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
                        if (TryGetChunk(chunkY - 1, chunkX, neighbChunk))
                        {
                            bottomLeft.x = neighbChunk.HeightMatrix[0, 0];
                            bottomLeft.y = neighbChunk.ForestMatrix[0, 0];
                            bottomRight.x = neighbChunk.HeightMatrix[0, GlobalMapChunkSize - 1];
                            bottomRight.y = neighbChunk.ForestMatrix[0, GlobalMapChunkSize - 1];
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                        else
                        {
                            bottomLeft.x = Random.value;
                            bottomLeft.y = Random.value;
                            bottomRight.x = Random.value;
                            bottomRight.y = Random.value;
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                    }
                    else
                    {
                        topLeft.x = Random.value;
                        topLeft.y = Random.value;
                        topRight.x = Random.value;
                        topRight.y = Random.value;
                        if (TryGetChunk(chunkY - 1, chunkX, neighbChunk))
                        {
                            bottomLeft.x = neighbChunk.HeightMatrix[0, 0];
                            bottomLeft.y = neighbChunk.ForestMatrix[0, 0];
                            bottomRight.x = neighbChunk.HeightMatrix[0, GlobalMapChunkSize - 1];
                            bottomRight.y = neighbChunk.ForestMatrix[0, GlobalMapChunkSize - 1];
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                        else
                        {
                            bottomLeft.x = Random.value;
                            bottomLeft.y = Random.value;
                            bottomRight.x = Random.value;
                            bottomRight.y = Random.value;
                            Generator.CreateHeightmap(chunk.HeightMatrix, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
                            Generator.CreateHeightmap(chunk.ForestMatrix, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
                            Generator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix);
                            return chunk;
                        }
                    }
                }
            }
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
                        chunk.ForestMatrix[y, x] = reader.ReadSingle();

                for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                    for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                        chunk.RiverMatrix[y, x] = reader.ReadBoolean();
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
                    writer.Write(chunk.ForestMatrix[y, x]);

            for (ushort y = 0; y < GlobalMapChunkSize; ++y)
                for (ushort x = 0; x < GlobalMapChunkSize; ++x)
                    writer.Write(chunk.RiverMatrix[y, x]);
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
                return !CurrentMap.BlockMatrix[(int)mapCoords.y, (int)mapCoords.x];
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
}
