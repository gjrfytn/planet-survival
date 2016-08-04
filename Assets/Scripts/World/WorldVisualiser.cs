using UnityEngine;
using System.Collections.Generic;


public class WorldVisualiser : MonoBehaviour
{
    [System.Serializable]
    public class Terrain
    {
        [SerializeField]
        TerrainType TerrainType_;
        public TerrainType TerrainType { get { return TerrainType_; } set { TerrainType_ = value; } }
        [SerializeField]
        Sprite[] Sprites_;
        public Sprite[] Sprites { get { return Sprites_; } set { Sprites_ = value; } }
    }

    [System.Serializable]
    public class GlobalMapSettings
    {
        [SerializeField]
        Terrain[] Terrains_;
        public Terrain[] Terrains { get { return Terrains_; } }
        [SerializeField]
        Sprite[] RiverStartSprites_;
        public Sprite[] RiverStartSprites { get { return RiverStartSprites_; } }
        [SerializeField]
        Sprite[] RiverStraightSprites_;
        public Sprite[] RiverStraightSprites { get { return RiverStraightSprites_; } }
        [SerializeField]
        Sprite[] RiverTurnSprites_;
        public Sprite[] RiverTurnSprites { get { return RiverTurnSprites_; } }
        [SerializeField]
        Sprite[] RiverEndSprites_;
        public Sprite[] RiverEndSprites { get { return RiverEndSprites_; } }
        [SerializeField]
        Sprite[] TreeSprites_;
        public Sprite[] TreeSprites { get { return TreeSprites_; } }
        [SerializeField]
        Sprite[] RuinSprites_;
        public Sprite[] RuinSprites { get { return RuinSprites_; } }
        [SerializeField]
        Sprite[] RoadStraightSprites_;
        public Sprite[] RoadStraightSprites { get { return RoadStraightSprites_; } }
        [SerializeField]
        Sprite[] RoadTurnSprites_;
        public Sprite[] RoadTurnSprites { get { return RoadTurnSprites_; } }
        [SerializeField]
        Sprite[] RoadStraightBridgeSprites_;
        public Sprite[] RoadStraightBridgeSprites { get { return RoadStraightBridgeSprites_; } }
        [SerializeField]
        Sprite[] RoadTurnBridgeSprites_;
        public Sprite[] RoadTurnBridgeSprites { get { return RoadTurnBridgeSprites_; } }
    }

    [System.Serializable]
    public class LocalMapSettings
    {
        [SerializeField]
        Terrain[] Terrains_;
        public Terrain[] Terrains { get { return Terrains_; } }
        [SerializeField]
        Sprite[] BankSprites_;
        public Sprite[] BankSprites { get { return BankSprites_; } }
        [SerializeField]
        Sprite[] DiagBankSprites_;
        public Sprite[] DiagBankSprites { get { return DiagBankSprites_; } }
    }

    public static Vector2 GlobalHexSpriteSize { get; private set; } //TODO static? Перенести в классы?
    public static Vector2 LocalHexSpriteSize { get; private set; }

    [SerializeField]
    GameObject InteractableHex;
    [SerializeField]
    GlobalMapSettings GlobalMapParam;
    [SerializeField]
    LocalMapSettings LocalMapParam_;
    public LocalMapSettings LocalMapParam { get { return LocalMapParam_; } }
    [SerializeField]
    Material DiffuseMaterial;
    [SerializeField]
    byte ForestGenGridSize;
    [SerializeField]
    float FadeInTime;
    [SerializeField]
    float FadeTime;

    List<Sprite> AllGlobalHexSprites = new List<Sprite>();
    List<Sprite> AllRiverSprites = new List<Sprite>();
    List<Sprite> AllRoadSprites = new List<Sprite>();

    List<Sprite> AllLocalHexSprites = new List<Sprite>();

    class ListType
    {
        public GameObject Hex;
        public GlobalPos Pos;
        public bool InSign;
    }

    List<ListType> RenderedHexes = new List<ListType>();
    Chunk[,] CashedChunks = new Chunk[3, 3];
    int ChunkX, ChunkY;


    class QueueType
    {
        public GlobalPos Pos;
        public byte Distance;
    }

    Queue<QueueType> SignQueue = new Queue<QueueType>();
    Queue<QueueType> BluesSignQueue = new Queue<QueueType>();

    class BluesType
    {
        public GameObject Hex;
        public bool InSign;
    }

    List<BluesType> RenderedBlues = new List<BluesType>();

