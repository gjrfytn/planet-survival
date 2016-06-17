using UnityEngine;
using System.IO;

public class World : MonoBehaviour
{
    public Map CurrentMap { get; private set; } //TODO Возможно, можно будет убрать. Карта, на которой находится игрок.

    [SerializeField]
    float LandscapeRoughness;
    [SerializeField]
    float ForestRoughness;

    [SerializeField]
    WorldGenerator.GlobalTerrainSettings GlobalMapTerrainParam;
    [SerializeField]
    WorldGenerator.LocalTerrainSettings LocalMapTerrainParam;

    [SerializeField]
    ushort ChunkSize;
    [SerializeField]
    LocalPos LocalMapSize_;
    public LocalPos LocalMapSize { get { return LocalMapSize_; } private set { LocalMapSize_ = value; } }

    [SerializeField]
    byte ForestDensity;
    [SerializeField]
    byte TreeCountForForestTerrain;

    [SerializeField]
    LivingBeing[] Enemies;

    Chunk[,] CashedChunks = new Chunk[3, 3];
    LocalMap[,] LocalMaps;

    WorldVisualiser Visualiser; //Временно

    GlobalPos GlobalMapPos;
    Player Player;
    int ChunkX, ChunkY;
    const string ChunksDirectoryName = "chunks";
    string ChunksDirectoryPath;

    [SerializeField]
    UnityEngine.UI.Button SkipTurnBtn;

    void OnEnable()
    {
        EventManager.PlayerMovedOnGlobal += OnPlayerGotoGlobalHex;
        EventManager.TurnMade += RerenderBlueHexesOnLocal;
    }

    void OnDisable()
    {
        EventManager.PlayerMovedOnGlobal -= OnPlayerGotoGlobalHex;
        EventManager.TurnMade -= RerenderBlueHexesOnLocal;
    }

    void Awake()
    {
        //Assert
        for (byte i = 1; i < GlobalMapTerrainParam.Terrains.Length; ++i)
            Debug.Assert(GlobalMapTerrainParam.Terrains[i - 1].StartingHeight < GlobalMapTerrainParam.Terrains[i].StartingHeight);
        for (byte i = 1; i < LocalMapTerrainParam.Terrains.Length; ++i)
            Debug.Assert(LocalMapTerrainParam.Terrains[i - 1].StartingHeight < LocalMapTerrainParam.Terrains[i].StartingHeight);
        //--
        for (byte i = 0; i < GlobalMapTerrainParam.Terrains.Length; ++i)
        {
            if (GlobalMapTerrainParam.Terrains[i].TerrainType == TerrainType.NONE)
                throw new System.Exception("You should not initialize TerrainType.NONE.");
            for (byte j = 0; j < GlobalMapTerrainParam.Terrains.Length; ++j)
                if (GlobalMapTerrainParam.Terrains[i].TerrainType == GlobalMapTerrainParam.Terrains[j].TerrainType && i != j)
                    throw new System.Exception("Duplicatated terrain types in GlobalMapTerrainParam.Terrains.");
        }
        for (byte i = 0; i < LocalMapTerrainParam.Terrains.Length; ++i)
        {
            if (LocalMapTerrainParam.Terrains[i].TerrainType == TerrainType.NONE)
                throw new System.Exception("You should not initialize TerrainType.NONE.");
            for (byte j = 0; j < LocalMapTerrainParam.Terrains.Length; ++j)
                if (LocalMapTerrainParam.Terrains[i].TerrainType == LocalMapTerrainParam.Terrains[j].TerrainType && i != j)
                    throw new System.Exception("Duplicatated terrain types in LocalMapTerrainParam.Terrains.");
        }
    }

