using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class WorldVisualiser : MonoBehaviour
{
    [System.Serializable]
    public class Terrain
    {
        public float StartingHeight;
        public Sprite[] Sprites;
    }

    public GameObject Hex;
    public GameObject InteractableHex;
    [System.Serializable]
    public class GlobalMapSettings
    {
        public Sprite[] BottommostTerrainSprites;
        public Terrain[] Terrains;
        public Sprite[] RiverStartSprites;
        public Sprite[] RiverStraightSprites;
        public Sprite[] RiverTurnSprites;
        public Sprite[] RiverEndSprites;
        public Sprite[] TreeSprites;
        public Sprite[] RuinSprites;
        public Sprite[] RoadStraightSprites;
        public Sprite[] RoadTurnSprites;
        public Sprite[] RoadStraightBridgeSprites;
        public Sprite[] RoadTurnBridgeSprites;
    }
    public GlobalMapSettings GlobalMapParam;
    [System.Serializable]
    public class LocalMapSettings
    {
        public Sprite[] BottommostTerrainSprites;
        public Terrain[] Terrains;
        public Sprite[] BankSprites;
		public Sprite[] TreeSprites;
		public Sprite[] BushSprites;
		public float PlantProbability;
		public float BushesForestValue;
    }
    public LocalMapSettings LocalMapParam;
    public Material DiffuseMaterial;
    public byte ForestGenGridSize;
    public float FadeInTime;
    public float FadeTime;
    public Sprite BlueHexSprite;
    public static Vector2 GlobalHexSpriteSize; //TODO static? Перенести в классы?
    public static Vector2 LocalHexSpriteSize;

    List<Sprite> AllGlobalHexSprites = new List<Sprite>();
    List<Sprite> AllRiverSprites = new List<Sprite>();
    List<Sprite> AllRoadSprites = new List<Sprite>();

    List<Sprite> AllLocalHexSprites = new List<Sprite>();

    class ListType
    {
        public GameObject Hex;
        public bool InSign;
        public List<GameObject> Trees = new List<GameObject>();
        //public GameObject RiverSprite;
        //public GameObject ClusterSprite;
        public List<GameObject> LandscapeObj = new List<GameObject>();
    }

    List<ListType> RenderedHexes = new List<ListType>();
    GlobalMap[,] CashedChunks = new GlobalMap[3, 3];
    int ChunkX, ChunkY;


    class QueueType
    {
        public Vector2 MapCoords;
        public byte Distance;
    }

    Queue<QueueType> SignQueue = new Queue<QueueType>();

    List<GameObject> RenderedObjects = new List<GameObject>();
    GameObject RenderedBackground;

    void Awake()
    {
        AllGlobalHexSprites.AddRange(GlobalMapParam.BottommostTerrainSprites);
        foreach (Terrain ts in GlobalMapParam.Terrains)
            AllGlobalHexSprites.AddRange(ts.Sprites);

        AllRiverSprites.AddRange(GlobalMapParam.RiverStartSprites);
        AllRiverSprites.AddRange(GlobalMapParam.RiverStraightSprites);
        AllRiverSprites.AddRange(GlobalMapParam.RiverTurnSprites);
        AllRiverSprites.AddRange(GlobalMapParam.RiverEndSprites);

        AllRoadSprites.AddRange(GlobalMapParam.RoadStraightSprites);
        AllRoadSprites.AddRange(GlobalMapParam.RoadTurnSprites);
        AllRoadSprites.AddRange(GlobalMapParam.RoadStraightBridgeSprites);
        AllRoadSprites.AddRange(GlobalMapParam.RoadTurnBridgeSprites);

        AllLocalHexSprites.AddRange(LocalMapParam.BottommostTerrainSprites);
        foreach (Terrain ts in LocalMapParam.Terrains)
            AllLocalHexSprites.AddRange(ts.Sprites);

        //Assert
        for (byte i = 1; i < GlobalMapParam.Terrains.Length; ++i)
            Debug.Assert(GlobalMapParam.Terrains[i - 1].StartingHeight < GlobalMapParam.Terrains[i].StartingHeight);
        for (ushort i = 1; i < AllGlobalHexSprites.Count; ++i)
            Debug.Assert(AllGlobalHexSprites[i - 1].bounds.size == AllGlobalHexSprites[i].bounds.size);
        for (ushort i = 1; i < AllLocalHexSprites.Count; ++i)
            Debug.Assert(AllLocalHexSprites[i - 1].bounds.size == AllLocalHexSprites[i].bounds.size);
        //--

        GlobalHexSpriteSize.x = GlobalMapParam.BottommostTerrainSprites[0].bounds.size.x;
        GlobalHexSpriteSize.y = GlobalMapParam.BottommostTerrainSprites[0].bounds.size.y;
        LocalHexSpriteSize.x = LocalMapParam.BottommostTerrainSprites[0].bounds.size.x;
        LocalHexSpriteSize.y = LocalMapParam.BottommostTerrainSprites[0].bounds.size.y;
    }

    /// <summary>
    /// Уничтожает все хексы.
    /// </summary>
    public void DestroyAllHexes()
    {
        RenderedHexes.ForEach(hex =>
        {
            hex.Trees.ForEach(tree => Destroy(tree));
            //Destroy(hex.RiverSprite);
            //Destroy(hex.ClusterSprite);
            hex.LandscapeObj.ForEach(obj => Destroy(obj));
            Destroy(hex.Hex);
        });
        RenderedHexes.Clear();
    }

    public void DestroyAllObjects()
    {
        RenderedObjects.ForEach(obj => Destroy(obj));
        RenderedObjects.Clear();
    }

    /// <summary>
    /// Отображает только хексы в поле зрения игрока.
    /// </summary>
    /// <param name="mapPosition">Координаты в матрице.</param>
    /// <param name="distance">Дальность обзора.</param>
    /// <param name="currentMap">Активная карта.</param>
    public void RenderVisibleHexes(Vector2 mapCoords, byte distance, GlobalMap[,] cashedChunks, int chunkY, int chunkX)
    {
        CashedChunks = cashedChunks;
        ChunkX = chunkX;
        ChunkY = chunkY;

        RenderedHexes.ForEach(hex => hex.InSign = false);

        SignQueue.Enqueue(new QueueType { MapCoords = mapCoords, Distance = distance });
        while (SignQueue.Count != 0)
        {
            QueueType buf = SignQueue.Dequeue();
            SpreadRender(buf.MapCoords, buf.Distance);
        }

        for (ushort i = 0; i < RenderedHexes.Count; ++i)
            if (!RenderedHexes[i].InSign)
            {
                // if (RenderedHexes[i].RiverSprite != null)
                //    StartCoroutine(RenderHelper.FadeAndDestroyObject(RenderedHexes[i].RiverSprite.GetComponent<Renderer>(), FadeTime));

                RenderedHexes[i].Trees.ForEach(tree => StartCoroutine(RenderHelper.FadeAndDestroyObject(tree.GetComponent<Renderer>(), FadeTime)));
                RenderedHexes[i].LandscapeObj.ForEach(obj => StartCoroutine(RenderHelper.FadeAndDestroyObject(obj.GetComponent<Renderer>(), FadeTime)));
                StartCoroutine(RenderHelper.FadeAndDestroyObject(RenderedHexes[i].Hex.GetComponent<Renderer>(), FadeTime));
                RenderedHexes.RemoveAt(i);
                --i;
            }
    }

    /// <summary>
    /// Рекурсивно заносит в очередь на отображение хексы.
    /// </summary>
    /// <param name="mapPosition">Координаты в матрице.</param>
    /// <param name="distance">Оставшееся расстояние для распространения.</param>
    void SpreadRender(Vector2 mapCoords, byte distance)
    {
        GlobalMap map;
        Vector2 chunkCoords;
        ushort chunkSize = (ushort)CashedChunks[1, 1].HeightMatrix.GetLength(0);

        float chunkX = mapCoords.x / chunkSize, chunkY = mapCoords.y / chunkSize;

        chunkX = Mathf.Floor(chunkX);
        chunkY = Mathf.Floor(chunkY);

        map = CashedChunks[(int)(chunkY - ChunkY + 1), (int)(chunkX - ChunkX + 1)];

        chunkCoords.x = mapCoords.x - chunkSize * chunkX;
        chunkCoords.y = mapCoords.y - chunkSize * chunkY;

        short index = (short)RenderedHexes.FindIndex(x => x.Hex.GetComponent<HexData>().MapCoords == mapCoords);
        if (index == -1)
        {
            ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapCoords(mapCoords, false), Quaternion.identity) as GameObject, InSign = true };
            hex.Hex.GetComponent<HexData>().MapCoords = mapCoords;
            MakeHexGraphics(hex, chunkCoords, map);
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
            SignQueue.Enqueue(new QueueType { MapCoords = HexNavigHelper.GetNeighborMapCoords(mapCoords, HexDirection.TOP_LEFT), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { MapCoords = HexNavigHelper.GetNeighborMapCoords(mapCoords, HexDirection.TOP), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { MapCoords = HexNavigHelper.GetNeighborMapCoords(mapCoords, HexDirection.TOP_RIGHT), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { MapCoords = HexNavigHelper.GetNeighborMapCoords(mapCoords, HexDirection.BOTTOM_RIGHT), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { MapCoords = HexNavigHelper.GetNeighborMapCoords(mapCoords, HexDirection.BOTTOM), Distance = (byte)(distance - 1) });
            SignQueue.Enqueue(new QueueType { MapCoords = HexNavigHelper.GetNeighborMapCoords(mapCoords, HexDirection.BOTTOM_LEFT), Distance = (byte)(distance - 1) });
        }
    }

    /// <summary>
    /// Создаёт спрайты, необходимые для отображения хекса.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    void MakeHexGraphics(ListType hex, Vector2 mapCoords, GlobalMap map)
    {
        if (map.GetHexSpriteID(mapCoords).HasValue /*?*/&& map.GetHexSpriteID(mapCoords) < AllGlobalHexSprites.Count)
            hex.Hex.GetComponent<SpriteRenderer>().sprite = AllGlobalHexSprites[map.GetHexSpriteID(mapCoords).Value];
        else
            hex.Hex.GetComponent<SpriteRenderer>().sprite = ChooseHexSprite(mapCoords, map);
        hex.Hex.GetComponent<SpriteRenderer>().sortingLayerName = "Landscape";//
        bool forestBlocked = false;
        if (map.HasRiver(mapCoords))
        {
            GameObject riverSprite = new GameObject();
            hex.LandscapeObj.Add(riverSprite);
            riverSprite.transform.position = hex.Hex.transform.position;
            riverSprite.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";
            riverSprite.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
            if (map.GetRiverSpriteID(mapCoords).HasValue /*?*/ && map.GetRiverSpriteID(mapCoords) < AllRiverSprites.Count)
            {
                riverSprite.GetComponent<SpriteRenderer>().sprite = AllRiverSprites[map.GetRiverSpriteID(mapCoords).Value];
                riverSprite.transform.Rotate(0, 0, map.GetRiverSpriteRotation(mapCoords));
            }
            else
                riverSprite.GetComponent<SpriteRenderer>().sprite = ChooseHexRiverSprite(riverSprite.transform, mapCoords, map);
            forestBlocked = true;
        }
        if (map.HasCluster(mapCoords))
        {
            GameObject clusterSprite = new GameObject();
            hex.LandscapeObj.Add(clusterSprite);
            clusterSprite.transform.position = hex.Hex.transform.position;
            clusterSprite.AddComponent<SpriteRenderer>().sortingLayerName = "Infrastructure";
            clusterSprite.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
            if (!map.GetClusterSpriteID(mapCoords).HasValue /*?*/|| map.GetClusterSpriteID(mapCoords) >= GlobalMapParam.RuinSprites.Length)
                map.ClusterSpriteID_Matrix[(int)mapCoords.y, (int)mapCoords.x] = (byte)Random.Range(0, GlobalMapParam.RuinSprites.Length);
            clusterSprite.GetComponent<SpriteRenderer>().sprite = GlobalMapParam.RuinSprites[map.GetClusterSpriteID(mapCoords).Value];
            forestBlocked = true;
        }
        if (map.HasRoad(mapCoords))
        {
            GameObject roadSprite = new GameObject();
            hex.LandscapeObj.Add(roadSprite);
            roadSprite.transform.position = hex.Hex.transform.position;
            roadSprite.AddComponent<SpriteRenderer>().sortingLayerName = "Infrastructure";
            roadSprite.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
            if (map.GetRoadSpriteID(mapCoords).HasValue /*?*/ && map.GetRoadSpriteID(mapCoords) < AllRiverSprites.Count)
            {
                roadSprite.GetComponent<SpriteRenderer>().sprite = AllRoadSprites[map.GetRoadSpriteID(mapCoords).Value];
                roadSprite.transform.Rotate(0, 0, map.GetRoadSpriteRotation(mapCoords));
            }
            else
                roadSprite.GetComponent<SpriteRenderer>().sprite = ChooseHexRoadSprite(roadSprite.transform, mapCoords, map);
            forestBlocked = true;
        }
        if (!forestBlocked)
            MakeHexForest(hex, mapCoords, map);
        StartCoroutine(RenderHelper.FadeIn(hex.Hex.GetComponent<Renderer>(), FadeInTime));
        hex.LandscapeObj.ForEach(obj => StartCoroutine(RenderHelper.FadeIn(obj.GetComponent<Renderer>(), FadeInTime)));
        hex.Trees.ForEach(tree => StartCoroutine(RenderHelper.FadeIn(tree.GetComponent<Renderer>(), FadeInTime)));
    }

    void MakeHexGraphics(ListType hex, Vector2 mapCoords, LocalMap map)
    {
        if (map.GetHexSpriteID(mapCoords).HasValue /*?*/&& map.GetHexSpriteID(mapCoords) < AllLocalHexSprites.Count)
            hex.Hex.GetComponent<SpriteRenderer>().sprite = AllLocalHexSprites[map.GetHexSpriteID(mapCoords).Value];
        else
            hex.Hex.GetComponent<SpriteRenderer>().sprite = ChooseHexSprite(mapCoords, map);
        hex.Hex.GetComponent<SpriteRenderer>().sortingLayerName = "Landscape";//
        if (map.GetHeight(mapCoords) < LocalMapParam.Terrains[0].StartingHeight)
		{
            for (byte i = 0; i < 6; ++i) //TODO К оптимизации.
                if (map.Contains(HexNavigHelper.GetNeighborMapCoords(mapCoords, (TurnedHexDirection)i)) && map.GetHeight(HexNavigHelper.GetNeighborMapCoords(mapCoords, (TurnedHexDirection)i)) > LocalMapParam.Terrains[0].StartingHeight)
                {
                    GameObject bank = new GameObject();
					hex.LandscapeObj.Add(bank);
                    bank.transform.position = hex.Hex.transform.position;
                    bank.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";
				bank.GetComponent<SpriteRenderer>().sprite = LocalMapParam.BankSprites[Random.Range(0,LocalMapParam.BankSprites.Length)];
					bank.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
                    bank.transform.Rotate(0, 0, (short)(Mathf.Sign(mapCoords.y - HexNavigHelper.GetNeighborMapCoords(mapCoords, (TurnedHexDirection)i).y) * Vector2.Angle(Vector2.left, GetTransformPosFromMapCoords(HexNavigHelper.GetNeighborMapCoords(mapCoords, (TurnedHexDirection)i), true) - (Vector2)hex.Hex.transform.position)));
                }
		}
		else if(map.GetForest(mapCoords)>0&&Random.value<LocalMapParam.PlantProbability*map.GetForest(mapCoords))
        {
            GameObject plant = new GameObject();
			hex.LandscapeObj.Add(plant);
			plant.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";
			plant.GetComponent<SpriteRenderer>().material = DiffuseMaterial;            
            if(map.GetForest(mapCoords)<LocalMapParam.BushesForestValue)
				plant.GetComponent<SpriteRenderer>().sprite=LocalMapParam.BushSprites[Random.Range(0,LocalMapParam.BushSprites.Length)];
			else				
				plant.GetComponent<SpriteRenderer>().sprite=LocalMapParam.TreeSprites[Random.Range(0,LocalMapParam.TreeSprites.Length)];
			plant.transform.position =new Vector2(hex.Hex.transform.position.x,hex.Hex.transform.position.y+plant.GetComponent<SpriteRenderer>().sprite.bounds.extents.y-LocalHexSpriteSize.y/2);
        }
        StartCoroutine(RenderHelper.FadeIn(hex.Hex.GetComponent<Renderer>(), FadeInTime));
        hex.LandscapeObj.ForEach(obj => StartCoroutine(RenderHelper.FadeIn(obj.GetComponent<Renderer>(), FadeInTime)));
    }

    /// <summary>
    /// Выбирает спрайт хекса.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    Sprite ChooseHexSprite(Vector2 mapCoords, GlobalMap map) //UNDONE
    {
        byte id = 0;
        if (map.GetHeight(mapCoords) < GlobalMapParam.Terrains[0].StartingHeight)
            id = (byte)Random.Range(0, GlobalMapParam.BottommostTerrainSprites.Length);
        else if (map.GetHeight(mapCoords) >= GlobalMapParam.Terrains[GlobalMapParam.Terrains.Length - 1].StartingHeight)
        {
            for (byte i = 0; i < GlobalMapParam.Terrains.Length - 1; ++i, id += (byte)GlobalMapParam.Terrains[i].Sprites.Length) ;
            id += (byte)(Random.Range(0, GlobalMapParam.Terrains[GlobalMapParam.Terrains.Length - 1].Sprites.Length) + GlobalMapParam.BottommostTerrainSprites.Length);
        }
        else
            for (byte i = 1; i < GlobalMapParam.Terrains.Length; i++)
                if (map.GetHeight(mapCoords) >= GlobalMapParam.Terrains[i - 1].StartingHeight && map.GetHeight(mapCoords) < GlobalMapParam.Terrains[i].StartingHeight)
                {
                    for (byte j = 0; j < i - 1; ++j, id += (byte)GlobalMapParam.Terrains[j].Sprites.Length) ;
                    id += (byte)(Random.Range(0, GlobalMapParam.Terrains[i - 1].Sprites.Length) + GlobalMapParam.BottommostTerrainSprites.Length);
                }

        map.HexSpriteID_Matrix[(int)mapCoords.y, (int)mapCoords.x] = id;
        return AllGlobalHexSprites[id];
    }

    Sprite ChooseHexSprite(Vector2 mapCoords, LocalMap map) //UNDONE
    {
        byte id = 0;
        if (map.GetHeight(mapCoords) < LocalMapParam.Terrains[0].StartingHeight)
            id = (byte)Random.Range(0, LocalMapParam.BottommostTerrainSprites.Length);
        else if (map.GetHeight(mapCoords) >= LocalMapParam.Terrains[LocalMapParam.Terrains.Length - 1].StartingHeight)
        {
            for (byte i = 0; i < LocalMapParam.Terrains.Length - 1; ++i, id += (byte)LocalMapParam.Terrains[i].Sprites.Length) ;
            id += (byte)(Random.Range(0, LocalMapParam.Terrains[LocalMapParam.Terrains.Length - 1].Sprites.Length) + LocalMapParam.BottommostTerrainSprites.Length);
        }
        else
            for (byte i = 1; i < LocalMapParam.Terrains.Length; i++)
                if (map.GetHeight(mapCoords) >= LocalMapParam.Terrains[i - 1].StartingHeight && map.GetHeight(mapCoords) < LocalMapParam.Terrains[i].StartingHeight)
                {
                    for (byte j = 0; j < i - 1; ++j, id += (byte)LocalMapParam.Terrains[j].Sprites.Length) ;
                    id += (byte)(Random.Range(0, LocalMapParam.Terrains[i - 1].Sprites.Length) + LocalMapParam.BottommostTerrainSprites.Length);
                }

        map.HexSpriteID_Matrix[(int)mapCoords.y, (int)mapCoords.x] = id;
        return AllLocalHexSprites[id];
    }

    Sprite ChooseHexRiverSprite(Transform spriteTransform, Vector2 mapCoords, GlobalMap map)//TODO К оптимизации.
    {
        foreach (List<Vector2> river in map.Rivers)
        {
            byte id;
            short angle;
            short index = (short)river.IndexOf(mapCoords);
            if (index != -1)
            {
                if (index == 0)
                {
                    id = (byte)Random.Range(0, GlobalMapParam.RiverStartSprites.Length);
                    angle = (short)(Mathf.Sign(river[1].x - mapCoords.x) * Vector2.Angle(Vector2.down, GetTransformPosFromMapCoords(river[1], false) - (Vector2)spriteTransform.position));
                }
                else if (index == river.Count - 1)
                {
                    id = (byte)(Random.Range(0, GlobalMapParam.RiverEndSprites.Length) + GlobalMapParam.RiverStartSprites.Length + GlobalMapParam.RiverStraightSprites.Length + GlobalMapParam.RiverTurnSprites.Length);
                    angle = (short)(Mathf.Sign(river[river.Count - 2].x - mapCoords.x) * Vector2.Angle(Vector2.down, GetTransformPosFromMapCoords(river[river.Count - 2], false) - (Vector2)spriteTransform.position));
                }
                else
                {
                    Vector2 prev = GetTransformPosFromMapCoords(river[index - 1], false) - (Vector2)spriteTransform.position;
                    if ((short)Vector2.Angle(GetTransformPosFromMapCoords(river[index - 1], false) - (Vector2)spriteTransform.position, GetTransformPosFromMapCoords(river[index + 1], false) - (Vector2)spriteTransform.position) > 150) //(==180, !=120)
                    {
                        id = (byte)(Random.Range(0, GlobalMapParam.RiverStraightSprites.Length) + GlobalMapParam.RiverStartSprites.Length);
                        angle = (short)(Mathf.Sign(river[index - 1].x - mapCoords.x) * Vector2.Angle(Vector2.down, prev));
                    }
                    else
                    {
                        id = (byte)(Random.Range(0, GlobalMapParam.RiverTurnSprites.Length) + GlobalMapParam.RiverStartSprites.Length + GlobalMapParam.RiverStraightSprites.Length);

                        Vector2 next = GetTransformPosFromMapCoords(river[index + 1], false) - (Vector2)spriteTransform.position;

                        angle = (short)(Mathf.Sign(river[index - 1].x - mapCoords.x) * Vector2.Angle(Vector2.down, prev));
                        if (Mathf.Approximately(prev.x, 0))
                            angle += (short)(240 * (Mathf.Sign(prev.y) == Mathf.Sign(next.x) ? 1 : 0));
                        else
                            angle += (short)(240 * (Mathf.Sign(prev.x) != Mathf.Sign(next.y) ? 1 : 0));
                    }
                }
            }
            else
                continue;

            map.RiverSpriteID_Matrix[(int)mapCoords.y, (int)mapCoords.x] = id;
            map.RiverSpriteRotationMatrix[(int)mapCoords.y, (int)mapCoords.x] = angle;
            spriteTransform.Rotate(0, 0, angle);

            return AllRiverSprites[id]; ;
        }
        return null;
    }

    Sprite ChooseHexRoadSprite(Transform spriteTransform, Vector2 mapCoords, GlobalMap map)//TODO К оптимизации.
    {
        foreach (List<Vector2> road in map.Roads)
        {
            byte id;
            short angle;
            short index = (short)road.IndexOf(mapCoords);
            if (index != -1)
            {
                if (index == 0)
                {
                    id = (byte)(map.HasRiver(mapCoords) ? Random.Range(0, GlobalMapParam.RoadStraightBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length : Random.Range(0, GlobalMapParam.RoadStraightSprites.Length));
                    angle = (short)(Mathf.Sign(road[1].x - mapCoords.x) * Vector2.Angle(Vector2.down, GetTransformPosFromMapCoords(road[1], false) - (Vector2)spriteTransform.position));
                }
                else if (index == road.Count - 1)
                {
                    id = (byte)(map.HasRiver(mapCoords) ? Random.Range(0, GlobalMapParam.RoadStraightBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length : Random.Range(0, GlobalMapParam.RoadStraightSprites.Length));
                    angle = (short)(Mathf.Sign(road[road.Count - 2].x - mapCoords.x) * Vector2.Angle(Vector2.down, GetTransformPosFromMapCoords(road[road.Count - 2], false) - (Vector2)spriteTransform.position));
                }
                else
                {
                    Vector2 prev = GetTransformPosFromMapCoords(road[index - 1], false) - (Vector2)spriteTransform.position;
                    if ((short)Vector2.Angle(GetTransformPosFromMapCoords(road[index - 1], false) - (Vector2)spriteTransform.position, GetTransformPosFromMapCoords(road[index + 1], false) - (Vector2)spriteTransform.position) > 150) //(==180, !=120)
                    {
                        id = (byte)(map.HasRiver(mapCoords) ? Random.Range(0, GlobalMapParam.RoadStraightBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length : Random.Range(0, GlobalMapParam.RoadStraightSprites.Length));
                        angle = (short)(Mathf.Sign(road[index - 1].x - mapCoords.x) * Vector2.Angle(Vector2.down, prev));
                    }
                    else
                    {
                        id = (byte)(map.HasRiver(mapCoords) ? Random.Range(0, GlobalMapParam.RoadTurnBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length + GlobalMapParam.RoadStraightBridgeSprites.Length : Random.Range(0, GlobalMapParam.RoadTurnSprites.Length) + GlobalMapParam.RoadStraightSprites.Length);

                        Vector2 next = GetTransformPosFromMapCoords(road[index + 1], false) - (Vector2)spriteTransform.position;

                        angle = (short)(Mathf.Sign(road[index - 1].x - mapCoords.x) * Vector2.Angle(Vector2.down, prev));
                        if (Mathf.Approximately(prev.x, 0))
                            angle += (short)(240 * (Mathf.Sign(prev.y) == Mathf.Sign(next.x) ? 1 : 0));
                        else
                            angle += (short)(240 * (Mathf.Sign(prev.x) != Mathf.Sign(next.y) ? 1 : 0));
                    }
                }
            }
            else
                continue;

            map.RoadSpriteID_Matrix[(int)mapCoords.y, (int)mapCoords.x] = id;
            map.RoadSpriteRotationMatrix[(int)mapCoords.y, (int)mapCoords.x] = angle;
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
    void MakeHexForest(ListType hex, Vector2 mapCoords, Map map) //TODO (WIP)
    {
        Vector2 spriteSize = hex.Hex.GetComponent<SpriteRenderer>().sprite.bounds.size;
        if (map.GetForest(mapCoords) >= 1) //TODO
        {
            float gridStepX = spriteSize.x / ForestGenGridSize;
            float gridStepY = spriteSize.y / ForestGenGridSize;
            Vector2 gridOrigin = new Vector2(hex.Hex.transform.position.x - spriteSize.x * 0.375f, hex.Hex.transform.position.y - spriteSize.y * 0.5f);
            byte treesCount = (byte)map.GetForest(mapCoords);

            while (true)
            {
                if (treesCount > ForestGenGridSize * ForestGenGridSize)
                {
                    for (float y = 0; y < spriteSize.y; y += gridStepY)
                        for (float x = 0; x < spriteSize.x; x += gridStepX)
                        {
                            Vector2 v = new Vector2(Random.value * gridStepX, Random.value * gridStepY);
                            GameObject tree = new GameObject();
                            hex.Trees.Add(tree);
                            tree.transform.position = new Vector2(gridOrigin.x + x + v.x, gridOrigin.y + y + v.y);
                            tree.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";//
                            tree.GetComponent<SpriteRenderer>().sprite = GlobalMapParam.TreeSprites[Random.Range(0, GlobalMapParam.TreeSprites.Length)];
                            tree.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
                            --treesCount;
                        }
                }
                else
                {
                    Vector2 v = Random.insideUnitCircle;
                    v.x *= spriteSize.x * 0.5f;
                    v.y *= spriteSize.y * 0.5f;
                    GameObject tree = new GameObject();
                    hex.Trees.Add(tree);
                    tree.transform.position = new Vector2(hex.Hex.transform.position.x + v.x, hex.Hex.transform.position.y + v.y);
                    tree.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";//
                    tree.GetComponent<SpriteRenderer>().sprite = GlobalMapParam.TreeSprites[Random.Range(0, GlobalMapParam.TreeSprites.Length)];
                    tree.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
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
        ushort height = (ushort)map.HeightMatrix.GetLength(0);
        ushort width = (ushort)map.HeightMatrix.GetLength(1);
        RenderedHexes.Capacity = height * width;

        for (ushort y = 0; y < height; ++y)
            for (ushort x = 0; x < width; ++x)
            {
                // TODO Возможно стоит заменить ListType на Hex?
                ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapCoords(new Vector2(x, y), true), Quaternion.identity) as GameObject, InSign = true };
                hex.Hex.GetComponent<HexData>().MapCoords = new Vector2(x, y);
                MakeHexGraphics(hex, new Vector2(x, y), map);
                RenderedHexes.Add(hex);
            }
    }

    /// <summary>
    /// Вычисляет координаты в сцене из координат на карте.
    /// </summary>
    /// <returns>Координаты в сцене.</returns>
    /// <param name="mapCoords">Координаты на карте.</param>
    public static Vector2 GetTransformPosFromMapCoords(Vector2 mapCoords, bool localMap)
    {
        if (localMap)
            return new Vector2(mapCoords.x * LocalHexSpriteSize.x + ((mapCoords.y % 2) != 0 ? 1 : 0) * LocalHexSpriteSize.x * 0.5f, mapCoords.y * LocalHexSpriteSize.y * 0.75f);
        else
            return new Vector2(mapCoords.x * GlobalHexSpriteSize.x * 0.75f, mapCoords.y * GlobalHexSpriteSize.y + ((mapCoords.x % 2) != 0 ? 1 : 0) * GlobalHexSpriteSize.y * 0.5f);
    }

    /// <summary>
    /// Накладывает на хекс спрайт.
    /// </summary>
    /// <param name="mapCoords">Координаты хекса.</param>
    /// <param name="highlightHexSprite">Спрайт.</param>
    public void HighlightHex(Vector2 mapCoords, Sprite highlightHexSprite, bool localMap)
    {
        RenderedObjects.Add(Instantiate(InteractableHex, GetTransformPosFromMapCoords(mapCoords, localMap), localMap ? Quaternion.Euler(0, 0, 90) : Quaternion.identity) as GameObject);
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sprite = highlightHexSprite;
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeHighlights";

        RenderedObjects[RenderedObjects.Count - 1].GetComponent<HexData>().MapCoords = mapCoords;//TODO временно
    }
}
