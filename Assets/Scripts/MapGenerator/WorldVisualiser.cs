using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldVisualiser : MonoBehaviour
{
    [System.Serializable]
    public class Terrain
    {
        public float StartingHeight;
        public Sprite Sprite;
    }

    public GameObject Hex;
    public GameObject InteractableHex;
    public GameObject Background;
    public Vector2 LocalMapBackgroundOffset;
    public Sprite BottommostTerrainSprite;
    public Terrain[] Terrains;
    public Sprite River;
    public GameObject Tree;
    public byte ForestDensity;
    public byte ForestGenGridSize;
    public float FadeInSpeed;
    public float FadeSpeed;
    public Sprite TransparentSprite;
    public Sprite BlueHexSprite;
    Vector2 HexSpriteSize;

    class ListType
    {
        public GameObject Hex;
        public bool InSign;
        public List<GameObject> Trees = new List<GameObject>();
    }

    List<ListType> RenderedHexes = new List<ListType>();
    //Map Map;
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
        //Assert
        Debug.Assert(BottommostTerrainSprite.bounds.size.x == River.bounds.size.x && River.bounds.size.x == BlueHexSprite.bounds.size.x && BottommostTerrainSprite.bounds.size.y == River.bounds.size.y && River.bounds.size.y == BlueHexSprite.bounds.size.y);
        for (byte i = 1; i < Terrains.Length; i++)
        {
            Debug.Assert(Terrains[i - 1].StartingHeight < Terrains[i].StartingHeight);
            Debug.Assert(Terrains[i - 1].Sprite.bounds.size.x == Terrains[i].Sprite.bounds.size.x && Terrains[i - 1].Sprite.bounds.size.y == Terrains[i].Sprite.bounds.size.y);
        }
        //--

        HexSpriteSize.x = BottommostTerrainSprite.bounds.size.x;
        HexSpriteSize.y = BottommostTerrainSprite.bounds.size.y;
    }

    /// <summary>
    /// Уничтожает все хексы.
    /// </summary>
    public void DestroyAllHexes()
    {
        RenderedHexes.ForEach(hex =>
        {
            hex.Trees.ForEach(tree => Destroy(tree));
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
                RenderedHexes[i].Trees.ForEach(tree => StartCoroutine(FadeAndDestroy(tree)));
                StartCoroutine(FadeAndDestroy(RenderedHexes[i].Hex));
                RenderedHexes.RemoveAt(i);
                i--;
            }
    }

    /// <summary>
    /// Постепенно отображает объект (корутина).
    /// </summary>
    /// <returns>(Корутина).</returns>
    /// <param name="obj">Объект для отображения.</param>
    IEnumerator FadeIn(GameObject obj)
    {
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        Color cbuf = renderer.material.color;
        cbuf.a = 0;
        renderer.material.color = cbuf;

        while (renderer != null && renderer.material.color.a < 1)
        {
            Color buf = renderer.material.color;
            buf.a += FadeInSpeed;
            renderer.material.color = buf;
            yield return null;
        }
    }

    /// <summary>
    /// Постепенно скрывает, затем уничтожает объект (корутина).
    /// </summary>
    /// <returns>(Корутина).</returns>
    /// <param name="obj">Объект к уничтожению.</param>
    IEnumerator FadeAndDestroy(GameObject obj)
    {
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();

        while (renderer.material.color.a > 0)
        {
            Color buf = renderer.material.color;
            buf.a -= FadeSpeed;
            renderer.material.color = buf;
            yield return null;
        }
        Destroy(obj);
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
            Quaternion rot = new Quaternion();
            ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapCoords(mapCoords), rot) as GameObject, InSign = true };
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
        ChooseHexSprite(hex.Hex, mapCoords, map);
        hex.Hex.GetComponent<SpriteRenderer>().sortingLayerName = "Landscape";//
        StartCoroutine(FadeIn(hex.Hex));
        MakeHexForest(hex, mapCoords, map);
    }

    /// <summary>
    /// Выбирает спрайт хекса.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    void ChooseHexSprite(GameObject hex, Vector2 mapCoords, Map map) //UNDONE
    {
        if (map.RiverMatrix[(int)mapCoords.y, (int)mapCoords.x])
            hex.GetComponent<SpriteRenderer>().sprite = River;
        else
        {
            if (map.HeightMatrix[(int)mapCoords.y, (int)mapCoords.x] < Terrains[0].StartingHeight)
            {
                hex.GetComponent<SpriteRenderer>().sprite = BottommostTerrainSprite;
                return;
            }
            for (byte i = 1; i < Terrains.Length; i++)
                if (map.HeightMatrix[(int)mapCoords.y, (int)mapCoords.x] >= Terrains[i - 1].StartingHeight && map.HeightMatrix[(int)mapCoords.y, (int)mapCoords.x] < Terrains[i].StartingHeight)
                {
                    hex.GetComponent<SpriteRenderer>().sprite = Terrains[i - 1].Sprite;
                    return;
                }
            if (map.HeightMatrix[(int)mapCoords.y, (int)mapCoords.x] >= Terrains[Terrains.Length - 1].StartingHeight)
                hex.GetComponent<SpriteRenderer>().sprite = Terrains[Terrains.Length - 1].Sprite;
        }
    }

    /// <summary>
    /// Создаёт лес на хексе.
    /// </summary>
    /// <param name="hex">Хекс.</param>
    /// <param name="mapCoords">Координаты в матрице.</param>
    void MakeHexForest(ListType hex, Vector2 mapCoords, Map map) //TODO (WIP)
    {
        if (map.ForestMatrix[(int)mapCoords.y, (int)mapCoords.x] * ForestDensity >= 1)
        {
            Quaternion rot = new Quaternion();
            float gridStepX = HexSpriteSize.x / ForestGenGridSize;
            float gridStepY = HexSpriteSize.y / ForestGenGridSize;
            Vector2 gridOrigin = new Vector2(hex.Hex.transform.position.x - HexSpriteSize.x * 0.375f, hex.Hex.transform.position.y - HexSpriteSize.y * 0.5f);
            byte treesCount = (byte)(map.ForestMatrix[(int)mapCoords.y, (int)mapCoords.x] * ForestDensity);

            while (true)
            {
                if (treesCount > ForestGenGridSize * ForestGenGridSize)
                {
                    for (float y = 0; y < HexSpriteSize.y; y += gridStepY)
                        for (float x = 0; x < HexSpriteSize.x; x += gridStepX)
                        {
                            Vector2 v = new Vector2(Random.value * gridStepX, Random.value * gridStepY);
                            hex.Trees.Add(Instantiate(Tree, new Vector2(gridOrigin.x + x + v.x, gridOrigin.y + y + v.y), rot) as GameObject);
                            hex.Trees[hex.Trees.Count - 1].GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";//
                            StartCoroutine(FadeIn(hex.Trees[hex.Trees.Count - 1]));
                            treesCount--;
                        }
                }
                else
                {
                    Vector2 v = Random.insideUnitCircle;
                    v.x *= HexSpriteSize.x * 0.5f;
                    v.y *= HexSpriteSize.y * 0.5f;
                    hex.Trees.Add(Instantiate(Tree, new Vector2(hex.Hex.transform.position.x + v.x, hex.Hex.transform.position.y + v.y), rot) as GameObject);
                    hex.Trees[hex.Trees.Count - 1].GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeObjects";//
                    StartCoroutine(FadeIn(hex.Trees[hex.Trees.Count - 1]));
                    treesCount--;
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
        Quaternion rot = new Quaternion();

        for (ushort y = 0; y < size; ++y)
            for (ushort x = 0; x < size; ++x)
            {
                // TODO Возможно стоит заменить ListType на Hex?
                ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapCoords(new Vector2(x, y)), rot) as GameObject, InSign = true };
                hex.Hex.GetComponent<HexData>().MapCoords = new Vector2(x, y);
                MakeHexGraphics(hex, new Vector2(y, x), map);
                RenderedHexes.Add(hex);
            }
    }

    public void RenderLocalMap()
    {
        RenderedBackground = Instantiate(Background, new Vector2(Background.GetComponent<SpriteRenderer>().sprite.bounds.extents.x - HexSpriteSize.x / 2 - LocalMapBackgroundOffset.x, Background.GetComponent<SpriteRenderer>().sprite.bounds.extents.y - HexSpriteSize.y / 2 - LocalMapBackgroundOffset.y), new Quaternion()) as GameObject;
        RenderedBackground.GetComponent<SpriteRenderer>().sortingLayerName = "Background";

        RenderedHexes.Capacity = (int)(GetComponent<World>().LocalMapSize.y * GetComponent<World>().LocalMapSize.x);
        Quaternion rot = new Quaternion();

        for (ushort y = 0; y < GetComponent<World>().LocalMapSize.y; ++y)
            for (ushort x = 0; x < GetComponent<World>().LocalMapSize.x; ++x)
            {
                // TODO Возможно стоит заменить ListType на Hex?
                ListType hex = new ListType { Hex = Instantiate(Hex, GetTransformPosFromMapCoords(new Vector2(x, y)), rot) as GameObject, InSign = true };
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
    public Vector2 GetTransformPosFromMapCoords(Vector2 mapCoords)
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
        RenderedObjects.Add(Instantiate(InteractableHex, GetTransformPosFromMapCoords(mapCoords), new Quaternion(0, 0, 0, 0)) as GameObject);
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sprite = highlightHexSprite;
        RenderedObjects[RenderedObjects.Count - 1].GetComponent<SpriteRenderer>().sortingLayerName = "LandscapeHighlights";

        RenderedObjects[RenderedObjects.Count - 1].GetComponent<HexData>().MapCoords = mapCoords;//TODO временно
    }
}