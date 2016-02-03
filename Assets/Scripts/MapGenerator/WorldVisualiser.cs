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
    public GameObject Background;
    public Vector2 LocalMapBackgroundOffset;
    public Sprite[] BottommostTerrainSprites;
    public Terrain[] Terrains;
    public Sprite[] RiverStartSprites;
    public Sprite[] RiverStraightSprites;
    public Sprite[] RiverTurnSprites;
    public Sprite[] RiverEndSprites;
    public Sprite[] TreeSprites;
    public Sprite[] RuinSprites;
    public byte ForestDensity;
    public byte ForestGenGridSize;
    public float FadeInTime;
    public float FadeTime;
    public Sprite TransparentSprite;
    public Sprite BlueHexSprite;
    static Vector2 HexSpriteSize;

    List<Sprite> AllHexSprites = new List<Sprite>();
    List<Sprite> AllRiverSprites = new List<Sprite>();

    class ListType
    {
        public GameObject Hex;
        public bool InSign;
        public List<GameObject> Trees = new List<GameObject>();
        public GameObject RiverSprite;
        public GameObject ClusterSprite;
    }

    List<ListType> RenderedHexes = new List<ListType>();
    Map[,] CashedChunks = new Map[3, 3];
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
        //Assert UNDONE
        //Debug.Assert(BottommostTerrainSprite.bounds.size.x == River.bounds.size.x && River.bounds.size.x == BlueHexSprite.bounds.size.x && BottommostTerrainSprite.bounds.size.y == River.bounds.size.y && River.bounds.size.y == BlueHexSprite.bounds.size.y); //TODO Временно

        for (byte i = 1; i < Terrains.Length; ++i)
        {
            Debug.Assert(Terrains[i - 1].StartingHeight < Terrains[i].StartingHeight);
            for (byte j = 1; j < Terrains[i - 1].Sprites.Length; ++j)
                Debug.Assert(Terrains[i - 1].Sprites[j - 1].bounds.size.x == Terrains[i - 1].Sprites[j].bounds.size.x && Terrains[i - 1].Sprites[j - 1].bounds.size.y == Terrains[i - 1].Sprites[j].bounds.size.y);
        }
        //--

        HexSpriteSize.x = BottommostTerrainSprites[0].bounds.size.x;
        HexSpriteSize.y = BottommostTerrainSprites[0].bounds.size.y;

        AllHexSprites.AddRange(BottommostTerrainSprites);
        foreach (Terrain ts in Terrains)
            AllHexSprites.AddRange(ts.Sprites);

        AllRiverSprites.AddRange(RiverStartSprites);
        AllRiverSprites.AddRange(RiverStraightSprites);
        AllRiverSprites.AddRange(RiverTurnSprites);
        AllRiverSprites.AddRange(RiverEndSprites);
    }

    /// <summary>
    /// Уничтожает все хексы.
    /// </summary>
    public void DestroyAllHexes()
    {
        RenderedHexes.ForEach(hex =>
        {
            hex.Trees.ForEach(tree => Destroy(tree));
            Destroy(hex.RiverSprite);
            Destroy(hex.ClusterSprite);
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
    public void RenderVisibleHexes(Vector2 mapCoords, byte distance, Map[,] cashedChunks, int chunkY, int chunkX)
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
                if (RenderedHexes[i].RiverSprite != null)
                    StartCoroutine(RenderHelper.FadeAndDestroyObject(RenderedHexes[i].RiverSprite.GetComponent<Renderer>(), FadeTime));
                RenderedHexes[i].Trees.ForEach(tree => StartCoroutine(RenderHelper.FadeAndDestroyObject(tree.GetComponent<Renderer>(), FadeTime)));
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
        Map map;
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
            ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapCoords(mapCoords), Quaternion.identity) as GameObject, InSign = true };
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
            byte k = (byte)((mapCoords.x % 2) != 0 ? 1 : 0); // Учитываем чётность/нечётность ряда хексов

            SignQueue.Enqueue(new QueueType { MapCoords = new Vector2(mapCoords.x - 1, mapCoords.y - 1 + k), Distance = (byte)(distance - 1) });

            SignQueue.Enqueue(new QueueType { MapCoords = new Vector2(mapCoords.x - 1, mapCoords.y + k), Distance = (byte)(distance - 1) });

            SignQueue.Enqueue(new QueueType { MapCoords = new Vector2(mapCoords.x, mapCoords.y - 1), Distance = (byte)(distance - 1) });

            SignQueue.Enqueue(new QueueType { MapCoords = new Vector2(mapCoords.x, mapCoords.y + 1), Distance = (byte)(distance - 1) });

            SignQueue.Enqueue(new QueueType { MapCoords = new Vector2(mapCoords.x + 1, mapCoords.y - 1 + k), Distance = (byte)(distance - 1) });

            SignQueue.Enqueue(new QueueType { MapCoords = new Vector2(mapCoords.x + 1, mapCoords.y + k), Distance = (byte)(distance - 1) });
        }
    }

    /// <summary>
    /// Создаёт спрайты, необходимые для отображения хекса.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    void MakeHexGraphics(ListType hex, Vector2 mapCoords, Map map)
    {
        if (map.GetHexSpriteID(mapCoords) != null /*?*/&& map.GetHexSpriteID(mapCoords) < AllHexSprites.Count)
            hex.Hex.GetComponent<SpriteRenderer>().sprite = AllHexSprites[(int)map.GetHexSpriteID(mapCoords)];
        else
            hex.Hex.GetComponent<SpriteRenderer>().sprite = ChooseHexSprite(mapCoords, map);
        hex.Hex.GetComponent<SpriteRenderer>().sortingLayerName = "Landscape";//
        StartCoroutine(RenderHelper.FadeIn(hex.Hex.GetComponent<Renderer>(), FadeInTime));
        if (map.HasRiver(mapCoords))
        {
            hex.RiverSprite = new GameObject();
            hex.RiverSprite.transform.position = GetTransformPosFromMapCoords(mapCoords);
            hex.RiverSprite.AddComponent<SpriteRenderer>();
            hex.RiverSprite.GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";
            if (map.GetRiverSpriteID(mapCoords) != null /*?*/&& map.GetRiverSpriteID(mapCoords) < AllRiverSprites.Count)
            {
                hex.RiverSprite.GetComponent<SpriteRenderer>().sprite = AllRiverSprites[(int)map.GetRiverSpriteID(mapCoords)];
                hex.RiverSprite.transform.Rotate(new Vector3(0, 0, map.GetRiverSpriteRotation(mapCoords)));
            }
            else
                hex.RiverSprite.GetComponent<SpriteRenderer>().sprite = ChooseHexRiverSprite(hex.RiverSprite.transform, mapCoords, map);
        }
        if (map.HasCluster(mapCoords))
        {
            hex.ClusterSprite = new GameObject();
            hex.ClusterSprite.transform.position = GetTransformPosFromMapCoords(mapCoords);
            hex.ClusterSprite.AddComponent<SpriteRenderer>();
            hex.ClusterSprite.GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";
            if (map.GetClusterSpriteID(mapCoords) != null /*?*/&& map.GetClusterSpriteID(mapCoords) < RuinSprites.Length)
                hex.ClusterSprite.GetComponent<SpriteRenderer>().sprite = RuinSprites[(int)map.GetClusterSpriteID(mapCoords)];
            else
                map.ClusterSpriteID_Matrix[(int)mapCoords.y, (int)mapCoords.x] = (byte)Random.Range(0, RuinSprites.Length);
            hex.ClusterSprite.GetComponent<SpriteRenderer>().sprite = RuinSprites[(int)map.GetClusterSpriteID(mapCoords)];
        }
        else
            MakeHexForest(hex, mapCoords, map);
    }

    /// <summary>
    /// Выбирает спрайт хекса.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    Sprite ChooseHexSprite(Vector2 mapCoords, Map map) //UNDONE
    {
        byte id = 0;
        if (map.GetHeight(mapCoords) < Terrains[0].StartingHeight)
            id = (byte)Random.Range(0, BottommostTerrainSprites.Length);
        else if (map.GetHeight(mapCoords) >= Terrains[Terrains.Length - 1].StartingHeight)
        {
            for (byte i = 0; i < Terrains.Length - 1; ++i, id += (byte)Terrains[i].Sprites.Length) ;
            id += (byte)(Random.Range(0, Terrains[Terrains.Length - 1].Sprites.Length) + BottommostTerrainSprites.Length);
        }
        else
            for (byte i = 1; i < Terrains.Length; i++)
                if (map.GetHeight(mapCoords) >= Terrains[i - 1].StartingHeight && map.GetHeight(mapCoords) < Terrains[i].StartingHeight)
                {
                    for (byte j = 0; j < i - 1; ++j, id += (byte)Terrains[j].Sprites.Length) ;
                    id += (byte)(Random.Range(0, Terrains[i - 1].Sprites.Length) + BottommostTerrainSprites.Length);
                }

        map.HexSpriteID_Matrix[(int)mapCoords.y, (int)mapCoords.x] = id;
        return AllHexSprites[id];
    }

    Sprite ChooseHexRiverSprite(Transform spriteTransform, Vector2 mapCoords, Map map)
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
                    id = (byte)Random.Range(0, RiverStartSprites.Length);
                    angle = (short)(Mathf.Sign(river[1].x - mapCoords.x) * Vector2.Angle(Vector2.down, GetTransformPosFromMapCoords(river[1]) - GetTransformPosFromMapCoords(mapCoords)));
                }
                else if (index == river.Count - 1)
                {
                    id = (byte)(Random.Range(0, RiverEndSprites.Length) + RiverStartSprites.Length + RiverStraightSprites.Length + RiverTurnSprites.Length);
                    angle = (short)(Mathf.Sign(river[river.Count - 2].x - mapCoords.x) * Vector2.Angle(Vector2.down, GetTransformPosFromMapCoords(river[river.Count - 2]) - GetTransformPosFromMapCoords(mapCoords)));
                }
                else
                {
                    Vector2 prev = GetTransformPosFromMapCoords(river[index - 1]) - GetTransformPosFromMapCoords(mapCoords);
                    if ((short)Vector2.Angle(GetTransformPosFromMapCoords(river[index - 1]) - GetTransformPosFromMapCoords(mapCoords), GetTransformPosFromMapCoords(river[index + 1]) - GetTransformPosFromMapCoords(mapCoords)) > 150) //(==180, !=120)
                    {
                        id = (byte)(Random.Range(0, RiverStraightSprites.Length) + RiverStartSprites.Length);
                        angle = (short)(Mathf.Sign(river[index - 1].x - mapCoords.x) * Vector2.Angle(Vector2.down, prev));
                    }
                    else
                    {
                        id = (byte)(Random.Range(0, RiverTurnSprites.Length) + RiverStartSprites.Length + RiverStraightSprites.Length);

                        Vector2 next = GetTransformPosFromMapCoords(river[index + 1]) - GetTransformPosFromMapCoords(mapCoords);

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
            spriteTransform.Rotate(new Vector3(0, 0, angle));
            Sprite sprite = AllRiverSprites[id];

            return sprite;
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
        if (map.GetForest(mapCoords) * ForestDensity >= 1)
        {
            float gridStepX = HexSpriteSize.x / ForestGenGridSize;
            float gridStepY = HexSpriteSize.y / ForestGenGridSize;
            Vector2 gridOrigin = new Vector2(hex.Hex.transform.position.x - HexSpriteSize.x * 0.375f, hex.Hex.transform.position.y - HexSpriteSize.y * 0.5f);
            byte treesCount = (byte)(map.GetForest(mapCoords) * ForestDensity);

            while (true)
            {
                if (treesCount > ForestGenGridSize * ForestGenGridSize)
                {
                    for (float y = 0; y < HexSpriteSize.y; y += gridStepY)
                        for (float x = 0; x < HexSpriteSize.x; x += gridStepX)
                        {
                            Vector2 v = new Vector2(Random.value * gridStepX, Random.value * gridStepY);
                            GameObject tree = new GameObject();
                            hex.Trees.Add(tree);
                            tree.transform.position = new Vector2(gridOrigin.x + x + v.x, gridOrigin.y + y + v.y);
                            tree.AddComponent<SpriteRenderer>();
                            tree.GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";//
                            tree.GetComponent<SpriteRenderer>().sprite = TreeSprites[Random.Range(0, TreeSprites.Length)];
                            StartCoroutine(RenderHelper.FadeIn(tree.GetComponent<Renderer>(), FadeInTime));
                            --treesCount;
                        }
                }
                else
                {
                    Vector2 v = Random.insideUnitCircle;
                    v.x *= HexSpriteSize.x * 0.5f;
                    v.y *= HexSpriteSize.y * 0.5f;
                    GameObject tree = new GameObject();
                    hex.Trees.Add(tree);
                    tree.transform.position = new Vector2(hex.Hex.transform.position.x + v.x, hex.Hex.transform.position.y + v.y);
                    tree.AddComponent<SpriteRenderer>();
                    tree.GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";//
                    tree.GetComponent<SpriteRenderer>().sprite = TreeSprites[Random.Range(0, TreeSprites.Length)];
                    StartCoroutine(RenderHelper.FadeIn(tree.GetComponent<Renderer>(), FadeInTime));
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
    public void RenderWholeMap(Map map)
    {
        ushort size = (ushort)map.HeightMatrix.GetLength(0);
        RenderedHexes.Capacity = size * size;

        for (ushort y = 0; y < size; ++y)
            for (ushort x = 0; x < size; ++x)
            {
                // TODO Возможно стоит заменить ListType на Hex?
                ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapCoords(new Vector2(x, y)), Quaternion.identity) as GameObject, InSign = true };
                hex.Hex.GetComponent<HexData>().MapCoords = new Vector2(x, y);
                MakeHexGraphics(hex, new Vector2(y, x), map);
                RenderedHexes.Add(hex);
            }
    }

    public void RenderLocalMap()
    {
        RenderedBackground = Instantiate(Background, new Vector2(Background.GetComponent<SpriteRenderer>().sprite.bounds.extents.x * Background.transform.localScale.x - HexSpriteSize.x / 2 - LocalMapBackgroundOffset.x, Background.GetComponent<SpriteRenderer>().sprite.bounds.extents.y * Background.transform.localScale.y - HexSpriteSize.y / 2 - LocalMapBackgroundOffset.y), Quaternion.identity) as GameObject;
        RenderedBackground.GetComponent<SpriteRenderer>().sortingLayerName = "Background";

        RenderedHexes.Capacity = (int)(GetComponent<World>().LocalMapSize.y * GetComponent<World>().LocalMapSize.x);

        for (ushort y = 0; y < GetComponent<World>().LocalMapSize.y; ++y)
            for (ushort x = 0; x < GetComponent<World>().LocalMapSize.x; ++x)
            {
                // TODO Возможно стоит заменить ListType на Hex?
                ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapCoords(new Vector2(x, y)), Quaternion.identity) as GameObject, InSign = true };
                hex.Hex.GetComponent<HexData>().MapCoords = new Vector2(x, y);
                hex.Hex.GetComponent<SpriteRenderer>().sprite = TransparentSprite;
                RenderedHexes.Add(hex);
            }
    }

    public void DestroyBackgound()
    {
        Destroy(RenderedBackground);
    }

    /// <summary>
    /// Вычисляет координаты в сцене из координат на карте.
    /// </summary>
    /// <returns>Координаты в сцене.</returns>
    /// <param name="mapCoords">Координаты на карте.</param>
    public static Vector2 GetTransformPosFromMapCoords(Vector2 mapCoords)
    {
        return new Vector2(mapCoords.x * HexSpriteSize.x * 0.75f, mapCoords.y * HexSpriteSize.y + ((mapCoords.x % 2) != 0 ? 1 : 0) * HexSpriteSize.y * 0.5f);
    }

    /// <summary>
    /// Накладывает на хекс спрайт.
    /// </summary>
    /// <param name="mapCoords">Координаты хекса.</param>
    /// <param name="highlightHexSprite">Спрайт.</param>
    public void HighlightHex(Vector2 mapCoords, Sprite highlightHexSprite)
    {
        RenderedObjects.Add(Instantiate(InteractableHex, GetTransformPosFromMapCoords(mapCoords), Quaternion.identity) as GameObject);
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sprite = highlightHexSprite;
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeHighlights";

        RenderedObjects[RenderedObjects.Count - 1].GetComponent<HexData>().MapCoords = mapCoords;//TODO временно
    }
}