    void Start()
    {
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

        Player = GameObject.FindWithTag("Player").GetComponent<Player>();
        Camera.main.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y + Camera.main.transform.position.z * (Mathf.Tan((360 - Camera.main.transform.rotation.eulerAngles.x) / 57.3f)), Camera.main.transform.position.z);
        Visualiser.RenderVisibleHexes(Player.GlobalPos, Player.ViewDistance, CashedChunks, ChunkY, ChunkX);
        for (byte i = 0; i < 6; ++i)
            Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(Player.GlobalPos, (HexDirection)i));
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
    void OnPlayerGotoGlobalHex(GlobalPos pos)
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

        Visualiser.RenderVisibleHexes(pos, Player.ViewDistance, CashedChunks, ChunkY, ChunkX);
        Visualiser.DestroyAllBlues();//TODO Временно
        for (byte i = 0; i < 6; ++i)
            Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(pos, (HexDirection)i));
    }

    /// <summary>
    /// Переключение карт.
    /// </summary>
    public void SwitchMap()
    {
        Visualiser.DestroyAllHexes(); //TODO Это лучше делать до генерации карты, чтобы не было видно подвисания (или нужно отображение загрузки).
        if (!IsCurrentMapLocal())
        {
            SkipTurnBtn.gameObject.SetActive(true);
            GotoLocalMap();
            SpawnRandomEnemy();
            Player.transform.position = WorldVisualiser.GetTransformPosFromMapPos(Player.Pos);
            Camera.main.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, Camera.main.transform.position.z);
        }
        else
        {
            SkipTurnBtn.gameObject.SetActive(false);
            GotoGlobalMap();
            //EventManager.OnLocalMapLeave();
            Player.transform.position = WorldVisualiser.GetTransformPosFromMapPos(Player.GlobalPos);
        }
        EventManager.OnPlayerObjectMoved();
    }

    /// <summary>
    /// Переход на локальную карту.
    /// </summary>
    void GotoLocalMap()
    {
        GlobalMapPos = Player.GlobalPos;
        LocalPos pos = new LocalPos((ushort)(GlobalMapPos.X - ChunkX * ChunkSize), (ushort)(GlobalMapPos.Y - ChunkY * ChunkSize)); //TODO new?
        if (LocalMaps[pos.Y, pos.X] == null)
            LocalMaps[pos.Y, pos.X] = CreateLocalMap(CashedChunks[1, 1].HeightMatrix[pos.Y, pos.X], CashedChunks[1, 1].ForestMatrix[pos.Y, pos.X], (CashedChunks[1, 1].TerrainMatrix[pos.Y, pos.X] & TerrainType.RIVER) != TerrainType.NONE);
        CurrentMap = LocalMaps[pos.Y, pos.X];
        (CurrentMap as LocalMap).Activate();

        //TEMP
        Player.GlobalPos.X = (LocalMapSize.X >> 1);
        Player.GlobalPos.Y = (LocalMapSize.Y >> 1);
        //
        EventManager.OnEntitySpawn(Player);

        Visualiser.RenderWholeMap(CurrentMap as LocalMap);
        RerenderBlueHexesOnLocal();
    }

    /// <summary>
    /// Переход на глобальную карту.
    /// </summary>
    void GotoGlobalMap()
    {
        EventManager.OnEntityDestroy(Player);
        (CurrentMap as LocalMap).Deactivate();
        CurrentMap = CashedChunks[1, 1];
        Player.GlobalPos = GlobalMapPos;
        Visualiser.RenderVisibleHexes(Player.GlobalPos, Player.ViewDistance, CashedChunks, ChunkY, ChunkX);

        Visualiser.DestroyAllBlues();
        for (byte i = 0; i < 6; ++i)
            Visualiser.HighlightHex(HexNavigHelper.GetNeighborMapCoords(Player.GlobalPos, (HexDirection)i));
    }

    /// <summary>
    /// Создаёт локальную карту.
    /// </summary>
    /// <param name="coords">Координаты новой карты на глобальной.</param>
    LocalMap CreateLocalMap(float height, float forest, bool river)
    {
        float?[,] buf = new float?[LocalMapSize.Y, LocalMapSize.X];
        if (river)
        {
            //LocalPos[] riverPath = new LocalPos[] { new LocalPos(0, (ushort)(LocalMapSize.Y >> 1)), new LocalPos((ushort)(LocalMapSize.X - 1), (ushort)(LocalMapSize.Y >> 1)) }; //UNDONE			
            //WorldGenerator.MakeEqualHeightLine(map.HeightMatrix, riverPath, Visualiser.LocalMapParam.Terrains[0].StartingHeight - 0.0001f); TODO
        }

        WorldGenerator.HeighmapNeighboring hmNb;
        hmNb.Left = new float[LocalMapSize.Y];
        hmNb.Top = new float[LocalMapSize.X];
        hmNb.Right = new float[LocalMapSize.Y];
        hmNb.Bottom = new float[LocalMapSize.X];
        for (ushort i = 0; i < LocalMapSize.Y; ++i)
            hmNb.Right[i] = hmNb.Left[i] = height;
        for (ushort i = 0; i < LocalMapSize.X; ++i)
            hmNb.Bottom[i] = hmNb.Top[i] = height;
        WorldGenerator.CreateHeightmap(ref buf, LandscapeRoughness, hmNb);

        LocalMap map = new LocalMap(LocalMapSize.X, LocalMapSize.Y);
        float[,] buf2 = new float[LocalMapSize.Y, LocalMapSize.X];
        for (ushort y = 0; y < LocalMapSize.Y; ++y)
            for (ushort x = 0; x < LocalMapSize.X; ++x)
            {
                buf2[y, x] = buf[y, x].Value;
                buf[y, x] = null;
            }
        map.TerrainMatrix = WorldGenerator.CreateTerrainmap(buf2, LocalMapTerrainParam);
        WorldGenerator.CreateVegetation(ref map, LocalMapTerrainParam, forest);

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
            using (SymBinaryReader reader = new SymBinaryReader(File.Open(filePath, FileMode.Open)))
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
            using (SymBinaryReader reader = new SymBinaryReader(File.Open(filePath, FileMode.Open)))
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
        Creature enemy = (MonoBehaviour.Instantiate(Enemies[Random.Range(0, Enemies.Length)]) as Creature);
        enemy.Pos = pos;
        EventManager.OnEntitySpawn(enemy);
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
        chunk.Rivers = WorldGenerator.CreateRivers(chunk.HeightMatrix, ref chunk.TerrainMatrix, GlobalMapTerrainParam.RiversParam);
        chunk.Clusters = WorldGenerator.CreateClusters(ref chunk, GlobalMapTerrainParam.ClustersParam);
        chunk.Roads = WorldGenerator.CreateRoads(chunk.HeightMatrix, ref chunk.TerrainMatrix, chunk.Clusters, GlobalMapTerrainParam.RoadsParam);
        chunk.TerrainMatrix = WorldGenerator.CreateTerrainmap(chunk.HeightMatrix, GlobalMapTerrainParam);
        return chunk;
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

    public void RerenderBlueHexesOnLocal()//C#6.0 EBD
    {
        Visualiser.RenderBluesHexes(Player.Pos, Player.RemainingMoves, CurrentMap as LocalMap);
    }
}