    void Awake()
    {
        for (byte i = 0; i < GlobalMapParam.Terrains.Length; ++i)
        {
            if (GlobalMapParam.Terrains[i].TerrainType == TerrainType.NONE)
                throw new System.Exception("You should not initialize TerrainType.NONE.");
            for (byte j = 0; j < GlobalMapParam.Terrains.Length; ++j)
                if (GlobalMapParam.Terrains[i].TerrainType == GlobalMapParam.Terrains[j].TerrainType && i != j)
                    throw new System.Exception("Duplicatated terrain types in GlobalMapParam.Terrains.");
        }
        for (byte i = 0; i < LocalMapParam.Terrains.Length; ++i)
        {
            if (LocalMapParam.Terrains[i].TerrainType == TerrainType.NONE)
                throw new System.Exception("You should not initialize TerrainType.NONE.");
            for (byte j = 0; j < LocalMapParam.Terrains.Length; ++j)
                if (LocalMapParam.Terrains[i].TerrainType == LocalMapParam.Terrains[j].TerrainType && i != j)
                    throw new System.Exception("Duplicatated terrain types in LocalMapParam.Terrains.");
        }

        System.Array.ForEach(GlobalMapParam.Terrains, ts => AllGlobalHexSprites.AddRange(ts.Sprites));
        //foreach (Terrain ts in GlobalMapParam.Terrains)
        //AllGlobalHexSprites.AddRange(ts.Sprites);

        AllRiverSprites.AddRange(GlobalMapParam.RiverStartSprites);
        AllRiverSprites.AddRange(GlobalMapParam.RiverStraightSprites);
        AllRiverSprites.AddRange(GlobalMapParam.RiverTurnSprites);
        AllRiverSprites.AddRange(GlobalMapParam.RiverEndSprites);

        AllRoadSprites.AddRange(GlobalMapParam.RoadStraightSprites);
        AllRoadSprites.AddRange(GlobalMapParam.RoadTurnSprites);
        AllRoadSprites.AddRange(GlobalMapParam.RoadStraightBridgeSprites);
        AllRoadSprites.AddRange(GlobalMapParam.RoadTurnBridgeSprites);

        System.Array.ForEach(LocalMapParam.Terrains, ts => AllLocalHexSprites.AddRange(ts.Sprites));
        //foreach (Terrain ts in LocalMapParam.Terrains)
        //AllLocalHexSprites.AddRange(ts.Sprites);

        //Assert
        for (ushort i = 1; i < AllGlobalHexSprites.Count; ++i)
            Debug.Assert(AllGlobalHexSprites[i - 1].bounds.size == AllGlobalHexSprites[i].bounds.size);
        for (ushort i = 1; i < AllLocalHexSprites.Count; ++i)
            Debug.Assert(AllLocalHexSprites[i - 1].bounds.size == AllLocalHexSprites[i].bounds.size);
        //--

        GlobalHexSpriteSize = new Vector2(GlobalMapParam.Terrains[0].Sprites[0].bounds.size.x, GlobalMapParam.Terrains[0].Sprites[0].bounds.size.y);
        LocalHexSpriteSize = new Vector2(LocalMapParam.Terrains[0].Sprites[0].bounds.size.x, LocalMapParam.Terrains[0].Sprites[0].bounds.size.y);
    }

    /// <summary>
    /// Уничтожает все хексы.
    /// </summary>
    public void DestroyAllHexes()
    {
        foreach (ListType hex in RenderedHexes)
            Destroy(hex.Hex);
        RenderedHexes.Clear();
    }

    public void DestroyAllBlues()
    {
        RenderedBlues.ForEach(b => Destroy(b.Hex));
        RenderedBlues.Clear();
    }

    /// <summary>
    /// Отображает только хексы в поле зрения игрока.
    /// </summary>
    /// <param name="mapPosition">Координаты в матрице.</param>
    /// <param name="distance">Дальность обзора.</param>
    /// <param name="currentMap">Активная карта.</param>
    public void RenderVisibleHexes(GlobalPos pos, byte distance, Chunk[,] cashedChunks, int chunkY, int chunkX)
    {
        CashedChunks = cashedChunks;
        ChunkX = chunkX;
        ChunkY = chunkY;

        RenderedHexes.ForEach(hex => hex.InSign = false);

        SignQueue.Enqueue(new QueueType { Pos = pos, Distance = distance });
        while (SignQueue.Count != 0)
        {
            QueueType buf = SignQueue.Dequeue();
            SpreadRender(buf.Pos, buf.Distance);
        }

        for (ushort i = 0; i < RenderedHexes.Count; ++i)
            if (!RenderedHexes[i].InSign)
            {
                for (ushort j = 0; j < RenderedHexes[i].Hex.transform.childCount; ++j)
                    RenderedHexes[i].Hex.transform.GetChild(j).gameObject.GetComponent<Fader>().FadeAndDestroyObject(FadeTime);
                RenderedHexes[i].Hex.GetComponent<Fader>().FadeAndDestroyObject(FadeTime);
                RenderedHexes.RemoveAt(i);
                --i;
            }
    }

