﻿using UnityEngine;
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
        public Sprite[] WaterSprites;
        public Terrain[] Terrains;
        public Sprite[] BankSprites;
        public Sprite[] DiagBankSprites;
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
    Chunk[,] CashedChunks = new Chunk[3, 3];
    int ChunkX, ChunkY;


    class QueueType
    {
        public GlobalPos Pos;
        public byte Distance;
    }

    Queue<QueueType> SignQueue = new Queue<QueueType>();

    List<GameObject> RenderedObjects = new List<GameObject>();
    GameObject RenderedBackground;

    void Awake()
    {
        AllGlobalHexSprites.AddRange(GlobalMapParam.BottommostTerrainSprites);
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

        AllLocalHexSprites.AddRange(LocalMapParam.WaterSprites);
        System.Array.ForEach(LocalMapParam.Terrains, ts => AllLocalHexSprites.AddRange(ts.Sprites));
        //foreach (Terrain ts in LocalMapParam.Terrains)
        //AllLocalHexSprites.AddRange(ts.Sprites);

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
        LocalHexSpriteSize.x = LocalMapParam.WaterSprites[0].bounds.size.x;
        LocalHexSpriteSize.y = LocalMapParam.WaterSprites[0].bounds.size.y;
    }

    /// <summary>
    /// Уничтожает все хексы.
    /// </summary>
    public void DestroyAllHexes()
    {
        foreach (ListType hex in RenderedHexes)
        {
            hex.Trees.ForEach(Destroy);
            //Destroy(hex.RiverSprite);
            //Destroy(hex.ClusterSprite);
            hex.LandscapeObj.ForEach(Destroy);
            Destroy(hex.Hex);
        }
        RenderedHexes.Clear();
    }

    public void DestroyAllObjects()
    {
        RenderedObjects.ForEach(Destroy);
        RenderedObjects.Clear();
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
                // if (RenderedHexes[i].RiverSprite != null)
                //    StartCoroutine(RenderHelper.FadeAndDestroyObject(RenderedHexes[i].RiverSprite.GetComponent<Renderer>(), FadeTime));

                RenderedHexes[i].Trees.ForEach(tree => tree.GetComponent<Fader>().FadeAndDestroyObject(FadeTime));
                RenderedHexes[i].LandscapeObj.ForEach(obj => obj.GetComponent<Fader>().FadeAndDestroyObject(FadeTime));
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

        short index = (short)RenderedHexes.FindIndex(x => x.Hex.GetComponent<HexData>().Pos == pos);
        if (index == -1)
        {
            ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapPos(pos), Quaternion.identity) as GameObject, InSign = true };
            hex.Hex.GetComponent<HexData>().Pos = pos;
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
        if (map.RiverMatrix[pos.Y, pos.X])
        {
            GameObject riverSprite = new GameObject();
            hex.LandscapeObj.Add(riverSprite);
            riverSprite.transform.position = hex.Hex.transform.position;
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
        if (map.ClusterMatrix[pos.Y, pos.X])
        {
            GameObject clusterSprite = new GameObject();
            hex.LandscapeObj.Add(clusterSprite);
            clusterSprite.transform.position = hex.Hex.transform.position;
            clusterSprite.AddComponent<SpriteRenderer>().sortingLayerName = "Infrastructure";
            clusterSprite.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
            if (!map.ClusterSpriteID_Matrix[pos.Y, pos.X].HasValue || map.ClusterSpriteID_Matrix[pos.Y, pos.X] >= GlobalMapParam.RuinSprites.Length)
                map.ClusterSpriteID_Matrix[pos.Y, pos.X] = (byte)Random.Range(0, GlobalMapParam.RuinSprites.Length);
            clusterSprite.GetComponent<SpriteRenderer>().sprite = GlobalMapParam.RuinSprites[map.ClusterSpriteID_Matrix[pos.Y, pos.X].Value];
            clusterSprite.AddComponent<Fader>();
            forestBlocked = true;
        }
        if (map.RoadMatrix[pos.Y, pos.X])
        {
            GameObject roadSprite = new GameObject();
            hex.LandscapeObj.Add(roadSprite);
            roadSprite.transform.position = hex.Hex.transform.position;
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
        hex.LandscapeObj.ForEach(obj => obj.GetComponent<Fader>().FadeIn(FadeInTime));
        hex.Trees.ForEach(tree => tree.GetComponent<Fader>().FadeIn(FadeInTime));
    }

    void MakeHexGraphics(ListType hex, LocalPos pos, LocalMap map)
    {
        if (map.HexSpriteID_Matrix[pos.Y, pos.X].HasValue /*?*/&& map.HexSpriteID_Matrix[pos.Y, pos.X] < AllLocalHexSprites.Count)
            hex.Hex.GetComponent<SpriteRenderer>().sprite = AllLocalHexSprites[map.HexSpriteID_Matrix[pos.Y, pos.X].Value];
        else
            hex.Hex.GetComponent<SpriteRenderer>().sprite = ChooseHexSprite(pos, map);
        hex.Hex.GetComponent<SpriteRenderer>().sortingLayerName = "Landscape";//
        if (map.HeightMatrix[pos.Y, pos.X] < LocalMapParam.Terrains[0].StartingHeight)
        {
            //float offsetbuf=(LocalHexSpriteSize.pos.X-LocalHexSpriteSize.y)/2;
            for (byte i = 0; i < 6; ++i) //TODO К оптимизации.
                if (map.Contains(HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i)) && map.HeightMatrix[HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i).Y, HexNavigHelper.GetNeighborMapCoords(pos, (TurnedHexDirection)i).X] > LocalMapParam.Terrains[0].StartingHeight)
                {
                    GameObject bank = new GameObject();
                    hex.LandscapeObj.Add(bank);
                    bank.transform.position = hex.Hex.transform.position;
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
        }
        else
            if (map.ForestMatrix[pos.Y, pos.X] > 0 && Random.value < LocalMapParam.PlantProbability * map.ForestMatrix[pos.Y, pos.X])
            {
                GameObject plant = new GameObject();
                hex.LandscapeObj.Add(plant);
                plant.AddComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";
                plant.GetComponent<SpriteRenderer>().material = DiffuseMaterial;
                if (map.ForestMatrix[pos.Y, pos.X] < LocalMapParam.BushesForestValue)
                    plant.GetComponent<SpriteRenderer>().sprite = LocalMapParam.BushSprites[Random.Range(0, LocalMapParam.BushSprites.Length)];
                else
                    plant.GetComponent<SpriteRenderer>().sprite = LocalMapParam.TreeSprites[Random.Range(0, LocalMapParam.TreeSprites.Length)];
                plant.transform.position = new Vector2(hex.Hex.transform.position.x, hex.Hex.transform.position.y + plant.GetComponent<SpriteRenderer>().sprite.bounds.extents.y - LocalHexSpriteSize.y * 0.5f);
                plant.AddComponent<Fader>();
            }
        hex.Hex.GetComponent<Fader>().FadeIn(FadeInTime);
        hex.LandscapeObj.ForEach(obj => obj.GetComponent<Fader>().FadeIn(FadeInTime));
    }

    /// <summary>
    /// Выбирает спрайт хекса.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    Sprite ChooseHexSprite(LocalPos pos, Chunk map) //UNDONE
    {
        byte id = 0;
        if (map.HeightMatrix[pos.Y, pos.X] < GlobalMapParam.Terrains[0].StartingHeight)
            id = (byte)Random.Range(0, GlobalMapParam.BottommostTerrainSprites.Length);
        else if (map.HeightMatrix[pos.Y, pos.X] >= GlobalMapParam.Terrains[GlobalMapParam.Terrains.Length - 1].StartingHeight)
        {
            for (byte i = 0; i < GlobalMapParam.Terrains.Length - 1; ++i, id += (byte)GlobalMapParam.Terrains[i].Sprites.Length) ;
            id += (byte)(Random.Range(0, GlobalMapParam.Terrains[GlobalMapParam.Terrains.Length - 1].Sprites.Length) + GlobalMapParam.BottommostTerrainSprites.Length);
        }
        else
            for (byte i = 1; i < GlobalMapParam.Terrains.Length; i++)
                if (map.HeightMatrix[pos.Y, pos.X] >= GlobalMapParam.Terrains[i - 1].StartingHeight && map.HeightMatrix[pos.Y, pos.X] < GlobalMapParam.Terrains[i].StartingHeight)
                {
                    for (byte j = 0; j < i - 1; ++j, id += (byte)GlobalMapParam.Terrains[j].Sprites.Length) ;
                    id += (byte)(Random.Range(0, GlobalMapParam.Terrains[i - 1].Sprites.Length) + GlobalMapParam.BottommostTerrainSprites.Length);
                }

        map.HexSpriteID_Matrix[pos.Y, pos.X] = id;
        return AllGlobalHexSprites[id];
    }

    Sprite ChooseHexSprite(LocalPos pos, LocalMap map) //UNDONE
    {
        byte id = 0;
        if (map.HeightMatrix[pos.Y, pos.X] < LocalMapParam.Terrains[0].StartingHeight)
            id = (byte)Random.Range(0, LocalMapParam.WaterSprites.Length);
        else if (map.HeightMatrix[pos.Y, pos.X] >= LocalMapParam.Terrains[LocalMapParam.Terrains.Length - 1].StartingHeight)
        {
            for (byte i = 0; i < LocalMapParam.Terrains.Length - 1; ++i, id += (byte)LocalMapParam.Terrains[i].Sprites.Length) ;
            id += (byte)(Random.Range(0, LocalMapParam.Terrains[LocalMapParam.Terrains.Length - 1].Sprites.Length) + LocalMapParam.WaterSprites.Length);
        }
        else
            for (byte i = 1; i < LocalMapParam.Terrains.Length; i++)
                if (map.HeightMatrix[pos.Y, pos.X] >= LocalMapParam.Terrains[i - 1].StartingHeight && map.HeightMatrix[pos.Y, pos.X] < LocalMapParam.Terrains[i].StartingHeight)
                {
                    for (byte j = 0; j < i - 1; ++j, id += (byte)LocalMapParam.Terrains[j].Sprites.Length) ;
                    id += (byte)(Random.Range(0, LocalMapParam.Terrains[i - 1].Sprites.Length) + LocalMapParam.WaterSprites.Length);
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
                    id = (byte)(map.RiverMatrix[pos.Y, pos.X] ? Random.Range(0, GlobalMapParam.RoadStraightBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length : Random.Range(0, GlobalMapParam.RoadStraightSprites.Length));
                    angle = (short)(Mathf.Sign(road[1].X - pos.X) * Vector2.Angle(Vector2.down, GetTransformPosFromMapPos((GlobalPos)road[1]) - inchunkPos));
                }
                else if (index == road.Count - 1)
                {
                    id = (byte)(map.RiverMatrix[pos.Y, pos.X] ? Random.Range(0, GlobalMapParam.RoadStraightBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length : Random.Range(0, GlobalMapParam.RoadStraightSprites.Length));
                    angle = (short)(Mathf.Sign(road[road.Count - 2].X - pos.X) * Vector2.Angle(Vector2.down, GetTransformPosFromMapPos((GlobalPos)road[road.Count - 2]) - inchunkPos));
                }
                else
                {
                    Vector2 prev = GetTransformPosFromMapPos((GlobalPos)road[index - 1]) - inchunkPos;
                    Vector2 next = GetTransformPosFromMapPos((GlobalPos)road[index + 1]) - inchunkPos;
                    if ((short)Vector2.Angle(prev, next) > 150) //(==180, !=120)
                    {
                        id = (byte)(map.RiverMatrix[pos.Y, pos.X] ? Random.Range(0, GlobalMapParam.RoadStraightBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length : Random.Range(0, GlobalMapParam.RoadStraightSprites.Length));
                        angle = (short)(Mathf.Sign(road[index - 1].X - pos.X) * Vector2.Angle(Vector2.down, prev));
                    }
                    else
                    {
                        id = (byte)(map.RiverMatrix[pos.Y, pos.X] ? Random.Range(0, GlobalMapParam.RoadTurnBridgeSprites.Length) + GlobalMapParam.RoadStraightSprites.Length + GlobalMapParam.RoadTurnSprites.Length + GlobalMapParam.RoadStraightBridgeSprites.Length : Random.Range(0, GlobalMapParam.RoadTurnSprites.Length) + GlobalMapParam.RoadStraightSprites.Length);
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
    void MakeHexForest(ListType hex, LocalPos pos, Map map) //TODO (WIP)
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
                            GameObject tree = new GameObject();
                            hex.Trees.Add(tree);
                            tree.transform.position = new Vector2(gridOrigin.x + x + v.x, gridOrigin.y + y + v.y);
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
                    GameObject tree = new GameObject();
                    hex.Trees.Add(tree);
                    tree.transform.position = new Vector2(hex.Hex.transform.position.x + v.x, hex.Hex.transform.position.y + v.y);
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
                ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapPos(new LocalPos(x, y)), Quaternion.identity) as GameObject, InSign = true };
                hex.Hex.GetComponent<HexData>().Pos = new GlobalPos(x, y);//TODO new?
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
        return new Vector2(pos.X * LocalHexSpriteSize.x + (pos.Y % 2) * LocalHexSpriteSize.x * 0.5f, pos.Y * LocalHexSpriteSize.y * 0.75f);
    }

    public static Vector2 GetTransformPosFromMapPos(GlobalPos pos)//C#6.0 EBD
    {
        return new Vector2(pos.X * GlobalHexSpriteSize.x * 0.75f, pos.Y * GlobalHexSpriteSize.y + ((pos.X % 2) != 0 ? 1 : 0) * GlobalHexSpriteSize.y * 0.5f);
    }


    /// <summary>
    /// Накладывает на хекс спрайт.
    /// </summary>
    /// <param name="mapCoords">Координаты хекса.</param>
    /// <param name="highlightHexSprite">Спрайт.</param>
    public void HighlightHex(GlobalPos pos, Sprite highlightHexSprite)
    {
        RenderedObjects.Add(Instantiate(InteractableHex, GetTransformPosFromMapPos(pos), Quaternion.identity) as GameObject);
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sprite = highlightHexSprite;
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeHighlights";

        RenderedObjects[RenderedObjects.Count - 1].GetComponent<HexData>().Pos = pos;//TODO временно
    }

    public void HighlightHex(LocalPos pos, Sprite highlightHexSprite)
    {
        RenderedObjects.Add(Instantiate(InteractableHex, GetTransformPosFromMapPos(pos), Quaternion.Euler(0, 0, 90)) as GameObject);
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sprite = highlightHexSprite;
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeHighlights";

        RenderedObjects[RenderedObjects.Count - 1].GetComponent<HexData>().Pos = pos;//TODO временно
    }

    public byte[] GetTerrainsSpriteID()
    {
        byte[] buf = new byte[1 + LocalMapParam.Terrains.Length];
        buf[0] = 0;
        buf[1] = (byte)LocalMapParam.WaterSprites.Length;
        for (byte i = 0; i < LocalMapParam.Terrains.Length - 1; ++i)
            buf[i + 2] = (byte)LocalMapParam.Terrains[i].Sprites.Length;
        return buf;
    }
}
