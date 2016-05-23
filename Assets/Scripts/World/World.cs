using UnityEngine;
using System.IO;

public class World
{
    public readonly float LandscapeRoughness;
    public readonly float ForestRoughness;

    public readonly WorldGenerator.RiversParameters RiverParam;
    public readonly WorldGenerator.ClustersParameters ClusterParam;
    public readonly WorldGenerator.RoadsParameters RoadParam;

    public readonly ushort ChunkSize;
    public readonly LocalPos LocalMapSize;

    public readonly byte ForestDensity;
    public readonly byte TreeCountForForestTerrain;

    public readonly GameObject[] Enemies;

    public Map CurrentMap { get; private set; } //TODO Возможно, можно будет убрать. Карта, на которой находится игрок.

    Chunk[,] CashedChunks = new Chunk[3, 3];
    LocalMap[,] LocalMaps;

    WorldVisualiser Visualiser; //Временно

    GlobalPos GlobalMapPos;
    GameObject Player;
    int ChunkX, ChunkY;
    const string ChunksDirectoryName = "chunks";
    string ChunksDirectoryPath;

    public World(float landscapeRoughness, float forestRoughness, WorldGenerator.RiversParameters riverParam, WorldGenerator.ClustersParameters clusterParam, WorldGenerator.RoadsParameters roadParam, ushort globalMapChunkSize, LocalPos localMapSize, byte forestDensity, byte treeCountForForestTerrain, GameObject[] enemies)
    {
        LandscapeRoughness = landscapeRoughness;
        ForestRoughness = forestRoughness;
        RiverParam = riverParam;
        ClusterParam = clusterParam;
        RoadParam = roadParam;
        ChunkSize = globalMapChunkSize;
        LocalMapSize = localMapSize;
        ForestDensity = forestDensity;
        TreeCountForForestTerrain = treeCountForForestTerrain;
        Enemies = enemies;

        ChunksDirectoryPath = Path.Combine(Application.streamingAssetsPath, ChunksDirectoryName); //UNDONE Не будет работать на Android?

        if (Directory.Exists(ChunksDirectoryPath))
            Directory.Delete(ChunksDirectoryPath, true);

        Visualiser = GameObject.FindWithTag("World").GetComponent<WorldVisualiser>(); //Временно

        // Это всё временно, как пример. На самом деле карта должна создаваться только при начале новой игры, иначе загружаться из сохранения.
        //--
        ChunkX = ChunkY = 0;

        CashedChunks[1, 1] = AllocAndGenerateChunk(CashedChunks[2, 1], CashedChunks[1, 2], CashedChunks[0, 1], CashedChunks[1, 0]);
        CashedChunks[1, 0] = AllocAndGenerateChunk(CashedChunks[2, 0], CashedChunks[0, 0], CashedChunks[0, 1], null);
        CashedChunks[2, 1] = AllocAndGenerateChunk(null, CashedChunks[2, 2], CashedChunks[1, 1], CashedChunks[2, 0]);
        CashedChunks[1, 2] = AllocAndGenerateChunk(CashedChunks[2, 2], null, CashedChunks[0, 2], CashedChunks[1, 1]);
        CashedChunks[0, 1] = AllocAndGenerateChunk(CashedChunks[1, 1], CashedChunks[0, 2], null, CashedChunks[0, 0]);
        CashedChunks[0, 0] = AllocAndGenerateChunk(CashedChunks[1, 0], CashedChunks[0, 1], null, null);
        CashedChunks[0, 2] = AllocAndGenerateChunk(CashedChunks[1, 2], null, null, CashedChunks[0, 1]);
        CashedChunks[2, 0] = AllocAndGenerateChunk(null, CashedChunks[2, 1], CashedChunks[1, 0], null);
        CashedChunks[2, 2] = AllocAndGenerateChunk(null, null, CashedChunks[1, 2], CashedChunks[2, 1]);

        LocalMaps = new LocalMap[ChunkSize, ChunkSize];

        CurrentMap = CashedChunks[1, 1];

        Player = GameObject.FindWithTag("Player");
        Player.GetComponent<Player>().GlobalPos.X = 5;
        Player.GetComponent<Player>().GlobalPos.Y = 5;
        Player.transform.position = WorldVisualiser.GetTransformPosFromMapPos(Player.GetComponent<Player>().GlobalPos);
        Camera.main.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y + Camera.main.transform.position.z * (Mathf.Tan((360 - Camera.main.transform.rotation.eulerAngles.x) / 57.3f)), Camera.main.transform.position.z);
        Visualiser.RenderVisibleHexes(Player.GetComponent<Player>().GlobalPos, Player.GetComponent<Player>().ViewDistance, CashedChunks, ChunkY, ChunkX);
        for (byte i = 0; i < 6; ++i)
            Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(Player.GetComponent<Player>().GlobalPos, (HexDirection)i), Visualiser.BlueHexSprite);
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
    public bool IsCurrentMapLocal() //C#6.0 EBD
    {
        return CurrentMap == CashedChunks[1, 1] ? false : true;
    }

    /// <summary>
    /// Необходимо вызывать, когда игрок переходит на другой хекс.
    /// </summary>
    void OnPlayerGotoHex(GlobalPos pos)
    {
        if (!IsCurrentMapLocal())
        {
            int chunkX = Mathf.FloorToInt((float)pos.X / ChunkSize), chunkY = Mathf.FloorToInt((float)pos.Y / ChunkSize);

            if (chunkX != ChunkX || chunkY != ChunkY)
            {
                Debug.Log("Cashing chunks.");

                sbyte dx = (sbyte)(ChunkX - chunkX), dy = (sbyte)(ChunkY - chunkY);
                if (dx != 0)
                    for (sbyte y = -1; y < 2; ++y)
                        SaveChunk(ChunkY + y, ChunkX + dx, CashedChunks[y + 1, dx + 1]);
                if (dy != 0)
                    for (sbyte x = -1; x < 2; ++x)
                        SaveChunk(ChunkY + dy, ChunkX + x, CashedChunks[dy + 1, x + 1]);
                SaveCurrentChunkLocalMaps();

                Chunk[,] bufCashe = new Chunk[3, 3];
                for (sbyte y = -1; y < 2; ++y)
                    for (sbyte x = -1; x < 2; ++x)
                        bufCashe[y + 1, x + 1] = GetChunk(chunkY + y, chunkX + x);
                if (!TryLoadFiledChunkLocalMaps(chunkY, chunkX))
                    for (ushort y = 0; y < ChunkSize; ++y)
                        for (ushort x = 0; x < ChunkSize; ++x)
                            LocalMaps[y, x] = null;

                CashedChunks = bufCashe;

                ChunkY = chunkY;
                ChunkX = chunkX;

                CurrentMap = CashedChunks[1, 1];
            }

            Visualiser.RenderVisibleHexes(pos, Player.GetComponent<Player>().ViewDistance, CashedChunks, ChunkY, ChunkX);
            Visualiser.DestroyAllObjects();//TODO Временно
            for (byte i = 0; i < 6; ++i)
                Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(pos, (HexDirection)i), Visualiser.BlueHexSprite);
        }
        else
        {
            Visualiser.DestroyAllObjects();//TODO Временно
            for (byte i = 0; i < 6; ++i)
                if (IsHexFree((LocalPos)HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i)))
                    Visualiser.HighlightHex((LocalPos)HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i), Visualiser.BlueHexSprite);
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
            Player.transform.position = WorldVisualiser.GetTransformPosFromMapPos((LocalPos)Player.GetComponent<Player>().GlobalPos);
        }
        else
        {
            GotoGlobalMap();
            EventManager.OnLocalMapLeave();
            Player.transform.position = WorldVisualiser.GetTransformPosFromMapPos(Player.GetComponent<Player>().GlobalPos);
        }
        EventManager.OnPlayerObjectMoved();
    }

    /// <summary>
    /// Переход на локальную карту.
    /// </summary>
    void GotoLocalMap()
    {
        GlobalMapPos = Player.GetComponent<Player>().GlobalPos;
        LocalPos pos = new LocalPos((ushort)(GlobalMapPos.X - ChunkX * ChunkSize), (ushort)(GlobalMapPos.Y - ChunkY * ChunkSize)); //TODO new?
        if (LocalMaps[pos.Y, pos.X] == null)
            LocalMaps[pos.Y, pos.X] = CreateLocalMap(CashedChunks[1, 1].HeightMatrix[pos.Y, pos.X], CashedChunks[1, 1].ForestMatrix[pos.Y, pos.X], CashedChunks[1, 1].RiverMatrix[pos.Y, pos.X], CashedChunks[1, 1].RoadMatrix[pos.Y, pos.X], CashedChunks[1, 1].ClusterMatrix[pos.Y, pos.X]);
        CurrentMap = LocalMaps[pos.Y, pos.X];

        //TEMP
        Player.GetComponent<Player>().Pos.X = (ushort)(LocalMapSize.X >> 1); //TODO Временно приведение
        Player.GetComponent<Player>().Pos.Y = (ushort)(LocalMapSize.Y >> 1);
        Player.GetComponent<Player>().GlobalPos.X = (ushort)(LocalMapSize.X >> 1);
        Player.GetComponent<Player>().GlobalPos.Y = (ushort)(LocalMapSize.Y >> 1);
        //
        (CurrentMap as LocalMap).AddObject(Player.GetComponent<Entity>());

        Visualiser.DestroyAllObjects();
        Visualiser.RenderWholeMap(CurrentMap as LocalMap);

        for (byte i = 0; i < 6; ++i)
            Visualiser.HighlightHex((LocalPos)HexNavigHelper.GetNeighborMapCoords(Player.GetComponent<Player>().Pos, (TurnedHexDirection)i), Visualiser.BlueHexSprite);
    }

    /// <summary>
    /// Переход на глобальную карту.
    /// </summary>
    void GotoGlobalMap()
    {
        CurrentMap = CashedChunks[1, 1];
        Player.GetComponent<Player>().GlobalPos = GlobalMapPos;
        Visualiser.RenderVisibleHexes(Player.GetComponent<Player>().GlobalPos, Player.GetComponent<Player>().ViewDistance, CashedChunks, ChunkY, ChunkX);

        Visualiser.DestroyAllObjects();
        for (byte i = 0; i < 6; ++i)
            Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(Player.GetComponent<Player>().GlobalPos, (HexDirection)i), Visualiser.BlueHexSprite);
    }

    /// <summary>
    /// Создаёт локальную карту.
    /// </summary>
    /// <param name="coords">Координаты новой карты на глобальной.</param>
    LocalMap CreateLocalMap(float height, float forest, bool river, bool road, bool ruins)
    {
        LocalMap map = new LocalMap(LocalMapSize.X, LocalMapSize.Y);

        if (river)
        {
            LocalPos[] riverPath = new LocalPos[] { new LocalPos(0, (ushort)(LocalMapSize.Y >> 1)), new LocalPos((ushort)(LocalMapSize.X - 1), (ushort)(LocalMapSize.Y >> 1)) }; //UNDONE			
            WorldGenerator.MakeEqualHeightLine(map.HeightMatrix, riverPath, Visualiser.LocalMapParam.Terrains[0].StartingHeight - 0.0001f);
        }
        float?[,] buf = new float?[LocalMapSize.Y, LocalMapSize.X];
        for (ushort y = 0; y < LocalMapSize.Y; ++y)
            for (ushort x = 0; x < LocalMapSize.X; ++x)
                if (map.HeightMatrix[y, x] != 0)
                    buf[y, x] = map.HeightMatrix[y, x];

        WorldGenerator.HeighmapNeighboring hmNb = new WorldGenerator.HeighmapNeighboring();
        hmNb.Left = new float[LocalMapSize.Y];
        hmNb.Top = new float[LocalMapSize.X];
        hmNb.Right = new float[LocalMapSize.Y];
        hmNb.Bottom = new float[LocalMapSize.X];
        for (ushort i = 0; i < LocalMapSize.Y; ++i)
            hmNb.Right[i] = hmNb.Left[i] = height;
        for (ushort i = 0; i < LocalMapSize.X; ++i)
            hmNb.Bottom[i] = hmNb.Top[i] = height;
        WorldGenerator.CreateHeightmap(ref buf, LandscapeRoughness, hmNb);

        for (ushort y = 0; y < LocalMapSize.Y; ++y)
            for (ushort x = 0; x < LocalMapSize.X; ++x)
            {
                map.HeightMatrix[y, x] = buf[y, x].Value;
                buf[y, x] = null;
            }

        for (ushort i = 0; i < LocalMapSize.Y; ++i)
            hmNb.Right[i] = hmNb.Left[i] = forest;
        for (ushort i = 0; i < LocalMapSize.X; ++i)
            hmNb.Bottom[i] = hmNb.Top[i] = forest;
        WorldGenerator.CreateHeightmap(ref buf, ForestRoughness, hmNb);

        for (ushort y = 0; y < LocalMapSize.Y; ++y)
            for (ushort x = 0; x < LocalMapSize.X; ++x)
                map.ForestMatrix[y, x] = buf[y, x].Value;

        return map;
    }

    /// <summary>
    /// Возвращает чанк по координатам.
    /// </summary>
    /// <returns>Чанк.</returns>
    /// <param name="chunkY">Координата по y.</param>
    /// <param name="chunkX">Координата по pos.X.</param>
    Chunk GetChunk(int chunkY, int chunkX)
    {
        Chunk chunk;
        if (TryGetChunk(chunkY, chunkX, out chunk))
            return chunk;

        Chunk top, right, bottom, left;
        TryGetChunk(chunkY + 1, chunkX, out top);
        TryGetChunk(chunkY, chunkX + 1, out right);
        TryGetChunk(chunkY - 1, chunkX, out bottom);
        TryGetChunk(chunkY, chunkX - 1, out left);
        return AllocAndGenerateChunk(top, right, bottom, left);
    }

    /// <summary>
    /// Пытается загрузить чанк по координатам.
    /// </summary>
    /// <returns><c>true</c>, если чанк загружен, иначе <c>false</c>.</returns>
    /// <param name="chunkY">Координата по y.</param>
    /// <param name="chunkX">Координата по pos.X.</param>
    /// <param name="chunk">[out] Чанк.</param>
    bool TryGetChunk(int chunkY, int chunkX, out Chunk chunk)
    {
        if (Mathf.Abs(chunkY - ChunkY) <= 1 && Mathf.Abs(chunkX - ChunkX) <= 1)
        {
            for (sbyte y = -1; y < 2; ++y)
                for (sbyte x = -1; x < 2; ++x)
                    if (chunkY == ChunkY + y && chunkX == ChunkX + x)
                    {
                        chunk = CashedChunks[y + 1, x + 1];
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
    /// <param name="chunkX">Координата по pos.X.</param>
    /// <param name="chunk">[out] Чанк.</param>
    bool TryLoadFiledChunk(int chunkY, int chunkX, out Chunk chunk)
    {
        string filePath = Path.Combine(ChunksDirectoryPath, chunkY + "_" + chunkX);
        if (File.Exists(filePath))
        {
            chunk = new Chunk(ChunkSize, ChunkSize);
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
    /// <param name="chunkX">Координата по pos.X.</param>
    /// <param name="chunk">Чанк.</param>
    void SaveChunk(int chunkY, int chunkX, Chunk chunk)
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
    /// <param name="chunkX">Координата по pos.X.</param>
    bool TryLoadFiledChunkLocalMaps(int chunkY, int chunkX)
    {
        string filePath = Path.Combine(ChunksDirectoryPath, chunkY + "_" + chunkX + "lm");
        if (File.Exists(filePath))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                for (ushort y = 0; y < ChunkSize; ++y)
                    for (ushort x = 0; x < ChunkSize; ++x)
                        if (reader.ReadString() == "null")
                            LocalMaps[y, x] = null;
                        else
                            LocalMaps[y, x].Read(reader);
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
            for (ushort y = 0; y < ChunkSize; ++y)
                for (ushort x = 0; x < ChunkSize; ++x)
                    if (LocalMaps[y, x] == null)
                        writer.Write("null");
                    else
                    {
                        writer.Write("notnull");
                        LocalMaps[y, x].Write(writer);
                    }
        }
    }

    void SpawnRandomEnemy()
    {
        LocalPos pos = new LocalPos((ushort)Random.Range(0, LocalMapSize.X), (ushort)Random.Range(0, LocalMapSize.Y));
        GameObject enemy = MonoBehaviour.Instantiate(Enemies[Random.Range(0, Enemies.Length)], WorldVisualiser.GetTransformPosFromMapPos(pos), Quaternion.identity) as GameObject;
        enemy.GetComponent<Creature>().Pos = pos;
        enemy.GetComponent<Creature>().Attack(Player.GetComponent<Player>());
        (CurrentMap as LocalMap).AddObject(enemy.GetComponent<Entity>());
    }

    public bool IsHexFree(LocalPos pos)
    {
        if (IsCurrentMapLocal())//Временно
            return (CurrentMap as LocalMap).Contains(pos) && !(CurrentMap as LocalMap).IsBlocked(pos);
        return true;
    }

    Chunk AllocAndGenerateChunk(Chunk topChunk, Chunk rightChunk, Chunk bottomChunk, Chunk leftChunk)
    {
        // TODO Ждём MonoDevelop 6.0 ( ?., ?[ ):
        WorldGenerator.HeighmapNeighboring hmNb = new WorldGenerator.HeighmapNeighboring();
        if (leftChunk != null)
        {
            hmNb.Left = new float[ChunkSize];
            for (ushort i = 0; i < ChunkSize; ++i)
                hmNb.Left[i] = leftChunk.HeightMatrix[i, ChunkSize - 1];
        }
        if (topChunk != null)
        {
            hmNb.Top = new float[ChunkSize];
            for (ushort i = 0; i < ChunkSize; ++i)
                hmNb.Top[i] = topChunk.HeightMatrix[0, i];
        }
        if (rightChunk != null)
        {
            hmNb.Right = new float[ChunkSize];
            for (ushort i = 0; i < ChunkSize; ++i)
                hmNb.Right[i] = rightChunk.HeightMatrix[i, 0];
        }
        if (bottomChunk != null)
        {
            hmNb.Bottom = new float[ChunkSize];
            for (ushort i = 0; i < ChunkSize; ++i)
                hmNb.Bottom[i] = bottomChunk.HeightMatrix[ChunkSize - 1, i];
        }

        float?[,] buf = new float?[ChunkSize, ChunkSize];
        WorldGenerator.CreateHeightmap(ref buf, LandscapeRoughness, hmNb);
        Chunk chunk = new Chunk(ChunkSize, ChunkSize);
        for (ushort y = 0; y < ChunkSize; ++y)
            for (ushort x = 0; x < ChunkSize; ++x)
            {
                chunk.HeightMatrix[y, x] = Mathf.Clamp(buf[y, x].Value, 0, Mathf.Abs(buf[y, x].Value));
                buf[y, x] = null;
            }

        if (leftChunk != null)
            for (ushort i = 0; i < ChunkSize; ++i)
                hmNb.Left[i] = leftChunk.ForestMatrix[i, ChunkSize - 1];
        if (topChunk != null)
            for (ushort i = 0; i < ChunkSize; ++i)
                hmNb.Top[i] = topChunk.ForestMatrix[0, i];
        if (rightChunk != null)
            for (ushort i = 0; i < ChunkSize; ++i)
                hmNb.Right[i] = rightChunk.ForestMatrix[i, 0];
        if (bottomChunk != null)
            for (ushort i = 0; i < ChunkSize; ++i)
                hmNb.Bottom[i] = bottomChunk.ForestMatrix[ChunkSize - 1, i];

        WorldGenerator.CreateHeightmap(ref buf, ForestRoughness, hmNb);
        for (ushort y = 0; y < ChunkSize; ++y)
            for (ushort x = 0; x < ChunkSize; ++x)
            {
                buf[y, x] = buf[y, x].Value * ForestDensity;
                chunk.ForestMatrix[y, x] = Mathf.Clamp(buf[y, x].Value, 0, Mathf.Abs(buf[y, x].Value));
            }
        chunk.Rivers = WorldGenerator.CreateRivers(chunk.HeightMatrix, chunk.RiverMatrix, RiverParam);
        chunk.Clusters = WorldGenerator.CreateClusters(chunk, ClusterParam);
        chunk.Roads = WorldGenerator.CreateRoads(chunk.HeightMatrix, chunk.RiverMatrix, chunk.RoadMatrix, chunk.Clusters, RoadParam);
        CalculateChunkTerrains(chunk);
        return chunk;
    }

    void CalculateChunkTerrains(Chunk chunk)
    {
        for (ushort y = 0; y < ChunkSize; ++y)
            for (ushort x = 0; x < ChunkSize; ++x)
                chunk.TerrainMatrix[y, x] = MakeTerrainFromHeight(chunk.HeightMatrix[y, x]) | (chunk.ForestMatrix[y, x] > TreeCountForForestTerrain ? TerrainType.FOREST : TerrainType.NONE) | (chunk.RiverMatrix[y, x] ? TerrainType.RIVER : TerrainType.NONE);
    }

    TerrainType MakeTerrainFromHeight(float height)//C#6.0 EBD
    {
        return TerrainType.MEADOW; //UNDONE
    }

    //TEST
    public void EnemyAttack()//C#6.0 EBD
    {
        SwitchMap();
    }

    public TerrainType GetHexTerrain(GlobalPos pos)//C#6.0 EBD
    {
        return (CurrentMap as Chunk).TerrainMatrix[pos.Y - ChunkY * ChunkSize, pos.X - ChunkX * ChunkSize];
    }
}