    /// <summary>
    /// Рекурсивно заносит в очередь на отображение хексы.
    /// </summary>
    /// <param name="mapPosition">Координаты в матрице.</param>
    /// <param name="distance">Оставшееся расстояние для распространения.</param>
    void SpreadRender(GlobalPos pos, byte distance)
    {
        Chunk map;
        ushort chunkSize = CashedChunks[1, 1].Width;

        float chunkX = (float)pos.X / chunkSize, chunkY = (float)pos.Y / chunkSize;

        chunkX = Mathf.Floor(chunkX);
        chunkY = Mathf.Floor(chunkY);

        map = CashedChunks[(int)(chunkY - ChunkY + 1), (int)(chunkX - ChunkX + 1)];

        LocalPos inchunkPos;
        inchunkPos.X = (ushort)(pos.X - chunkSize * chunkX);
        inchunkPos.Y = (ushort)(pos.Y - chunkSize * chunkY);

        short index = (short)RenderedHexes.FindIndex(x => x.Pos == pos);
        if (index == -1)
        {
            ListType hex = new ListType { Hex = new GameObject("hex"), InSign = true };
            hex.Hex.transform.position = GetTransformPosFromMapPos(pos);
            hex.Hex.transform.parent = transform;
            hex.Pos = pos;
            hex.Hex.AddComponent<SpriteRenderer>();
            hex.Hex.AddComponent<Fader>();

            MakeHexGraphics(hex, inchunkPos, map);
            RenderedHexes.Add(hex);
        }
        else
        {
            if (RenderedHexes[index].InSign)
                return;
            else
                RenderedHexes[index].InSign = true;
        }

        if (distance != 0)
        {
            SignQueue.Enqueue(new QueueType { Pos = HexNavigHelper.GetNeighborMapCoords(pos, HexDirection.TOP_LEFT), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { Pos = HexNavigHelper.GetNeighborMapCoords(pos, HexDirection.TOP), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { Pos = HexNavigHelper.GetNeighborMapCoords(pos, HexDirection.TOP_RIGHT), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { Pos = HexNavigHelper.GetNeighborMapCoords(pos, HexDirection.BOTTOM_RIGHT), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { Pos = HexNavigHelper.GetNeighborMapCoords(pos, HexDirection.BOTTOM), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { Pos = HexNavigHelper.GetNeighborMapCoords(pos, HexDirection.BOTTOM_LEFT), Distance = (byte)(distance - 1) });
        }
    }

    /// <summary>
    /// Создаёт спрайты, необходимые для отображения хекса.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    void MakeHexGraphics(ListType hex, LocalPos pos, Chunk map)
    {
        if (map.HexSpriteID_Matrix[pos.Y, pos.X].HasValue /*?*/&& map.HexSpriteID_Matrix[pos.Y, pos.X] < AllGlobalHexSprites.Count)
            hex.Hex.GetComponent<SpriteRenderer>().sprite = AllGlobalHexSprites[map.HexSpriteID_Matrix[pos.Y, pos.X].Value];
        else
            hex.Hex.GetComponent<SpriteRenderer>().sprite = ChooseHexSprite(pos, map);
        hex.Hex.GetComponent<SpriteRenderer>().sortingLayerName = "Landscape";//
        bool forestBlocked = false;
        if ((map.TerrainMatrix[pos.Y, pos.X] & TerrainType.RIVER) != TerrainType.NONE)
        {
            GameObject riverSprite = new GameObject("riverSprite");
            riverSprite.transform.position = hex.Hex.transform.position;
            riverSprite.transform.parent = hex.Hex.transform;
            riverSprite.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";
            riverSprite.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
            if (map.RiverSpriteID_Matrix[pos.Y, pos.X].HasValue && map.RiverSpriteID_Matrix[pos.Y, pos.X] < AllRiverSprites.Count)
            {
                riverSprite.GetComponent<SpriteRenderer>().sprite = AllRiverSprites[map.RiverSpriteID_Matrix[pos.Y, pos.X].Value];
                riverSprite.transform.Rotate(0, 0, map.RiverSpriteRotationMatrix[pos.Y, pos.X]);
            }
            else
                riverSprite.GetComponent<SpriteRenderer>().sprite = ChooseHexRiverSprite(riverSprite.transform, pos, map);
            riverSprite.AddComponent<Fader>();
            forestBlocked = true;
        }
        if ((map.TerrainMatrix[pos.Y, pos.X] & TerrainType.BUILDING) != TerrainType.NONE)
        {
            GameObject clusterSprite = new GameObject("clusterSprite");
            clusterSprite.transform.position = hex.Hex.transform.position;
            clusterSprite.transform.parent = hex.Hex.transform;
            clusterSprite.AddComponent<SpriteRenderer>().sortingLayerName = "Infrastructure";
            clusterSprite.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
            if (!map.ClusterSpriteID_Matrix[pos.Y, pos.X].HasValue || map.ClusterSpriteID_Matrix[pos.Y, pos.X] >= GlobalMapParam.RuinSprites.Length)
                map.ClusterSpriteID_Matrix[pos.Y, pos.X] = (byte)Random.Range(0, GlobalMapParam.RuinSprites.Length);
            clusterSprite.GetComponent<SpriteRenderer>().sprite = GlobalMapParam.RuinSprites[map.ClusterSpriteID_Matrix[pos.Y, pos.X].Value];
            clusterSprite.AddComponent<Fader>();
            forestBlocked = true;
        }
        if ((map.TerrainMatrix[pos.Y, pos.X] & TerrainType.ROAD) != TerrainType.NONE)
        {
            GameObject roadSprite = new GameObject("roadSprite");
            roadSprite.transform.position = hex.Hex.transform.position;
            roadSprite.transform.parent = hex.Hex.transform;
            roadSprite.AddComponent<SpriteRenderer>().sortingLayerName = "Infrastructure";
            roadSprite.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
            if (map.RoadSpriteID_Matrix[pos.Y, pos.X].HasValue /*?*/ && map.RoadSpriteID_Matrix[pos.Y, pos.X] < AllRiverSprites.Count)
            {
                roadSprite.GetComponent<SpriteRenderer>().sprite = AllRoadSprites[map.RoadSpriteID_Matrix[pos.Y, pos.X].Value];
                roadSprite.transform.Rotate(0, 0, map.RoadSpriteRotationMatrix[pos.Y, pos.X]);
            }
            else
                roadSprite.GetComponent<SpriteRenderer>().sprite = ChooseHexRoadSprite(roadSprite.transform, pos, map);
            roadSprite.AddComponent<Fader>();
            forestBlocked = true;
        }
        if (!forestBlocked)
            MakeHexForest(hex, pos, map);
        hex.Hex.GetComponent<Fader>().FadeIn(FadeInTime);
        for (ushort j = 0; j < hex.Hex.transform.childCount; ++j)
            hex.Hex.transform.GetChild(j).gameObject.GetComponent<Fader>().FadeIn(FadeInTime);
    }

    void MakeHexGraphics(ListType hex, LocalPos pos, LocalMap map)
    {
        if (map.HexSpriteID_Matrix[pos.Y, pos.X].HasValue /*?*/&& map.HexSpriteID_Matrix[pos.Y, pos.X] < AllLocalHexSprites.Count)
            hex.Hex.GetComponent<SpriteRenderer>().sprite = AllLocalHexSprites[map.HexSpriteID_Matrix[pos.Y, pos.X].Value];
        else
            hex.Hex.GetComponent<SpriteRenderer>().sprite = ChooseHexSprite(pos, map);
        hex.Hex.GetComponent<SpriteRenderer>().sortingLayerName = "Landscape";//
        if ((map.TerrainMatrix[pos.Y, pos.X] & TerrainType.WATER) != TerrainType.NONE)
            //float offsetbuf=(LocalHexSpriteSize.pos.X-LocalHexSpriteSize.y)/2;
            for (byte i = 0; i < 6; ++i) //TODO К оптимизации.
                if (map.Contains(HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i)) && (map.TerrainMatrix[HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i).Y, HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i).X] & TerrainType.WATER) == TerrainType.NONE)
                {
                    GameObject bank = new GameObject("bankSprite");
                    bank.transform.position = hex.Hex.transform.position;
                    bank.transform.parent = hex.Hex.transform;
                    bank.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";
                    if ((TurnedHexDirection)i == TurnedHexDirection.LEFT || (TurnedHexDirection)i == TurnedHexDirection.RIGHT)
                    {
                        bank.GetComponent<SpriteRenderer>().sprite = LocalMapParam.BankSprites[Random.Range(0, LocalMapParam.BankSprites.Length)];
                        if ((TurnedHexDirection)i == TurnedHexDirection.RIGHT)
                            bank.transform.Rotate(0, 0, 180);
                    }
                    else
                    {
                        bank.GetComponent<SpriteRenderer>().sprite = LocalMapParam.DiagBankSprites[Random.Range(0, LocalMapParam.DiagBankSprites.Length)];
                        switch ((TurnedHexDirection)i)
                        {
                            case TurnedHexDirection.RIGHT_TOP: bank.transform.Rotate(0, 180, 0);
                                break;
                            case TurnedHexDirection.RIGHT_BOTTOM: bank.transform.Rotate(180, 180, 0);
                                break;
                            case TurnedHexDirection.LEFT_BOTTOM: bank.transform.Rotate(180, 0, 0);
                                break;
                        }

                    }
                    //float offset=((TurnedHexDirection)i==TurnedHexDirection.LEFT_TOP||(TurnedHexDirection)i==TurnedHexDirection.LEFT_BOTTOM)? -offsetbuf:offsetbuf;
                    //bank.transform.position = new Vector3(hex.Hex.transform.position.pos.X+offset,hex.Hex.transform.position.y,hex.Hex.transform.position.z);
                    //bank.transform.Rotate(0, 0, (short)(Mathf.Sign(mapCoords.y - HexNavigHelper.GetNeighborMapCoords(mapCoords, (TurnedHexDirection)i).y) * Vector2.Angle(Vector2.left,new Vector2(GetTransformPosFromMapPos(HexNavigHelper.GetNeighborMapCoords(mapCoords, (TurnedHexDirection)i), true).pos.X-offset,GetTransformPosFromMapPos(HexNavigHelper.GetNeighborMapCoords(mapCoords, (TurnedHexDirection)i), true).y) - (Vector2)bank.transform.position)));
                    bank.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
                    bank.AddComponent<Fader>();
                }

        hex.Hex.GetComponent<Fader>().FadeIn(FadeInTime);
        for (ushort j = 0; j < hex.Hex.transform.childCount; ++j)
            hex.Hex.transform.GetChild(j).gameObject.GetComponent<Fader>().FadeIn(FadeInTime);
    }

    /// <summary>
    /// Выбирает спрайт хекса.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    Sprite ChooseHexSprite(LocalPos pos, Chunk map) //UNDONE
    {
        byte id = 0;
        for (byte i = 0; i < GlobalMapParam.Terrains.Length; ++i)
            if ((map.TerrainMatrix[pos.Y, pos.X] & GlobalMapParam.Terrains[i].TerrainType) != TerrainType.NONE)
            {
                for (byte j = 0; j < i; ++j, id += (byte)GlobalMapParam.Terrains[j].Sprites.Length) ;
                id += (byte)Random.Range(0, GlobalMapParam.Terrains[i].Sprites.Length);
            }

        map.HexSpriteID_Matrix[pos.Y, pos.X] = id;
        return AllGlobalHexSprites[id];
    }

    Sprite ChooseHexSprite(LocalPos pos, LocalMap map) //UNDONE
    {
        byte id = 0;
        for (byte i = 0; i < LocalMapParam.Terrains.Length; ++i)
            if ((map.TerrainMatrix[pos.Y, pos.X] & LocalMapParam.Terrains[i].TerrainType) != TerrainType.NONE)
            {
                for (byte j = 0; j < i; ++j, id += (byte)LocalMapParam.Terrains[j].Sprites.Length) ;
                id += (byte)Random.Range(0, LocalMapParam.Terrains[i].Sprites.Length);
            }

        map.HexSpriteID_Matrix[pos.Y, pos.X] = id;
        return AllLocalHexSprites[id];
    }

    Sprite ChooseHexRiverSprite(Transform spriteTransform, LocalPos pos, Chunk map)//TODO К оптимизации.
    {
        foreach (List<LocalPos> river in map.Rivers)
        {
            byte id;
            short angle;
            short index = (short)river.IndexOf(pos);
            if (index != -1)
            {
                Vector2 inchunkPos = GetTransformPosFromMapPos((GlobalPos)pos);
                if (index == 0)
                {
                    id = (byte)Random.Range(0, GlobalMapParam.RiverStartSprites.Length);
                    angle = (short)(Mathf.Sign(river[1].X - pos.X) * Vector2.Angle(Vector2.down, GetTransformPosFromMapPos((GlobalPos)river[1]) - inchunkPos));
                }
                else if (index == river.Count - 1)
                {
                    id = (byte)(Random.Range(0, GlobalMapParam.RiverEndSprites.Length) + GlobalMapParam.RiverStartSprites.Length + GlobalMapParam.RiverStraightSprites.Length + GlobalMapParam.RiverTurnSprites.Length);
                    angle = (short)(Mathf.Sign(river[river.Count - 2].X - pos.X) * Vector2.Angle(Vector2.down, GetTransformPosFromMapPos((GlobalPos)river[river.Count - 2]) - inchunkPos));
                }
                else
                {
                    Vector2 prev = GetTransformPosFromMapPos((GlobalPos)river[index - 1]) - inchunkPos;
                    Vector2 next = GetTransformPosFromMapPos((GlobalPos)river[index + 1]) - inchunkPos;
                    if ((short)Vector2.Angle(prev, next) > 150) //(==180, !=120)
                    {
                        id = (byte)(Random.Range(0, GlobalMapParam.RiverStraightSprites.Length) + GlobalMapParam.RiverStartSprites.Length);
                        angle = (short)(Mathf.Sign(river[index - 1].X - pos.X) * Vector2.Angle(Vector2.down, prev));
                    }
                    else
                    {
                        id = (byte)(Random.Range(0, GlobalMapParam.RiverTurnSprites.Length) + GlobalMapParam.RiverStartSprites.Length + GlobalMapParam.RiverStraightSprites.Length);
                        angle = (short)(Mathf.Sign(river[index - 1].X - pos.X) * Vector2.Angle(Vector2.down, prev));
                        if (Mathf.Approximately(prev.x, 0))
                            angle += (short)(240 * (Mathf.Sign(prev.y) == Mathf.Sign(next.x) ? 1 : 0));
                        else
                            angle += (short)(240 * (Mathf.Sign(prev.x) != Mathf.Sign(next.y) ? 1 : 0));
                    }
                }
            }
            else
                continue;

            map.RiverSpriteID_Matrix[pos.Y, pos.X] = id;
            map.RiverSpriteRotationMatrix[pos.Y, pos.X] = angle;
            spriteTransform.Rotate(0, 0, angle);

            return AllRiverSprites[id]; ;
        }
        return null;
    }

    Sprite ChooseHexRoadSprite(Transform spriteTransform, LocalPos pos, Chunk map)//TODO К оптимизации.
    {
        foreach (List<LocalPos> road in map.Roads)
        {
            byte id;
            short angle;
            short index = (short)road.IndexOf(pos);
            if (index != -1)
            {
                Vector2 inchunkPos = GetTransformPosFromMapPos((GlobalPos)pos);
                if (index == 0)
                {
                    id = (byte)((map.TerrainMatrix[pos.Y, pos.X] & TerrainType.RIVER) != TerrainType.NONE ? Random.Range(0, GlobalMapParam.RoadStraightBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length : Random.Range(0, GlobalMapParam.RoadStraightSprites.Length));
                    angle = (short)(Mathf.Sign(road[1].X - pos.X) * Vector2.Angle(Vector2.down, GetTransformPosFromMapPos((GlobalPos)road[1]) - inchunkPos));
                }
                else if (index == road.Count - 1)
                {
                    id = (byte)((map.TerrainMatrix[pos.Y, pos.X] & TerrainType.RIVER) != TerrainType.NONE ? Random.Range(0, GlobalMapParam.RoadStraightBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length : Random.Range(0, GlobalMapParam.RoadStraightSprites.Length));
                    angle = (short)(Mathf.Sign(road[road.Count - 2].X - pos.X) * Vector2.Angle(Vector2.down, GetTransformPosFromMapPos((GlobalPos)road[road.Count - 2]) - inchunkPos));
                }
                else
                {
                    Vector2 prev = GetTransformPosFromMapPos((GlobalPos)road[index - 1]) - inchunkPos;
                    Vector2 next = GetTransformPosFromMapPos((GlobalPos)road[index + 1]) - inchunkPos;
                    if ((short)Vector2.Angle(prev, next) > 150) //(==180, !=120)
                    {
                        id = (byte)((map.TerrainMatrix[pos.Y, pos.X] & TerrainType.RIVER) != TerrainType.NONE ? Random.Range(0, GlobalMapParam.RoadStraightBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length : Random.Range(0, GlobalMapParam.RoadStraightSprites.Length));
                        angle = (short)(Mathf.Sign(road[index - 1].X - pos.X) * Vector2.Angle(Vector2.down, prev));
                    }
                    else
                    {
                        id = (byte)((map.TerrainMatrix[pos.Y, pos.X] & TerrainType.RIVER) != TerrainType.NONE ? Random.Range(0, GlobalMapParam.RoadTurnBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length + GlobalMapParam.RoadStraightBridgeSprites.Length : Random.Range(0, GlobalMapParam.RoadTurnSprites.Length) + GlobalMapParam.RoadStraightSprites.Length);
                        angle = (short)(Mathf.Sign(road[index - 1].X - pos.X) * Vector2.Angle(Vector2.down, prev));
                        if (Mathf.Approximately(prev.x, 0))
                            angle += (short)(240 * (Mathf.Sign(prev.y) == Mathf.Sign(next.x) ? 1 : 0));
                        else
                            angle += (short)(240 * (Mathf.Sign(prev.x) != Mathf.Sign(next.y) ? 1 : 0));
                    }
                }
            }
            else
                continue;

            map.RoadSpriteID_Matrix[pos.Y, pos.X] = id;
            map.RoadSpriteRotationMatrix[pos.Y, pos.X] = angle;
            spriteTransform.Rotate(0, 0, angle);

            return AllRoadSprites[id]; ;
        }
        return null;
    }

    /// <summary>
    /// Создаёт лес на хексе.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    void MakeHexForest(ListType hex, LocalPos pos, Chunk map)
    {
        Vector2 spriteSize = hex.Hex.GetComponent<SpriteRenderer>().sprite.bounds.size;
        if (map.ForestMatrix[pos.Y, pos.X] >= 1) //TODO
        {
            float gridStepX = spriteSize.x / ForestGenGridSize;
            float gridStepY = spriteSize.y / ForestGenGridSize;
            Vector2 gridOrigin = new Vector2(hex.Hex.transform.position.x - spriteSize.x * 0.375f, hex.Hex.transform.position.y - spriteSize.y * 0.5f); //TODO new?
            byte treesCount = (byte)map.ForestMatrix[pos.Y, pos.X];

            while (true)
            {
                if (treesCount > ForestGenGridSize * ForestGenGridSize)
                {
                    for (float y = 0; y < spriteSize.y; y += gridStepY)
                        for (float x = 0; x < spriteSize.x; x += gridStepX)
                        {
                            Vector2 v = new Vector2(Random.value * gridStepX, Random.value * gridStepY); //TODO new?
                            GameObject tree = new GameObject("treeSprite");
                            tree.transform.position = new Vector2(gridOrigin.x + x + v.x, gridOrigin.y + y + v.y);
                            tree.transform.parent = hex.Hex.transform;
                            tree.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";//
                            tree.GetComponent<SpriteRenderer>().sprite = GlobalMapParam.TreeSprites[Random.Range(0, GlobalMapParam.TreeSprites.Length)];
                            tree.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
                            tree.AddComponent<Fader>();
                            --treesCount;
                        }
                }
                else
                {
                    Vector2 v = Random.insideUnitCircle;
                    v.x *= spriteSize.x * 0.5f;
                    v.y *= spriteSize.y * 0.5f;
                    GameObject tree = new GameObject("treeSprite");
                    tree.transform.position = new Vector2(hex.Hex.transform.position.x + v.x, hex.Hex.transform.position.y + v.y);
                    tree.transform.parent = hex.Hex.transform;
                    tree.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";//
                    tree.GetComponent<SpriteRenderer>().sprite = GlobalMapParam.TreeSprites[Random.Range(0, GlobalMapParam.TreeSprites.Length)];
                    tree.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
                    tree.AddComponent<Fader>();
                    --treesCount;
                    if (treesCount == 0)
                        return;
                }
            }
        }
    }

    /// <summary>
    /// Выводит хексы карты на сцену.
    /// </summary>
    /// <param name="map">Карта.</param>
    public void RenderWholeMap(LocalMap map)
    {
        ushort height = map.Height;
        ushort width = map.Width;
        RenderedHexes.Capacity = height * width;

        for (ushort y = 0; y < height; ++y)
            for (ushort x = 0; x < width; ++x)
            {
                // TODO Возможно стоит заменить ListType на Hex?
                ListType hex = new ListType { Hex = new GameObject("hex"), InSign = true };
                hex.Hex.transform.position = GetTransformPosFromMapPos(new LocalPos(x, y));
                hex.Hex.transform.parent = transform;
                hex.Pos = new GlobalPos(x, y);
                hex.Hex.AddComponent<SpriteRenderer>();
                hex.Hex.AddComponent<Fader>();

                MakeHexGraphics(hex, new LocalPos(x, y), map);
                RenderedHexes.Add(hex);
            }
    }

    /// <summary>
    /// Вычисляет координаты в сцене из координат на карте.
    /// </summary>
    /// <returns>Координаты в сцене.</returns>
    /// <param name="mapCoords">Координаты на карте.</param>
    public static Vector2 GetTransformPosFromMapPos(LocalPos pos)//C#6.0 EBD
    {
        return new Vector2(pos.X * LocalHexSpriteSize.x + (pos.Y & 1) * LocalHexSpriteSize.x * 0.5f, pos.Y * LocalHexSpriteSize.y * 0.75f);
    }

    public static Vector2 GetTransformPosFromMapPos(GlobalPos pos)//C#6.0 EBD
    {
        return new Vector2(pos.X * GlobalHexSpriteSize.x * 0.75f, pos.Y * GlobalHexSpriteSize.y + (pos.X & 1) * GlobalHexSpriteSize.y * 0.5f);
    }


    /// <summary>
    /// Накладывает на хекс спрайт.
    /// </summary>
    /// <param name="mapCoords">Координаты хекса.</param>
    /// <param name="highlightHexSprite">Спрайт.</param>
    public void HighlightHex(GlobalPos pos)
    {
        BluesType hex = new BluesType { Hex = Instantiate(InteractableHex, GetTransformPosFromMapPos(pos), Quaternion.identity) as GameObject, InSign = true };
        hex.Hex.GetComponent<HexInteraction>().Pos = pos;
        RenderedBlues.Add(hex);
    }

    void HighlightHex(LocalPos pos)
    {
        BluesType hex = new BluesType { Hex = Instantiate(InteractableHex, GetTransformPosFromMapPos(pos), Quaternion.Euler(0, 0, 90)) as GameObject, InSign = true };
        hex.Hex.GetComponent<HexInteraction>().Pos = pos;
        RenderedBlues.Add(hex);
    }

    public void RenderBluesHexes(LocalPos pos, byte distance, LocalMap map)
    {
        Debug.Assert(distance != 0);
        RenderedBlues.ForEach(hex => hex.InSign = false);

        for (byte i = 0; i < 6; ++i)
        {
            GlobalPos buf = HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i);
            if (buf.X >= 0 && buf.Y >= 0 && buf.X < map.Width && buf.Y < map.Height && !map.IsBlocked((LocalPos)buf))
                BluesSignQueue.Enqueue(new QueueType { Pos = buf, Distance = (byte)(distance - 1) });
        }

        while (BluesSignQueue.Count != 0)
        {
            QueueType buf = BluesSignQueue.Dequeue();
            SpreadBlues((LocalPos)buf.Pos, buf.Distance, map);
        }

        for (ushort i = 0; i < RenderedBlues.Count; ++i)
            if (!RenderedBlues[i].InSign)
            {
                Destroy(RenderedBlues[i].Hex);
                RenderedBlues.RemoveAt(i);
                --i;
            }
    }

    void SpreadBlues(LocalPos pos, byte distance, LocalMap map)
    {
        short index = (short)RenderedBlues.FindIndex(x => x.Hex.GetComponent<HexInteraction>().Pos == pos);
        if (index == -1)
            HighlightHex(pos);
        else
        {
            if (RenderedBlues[index].InSign)
                return;
            else
                RenderedBlues[index].InSign = true;
        }

        if (distance != 0)
        {
            for (byte i = 0; i < 6; ++i)
            {
                GlobalPos buf = HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i);
                if (buf.X >= 0 && buf.Y >= 0 && buf.X < map.Width && buf.Y < map.Height && !map.IsBlocked((LocalPos)buf))
                    BluesSignQueue.Enqueue(new QueueType { Pos = buf, Distance = (byte)(distance - 1) });
            }
        }
    }

    public ushort GetSpriteID(TerrainType terrain, byte index)
    {
        ushort id = 0;
        foreach (Terrain terr in LocalMapParam.Terrains)
            if (terr.TerrainType == terrain)
                return (ushort)(id + index);
            else
                id += (ushort)terr.Sprites.Length;
        throw new System.ArgumentException("Terrain not found.", "terrain");
    }
}
