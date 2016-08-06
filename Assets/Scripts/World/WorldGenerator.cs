using UnityEngine;
using System.Collections.Generic;

public static class WorldGenerator
{
    [System.Serializable]
    public class LocalTerrainSettings
    {
        [System.Serializable]
        public class Terrain
        {
            [SerializeField]
            TerrainType TerrainType_;
            public TerrainType TerrainType { get { return TerrainType_; } set { TerrainType_ = value; } }
            [SerializeField]
            float StartingHeight_;
            public float StartingHeight { get { return StartingHeight_; } set { StartingHeight_ = value; } }
            [SerializeField]
            float PlantProbability_;
            public float PlantProbability { get { return PlantProbability_; } }
            /*[SerializeField]
        float BushesForestValue_;
        public float BushesForestValue { get { return BushesForestValue_; } private set { BushesForestValue_ = value; } }*/
            [SerializeField]
            Entity[] Trees_;
            public Entity[] Trees { get { return Trees_; } set { Trees_ = value; } }
        }

        [SerializeField]
        Terrain BottommostTerrain_;
        public Terrain BottommostTerrain { get { return BottommostTerrain_; } }
        [SerializeField]
        Terrain[] Terrains_;
        public Terrain[] Terrains { get { return Terrains_; } }

        public Terrain GetTerrain(TerrainType terrain)
        {
            if (BottommostTerrain_.TerrainType == terrain)
                return BottommostTerrain_;
            foreach (Terrain t in Terrains_)
                if (t.TerrainType == terrain)
                    return t;
            throw new System.ArgumentException("TerrainType not found", "terrain");
        }
    }

    [System.Serializable]
    public class GlobalTerrainSettings
    {
        [System.Serializable]
        public class Terrain
        {
            [SerializeField]
            TerrainType TerrainType_;
            public TerrainType TerrainType { get { return TerrainType_; } set { TerrainType_ = value; } }
            [SerializeField]
            float StartingHeight_;
            public float StartingHeight { get { return StartingHeight_; } set { StartingHeight_ = value; } }
        }

        [System.Serializable]
        public class RiversSettings
        {
            [SerializeField]
            float Height_;
            public float Height { get { return Height_; } }//Коэффициент высоты реки относительно средней высоты
            [SerializeField]
            byte Count_;
            public byte Count { get { return Count_; } }
            [SerializeField]
            ushort MinimumLength_;
            public ushort MinimumLength { get { return MinimumLength_; } }
            [SerializeField]
            byte Attempts_;
            public byte Attempts { get { return Attempts_; } } //Количество попыток построения реки из одного хекса
            [SerializeField]
            float FlowHeightKoef_;
            public float FlowHeightKoef { get { return FlowHeightKoef_; } } //Насколько реалистично река распространяется относительно высоты (1 - самое реалистичное)
        }

        [System.Serializable]
        public class ClustersSettings
        {
            [SerializeField]
            byte Count_;
            public byte Count { get { return Count_; } }
            [SerializeField]
            byte Size_;
            public byte Size { get { return Size_; } }
        }

        [System.Serializable]
        public class RoadsSettings
        {
            [SerializeField]
            float RoadMergeMultiplier_;
            public float RoadMergeMultiplier { get { return RoadMergeMultiplier_; } }
            [SerializeField]
            float GoingAlongRiverMultiplier_;
            public float GoingAlongRiverMultiplier { get { return GoingAlongRiverMultiplier_; } }
        }

        [SerializeField]
        TerrainType BottommostTerrain_;
        public TerrainType BottommostTerrain { get { return BottommostTerrain_; } }
        [SerializeField]
        Terrain[] Terrains_;
        public Terrain[] Terrains { get { return Terrains_; } }
        [SerializeField]
        RiversSettings RiversParam_;
        public RiversSettings RiversParam { get { return RiversParam_; } }
        [SerializeField]
        RoadsSettings RoadsParam_;
        public RoadsSettings RoadsParam { get { return RoadsParam_; } }
        [SerializeField]
        ClustersSettings ClustersParam_;
        public ClustersSettings ClustersParam { get { return ClustersParam_; } }
    }

    public struct HeighmapNeighboring
    {
        public float[] Left;
        public float[] Top;
        public float[] Right;
        public float[] Bottom;
    }

    static Stack<U16Vec2> RiverStack; //Стек для постройки реки

    public static void CreateLocalMap(ref LocalMap map, LocalTerrainSettings settings, HeighmapNeighboring hmNb, float landscapeRoughness, float forest) //TODO Временно
    {
        float?[,] buf = new float?[map.Height, map.Width];

        WorldGenerator.CreateHeightmap(ref buf, landscapeRoughness, hmNb);

        float[,] buf2 = new float[map.Height, map.Width];
        for (ushort y = 0; y < map.Height; ++y)
            for (ushort x = 0; x < map.Width; ++x)
            {
                buf2[y, x] = buf[y, x].Value;
                buf[y, x] = null;
            }
        WorldGenerator.CreateTerrainmap(ref map.TerrainMatrix, buf2, settings);
        WorldGenerator.CreateVegetation(ref map, settings, forest);
    }

    public static void CreateChunk(ref Chunk map, GlobalTerrainSettings settings, HeighmapNeighboring hmNbHeight, float landscapeRoughness, HeighmapNeighboring hmNbForest, float forestRoughness, float forestDensity)
    {
        float?[,] buf = new float?[map.Height, map.Width];
        WorldGenerator.CreateHeightmap(ref buf, landscapeRoughness, hmNbHeight);
        for (ushort y = 0; y < map.Height; ++y)
            for (ushort x = 0; x < map.Width; ++x)
            {
                map.HeightMatrix[y, x] = Mathf.Clamp(buf[y, x].Value, 0, Mathf.Abs(buf[y, x].Value));
                buf[y, x] = null;
            }

        WorldGenerator.CreateHeightmap(ref buf, forestRoughness, hmNbForest);
        for (ushort y = 0; y < map.Height; ++y)
            for (ushort x = 0; x < map.Width; ++x)
            {
                buf[y, x] = buf[y, x].Value * forestDensity;
                map.ForestMatrix[y, x] = Mathf.Clamp(buf[y, x].Value, 0, Mathf.Abs(buf[y, x].Value));
            }
        map.Rivers = WorldGenerator.CreateRivers(map.HeightMatrix, ref map.TerrainMatrix, settings.RiversParam);
        map.Clusters = WorldGenerator.CreateClusters(ref map, settings.ClustersParam);
        map.Roads = WorldGenerator.CreateRoads(map.HeightMatrix, ref map.TerrainMatrix, map.Clusters, settings.RoadsParam);
        WorldGenerator.CreateTerrainmap(ref map.TerrainMatrix, map.HeightMatrix, settings);
    }

    public static void CreateHeightmap(ref float?[,] matrix, float roughness, HeighmapNeighboring hmNb)
    {
        ushort height = (ushort)matrix.GetLength(0);
        ushort width = (ushort)matrix.GetLength(1);

        //-- Assert
        //TODO C# 6.0
        System.Diagnostics.Debug.Assert((hmNb.Left == null || hmNb.Left.Length == height) && (hmNb.Right == null || hmNb.Right.Length == height));
        System.Diagnostics.Debug.Assert((hmNb.Top == null || hmNb.Top.Length == width) && (hmNb.Bottom == null || hmNb.Bottom.Length == width));
        //--

        //Задаём начальные значения по углам
        //TODO C# 6.0
        if (!matrix[0, 0].HasValue)
            matrix[0, 0] = ((hmNb.Left == null ? (hmNb.Bottom == null ? Random.Range(-roughness, roughness) : hmNb.Bottom[0]) : hmNb.Left[0]) + (hmNb.Bottom == null ? (hmNb.Left == null ? Random.Range(-roughness, roughness) : hmNb.Left[0]) : hmNb.Bottom[0])) * 0.5f;
        if (!matrix[0, width - 1].HasValue)
            matrix[0, width - 1] = ((hmNb.Bottom == null ? (hmNb.Right == null ? Random.Range(-roughness, roughness) : hmNb.Right[0]) : hmNb.Bottom[width - 1]) + (hmNb.Right == null ? (hmNb.Bottom == null ? Random.Range(-roughness, roughness) : hmNb.Bottom[width - 1]) : hmNb.Right[0])) * 0.5f;
        if (!matrix[height - 1, 0].HasValue)
            matrix[height - 1, 0] = ((hmNb.Top == null ? (hmNb.Left == null ? Random.Range(-roughness, roughness) : hmNb.Left[height - 1]) : hmNb.Top[0]) + (hmNb.Left == null ? (hmNb.Top == null ? Random.Range(-roughness, roughness) : hmNb.Top[0]) : hmNb.Left[height - 1])) * 0.5f;
        if (!matrix[height - 1, width - 1].HasValue)
            matrix[height - 1, width - 1] = ((hmNb.Right == null ? (hmNb.Top == null ? Random.Range(-roughness, roughness) : hmNb.Top[width - 1]) : hmNb.Right[height - 1]) + (hmNb.Top == null ? (hmNb.Right == null ? Random.Range(-roughness, roughness) : hmNb.Right[height - 1]) : hmNb.Top[width - 1])) * 0.5f;

        float randRangeCoef = roughness / (height + width);

        if (hmNb.Left != null)
            for (ushort i = 1; i < height - 1; ++i)
                matrix[i, 0] = hmNb.Left[i] + Random.Range(-randRangeCoef, randRangeCoef);
        if (hmNb.Right != null)
            for (ushort i = 1; i < height - 1; ++i)
                matrix[i, width - 1] = hmNb.Right[i] + Random.Range(-randRangeCoef, randRangeCoef);
        if (hmNb.Bottom != null)
            for (ushort i = 1; i < width - 1; ++i)
                matrix[0, i] = hmNb.Bottom[i] + Random.Range(-randRangeCoef, randRangeCoef);
        if (hmNb.Top != null)
            for (ushort i = 1; i < width - 1; ++i)
                matrix[height - 1, i] = hmNb.Top[i] + Random.Range(-randRangeCoef, randRangeCoef);

        ushort len = (ushort)Mathf.Max(Mathf.NextPowerOfTwo(height), Mathf.NextPowerOfTwo(width));
        ushort[] yOffsets = new ushort[len + 2];//////////////////////////////temp
        ushort[] xOffsets = new ushort[len + 2];/////////////////////////////////temp
        yOffsets[0] = (ushort)(height - 1);
        xOffsets[0] = (ushort)(width - 1);
        ushort[] yOffsetsBuf = new ushort[len + 2];
        ushort[] xOffsetsBuf = new ushort[len + 2];

        ushort count = 1;
        while (count != len)
        {
            System.Array.Copy(yOffsets, yOffsetsBuf, len + 2);//////////////////////////////temp +2
            System.Array.Copy(xOffsets, xOffsetsBuf, len + 2);//////////////////////////////temp +2
            for (ushort i = 0; i < count; ++i)
            {
                ushort q = (ushort)(yOffsetsBuf[i] + 1 >> 1);
                byte nr = (byte)((yOffsetsBuf[i] + 1) & 1 ^ 1);
                yOffsets[i << 1] = q;
                yOffsets[(i << 1) + 1] = (ushort)(q - nr);

                q = (ushort)(xOffsetsBuf[i] + 1 >> 1);
                nr = (byte)((xOffsetsBuf[i] + 1) & 1 ^ 1);
                xOffsets[i << 1] = q;
                xOffsets[(i << 1) + 1] = (ushort)(q - nr);
            }

            count <<= 1;

            //Центры
            for (ushort y = 0, yPoint = yOffsets[0]; y < count; y += 2)
            {
                for (ushort x = 0, xPoint = xOffsets[0]; x < count; x += 2)
                {
                    if (!matrix[yPoint, xPoint].HasValue)
                    {
                        float randRange = (yOffsets[y] + yOffsets[y + 1] + xOffsets[x] + xOffsets[x + 1]) * randRangeCoef;
                        matrix[yPoint, xPoint] = (matrix[yPoint - yOffsets[y], xPoint - xOffsets[x]] + matrix[yPoint + yOffsets[y + 1], xPoint - xOffsets[x]] + matrix[yPoint - yOffsets[y], xPoint + xOffsets[x + 1]] + matrix[yPoint + yOffsets[y + 1], xPoint + xOffsets[x + 1]]) * 0.25f + Random.Range(-randRange, randRange);
                    }
                    xPoint += (ushort)(xOffsets[x + 1] + xOffsets[x + 2]);
                }
                yPoint += (ushort)(yOffsets[y + 1] + yOffsets[y + 2]);
            }


            //Вертикальные границы
            for (ushort y = 0, yPoint = yOffsets[0]; y < count; y += 2)
            {
                if (!matrix[yPoint, 0].HasValue)
                {
                    float randRange = (yOffsets[y] + yOffsets[y + 1] + xOffsets[0]) * randRangeCoef;
                    matrix[yPoint, 0] = (matrix[yPoint - yOffsets[y], 0] + matrix[yPoint + yOffsets[y + 1], 0] + matrix[yPoint, xOffsets[0]]) * 0.33333f + Random.Range(-randRange, randRange);

                }
                if (!matrix[yPoint, width - 1].HasValue)
                {
                    float randRange = (yOffsets[y] + yOffsets[y + 1] + xOffsets[count - 1]) * randRangeCoef;
                    matrix[yPoint, width - 1] = (matrix[yPoint - yOffsets[y], width - 1] + matrix[yPoint + yOffsets[y + 1], width - 1] + matrix[yPoint, width - 1 - xOffsets[count - 1]]) * 0.33333f + Random.Range(-randRange, randRange);
                }
                yPoint += (ushort)(yOffsets[y + 1] + yOffsets[y + 2]);
            }


            //Горизонтальные границы
            for (ushort x = 0, xPoint = xOffsets[0]; x < count; x += 2)
            {
                if (!matrix[0, xPoint].HasValue)
                {
                    float randRange = (xOffsets[x] + xOffsets[x + 1] + yOffsets[0]) * randRangeCoef;
                    matrix[0, xPoint] = (matrix[0, xPoint - xOffsets[x]] + matrix[0, xPoint + xOffsets[x + 1]] + matrix[yOffsets[0], xPoint]) * 0.33333f + Random.Range(-randRange, randRange);

                }
                if (!matrix[height - 1, xPoint].HasValue)
                {
                    float randRange = (xOffsets[x] + xOffsets[x + 1] + yOffsets[count - 1]) * randRangeCoef;
                    matrix[height - 1, xPoint] = (matrix[height - 1, xPoint - xOffsets[x]] + matrix[height - 1, xPoint + xOffsets[x + 1]] + matrix[height - 1 - yOffsets[count - 1], xPoint]) * 0.33333f + Random.Range(-randRange, randRange);
                }
                xPoint += (ushort)(xOffsets[x + 1] + xOffsets[x + 2]);
            }

            //Первый столбец
            for (ushort y = 0, yPoint = yOffsets[0]; y < count - 2; y += 2)
            {
                if (!matrix[yPoint + yOffsets[y + 1], xOffsets[0]].HasValue)
                {
                    float randRange = (yOffsets[y + 1] + yOffsets[y + 2] + xOffsets[0] + xOffsets[1]) * randRangeCoef;
                    matrix[yPoint + yOffsets[y + 1], xOffsets[0]] = (matrix[yPoint, xOffsets[0]] + matrix[yPoint + yOffsets[y + 1] + yOffsets[y + 2], xOffsets[0]] + matrix[yPoint + yOffsets[y + 1], 0] + matrix[yPoint + yOffsets[y + 1], xOffsets[0] + xOffsets[1]]) * 0.25f + Random.Range(-randRange, randRange);
                }
                yPoint += (ushort)(yOffsets[y + 1] + yOffsets[y + 2]);
            }

            //Первая строка
            for (ushort x = 0, xPoint = xOffsets[0]; x < count - 2; x += 2)
            {
                if (!matrix[yOffsets[0], xPoint + xOffsets[x + 1]].HasValue)
                {
                    float randRange = (xOffsets[x + 1] + xOffsets[x + 2] + yOffsets[0] + yOffsets[1]) * randRangeCoef;
                    matrix[yOffsets[0], xPoint + xOffsets[x + 1]] = (matrix[yOffsets[0], xPoint] + matrix[yOffsets[0], xPoint + xOffsets[x + 1] + xOffsets[x + 2]] + matrix[0, xPoint + xOffsets[x + 1]] + matrix[yOffsets[0] + yOffsets[1], xPoint + xOffsets[x + 1]]) * 0.25f + Random.Range(-randRange, randRange);
                }
                xPoint += (ushort)(xOffsets[x + 1] + xOffsets[x + 2]);
            }

            //Остальное
            for (ushort y = 1, yPoint = (ushort)(yOffsets[0] + yOffsets[1]); y < count - 2; y += 2)// -1 ?
            {
                for (ushort x = 1, xPoint = (ushort)(xOffsets[0] + xOffsets[1]); x < count - 2; x += 2)// -1 ?
                {
                    if (!matrix[yPoint + yOffsets[y + 1], xPoint].HasValue)
                    {
                        float randRange = (yOffsets[y + 1] + yOffsets[y + 2] + xOffsets[x] + xOffsets[x + 1]) * randRangeCoef;
                        matrix[yPoint + yOffsets[y + 1], xPoint] = (matrix[yPoint + yOffsets[y + 1], xPoint - xOffsets[x]] + matrix[yPoint, xPoint] + matrix[yPoint + yOffsets[y + 1], xPoint + xOffsets[x + 1]] + matrix[yPoint + yOffsets[y + 1] + yOffsets[y + 2], xPoint]) * 0.25f + Random.Range(-randRange, randRange);
                    }
                    if (!matrix[yPoint, xPoint + xOffsets[x + 1]].HasValue)
                    {
                        float randRange = (yOffsets[y] + yOffsets[y + 1] + xOffsets[x + 1] + xOffsets[x + 2]) * randRangeCoef;
                        matrix[yPoint, xPoint + xOffsets[x + 1]] = (matrix[yPoint - yOffsets[y], xPoint + xOffsets[x + 1]] + matrix[yPoint, xPoint] + matrix[yPoint, xPoint + xOffsets[x + 1] + xOffsets[x + 2]] + matrix[yPoint + yOffsets[y + 1], xPoint + xOffsets[x + 1]]) * 0.25f + Random.Range(-randRange, randRange);
                    }
                    xPoint += (ushort)(xOffsets[x + 1] + xOffsets[x + 2]);
                }
                yPoint += (ushort)(yOffsets[y + 1] + yOffsets[y + 2]);
            }
        }
    }

    public static void CreateTerrainmap(ref TerrainType[,] terrainmap, float[,] matrix, LocalTerrainSettings terrainParam)
    {
        ushort height = (ushort)terrainmap.GetLength(0);
        ushort width = (ushort)terrainmap.GetLength(1);

        for (ushort y = 0; y < height; ++y)
            for (ushort x = 0; x < width; ++x)
                if (matrix[y, x] < terrainParam.Terrains[0].StartingHeight)
                    terrainmap[y, x] |= terrainParam.BottommostTerrain.TerrainType;
                else if (matrix[y, x] >= terrainParam.Terrains[terrainParam.Terrains.Length - 1].StartingHeight)
                    terrainmap[y, x] |= terrainParam.Terrains[terrainParam.Terrains.Length - 1].TerrainType;
                else
                    for (byte i = 1; i < terrainParam.Terrains.Length; ++i)
                        if (matrix[y, x] < terrainParam.Terrains[i].StartingHeight)
                        {
                            terrainmap[y, x] |= terrainParam.Terrains[i - 1].TerrainType;
                            break;
                        }
    }

    public static void CreateTerrainmap(ref TerrainType[,] terrainmap, float[,] matrix, GlobalTerrainSettings terrainParam)
    {
        ushort height = (ushort)terrainmap.GetLength(0);
        ushort width = (ushort)terrainmap.GetLength(1);

        for (ushort y = 0; y < height; ++y)
            for (ushort x = 0; x < width; ++x)
                if (matrix[y, x] < terrainParam.Terrains[0].StartingHeight)
                    terrainmap[y, x] |= terrainParam.BottommostTerrain;
                else if (matrix[y, x] >= terrainParam.Terrains[terrainParam.Terrains.Length - 1].StartingHeight)
                    terrainmap[y, x] |= terrainParam.Terrains[terrainParam.Terrains.Length - 1].TerrainType;
                else
                    for (byte i = 1; i < terrainParam.Terrains.Length; ++i)
                        if (matrix[y, x] < terrainParam.Terrains[i].StartingHeight)
                        {
                            terrainmap[y, x] |= terrainParam.Terrains[i - 1].TerrainType;
                            break;
                        }
    }

    public static void CreateVegetation(ref LocalMap map, LocalTerrainSettings terrainParam, float vegetationValue)
    {
        float?[,] forestMatrix = new float?[map.Height, map.Width];
        HeighmapNeighboring hmNb;
        hmNb.Left = new float[map.Height];
        hmNb.Top = new float[map.Width];
        hmNb.Right = new float[map.Height];
        hmNb.Bottom = new float[map.Width];
        for (ushort i = 0; i < map.Height; ++i)
            hmNb.Right[i] = hmNb.Left[i] = vegetationValue;
        for (ushort i = 0; i < map.Width; ++i)
            hmNb.Bottom[i] = hmNb.Top[i] = vegetationValue;
        CreateHeightmap(ref forestMatrix, 1, hmNb);//TODO
        for (ushort y = 0; y < map.Height; ++y)
            for (ushort x = 0; x < map.Width; ++x)
            {
                LocalTerrainSettings.Terrain terrain = terrainParam.GetTerrain(map.TerrainMatrix[y, x]);
                if (terrain.PlantProbability > 0 && forestMatrix[y, x] > 0 && Random.value < terrain.PlantProbability * forestMatrix[y, x])
                {
                    Entity plant = GameObject.Instantiate(terrain.Trees[Random.Range(0, terrain.Trees.Length)]);
                    plant.Pos = new U16Vec2(x, y);
                    map.AddObject(plant);
                    /*if (map.ForestMatrix[pos.Y, pos.X] < LocalMapParam.BushesForestValue)
                        plant.GetComponent<SpriteRenderer>().sprite = LocalMapParam.BushSprites[Random.Range(0, LocalMapParam.BushSprites.Length)];
                    else
                        plant.GetComponent<SpriteRenderer>().sprite = LocalMapParam.TreeSprites[Random.Range(0, LocalMapParam.TreeSprites.Length)];*/
                }
            }
    }

    public static void MakeEqualHeightLine(float[,] matrix, U16Vec2[] vertices, float height)
    {
        foreach (U16Vec2 v in vertices)
            if (v.X >= matrix.GetLength(1) || v.Y >= matrix.GetLength(0))
                throw new System.ArgumentOutOfRangeException("vertices", v.ToString(), "Vector is out of matrix length.");

        for (byte i = 0; i < vertices.Length - 1; ++i)
        {
            S32Vec2 v = vertices[i];
            while (v != vertices[i + 1])
            {
                matrix[v.Y, v.X] = height;
                v.Y += Mathf.Clamp(vertices[i + 1].Y - v.Y, -1, 1);
                v.X += Mathf.Clamp(vertices[i + 1].X - v.X, -1, 1);
            }
        }
        matrix[vertices[vertices.Length - 1].Y, vertices[vertices.Length - 1].X] = height;
    }

    /// <summary>
    /// Создаёт реки.
    /// </summary>
    /// <returns>Список рек.</returns>
    /// <param name="heightMatrix">Карта высот.</param>
    /// <param name="riverMatrix">[out] Карта рек.</param>
    public static List<List<U16Vec2>> CreateRivers(float[,] heightMap, ref TerrainType[,] terrainMap, GlobalTerrainSettings.RiversSettings riversParam)
    {
        RiverStack = new Stack<U16Vec2>();

        ushort height = (ushort)heightMap.GetLength(0);
        ushort width = (ushort)heightMap.GetLength(1);

        double avg = 0;
        foreach (float h in heightMap)
            avg += h;
        avg /= height * width;
        float minRiverHeight = (float)avg * riversParam.Height;

        List<List<U16Vec2>> rivers = new List<List<U16Vec2>>();

        for (byte i = 0; i < riversParam.Count; ++i)
        {
            bool riverCreated = false;
            for (ushort y = 1; y < height - 1 && !riverCreated; ++y) //TODO
                for (ushort x = 1; x < width - 1 && !riverCreated; ++x) //TODO
                    if (heightMap[y, x] > minRiverHeight && (terrainMap[y, x] & TerrainType.RIVER) == TerrainType.NONE && RiverNeighbours(new U16Vec2(x, y), terrainMap) == 0) //Проверяем, можно ли нам начать создание реки с этого хекса
                        for (byte k = 0; k < riversParam.Attempts && !riverCreated; ++k)
                        {
                            DirectRiver(new U16Vec2(x, y), heightMap, terrainMap, riversParam.FlowHeightKoef); //Запускаем рекурсию
                            if (RiverStack.Count >= riversParam.MinimumLength)
                            { //Если река получилась больше необходим длины, то помечаем ячейки матрицы, иначе пробуем ещё раз 
                                foreach (U16Vec2 hex in RiverStack)
                                    terrainMap[hex.Y, hex.X] |= TerrainType.RIVER;
                                riverCreated = true;
                                rivers.Add(new List<U16Vec2>(RiverStack));
                                rivers[rivers.Count - 1].Reverse();
                            }
                            RiverStack.Clear();
                        }
        }
        RiverStack = null;
        return rivers;
    }

    /// <summary>
    /// Выбирает направление распространения реки.
    /// </summary>
    /// <param name="y">y координата.</param>
    /// <param name="x">x координата.</param>
    /// <param name="heightMatrix">Карта высот.</param>
    /// <param name="matrix">Карта рек.</param>
    static void DirectRiver(U16Vec2 pos, float[,] heightMatrix, TerrainType[,] terrainMatrix, float flowHeightKoef)
    {
        ushort height = (ushort)heightMatrix.GetLength(0);
        ushort width = (ushort)heightMatrix.GetLength(1);

        RiverStack.Push(pos);

        if (pos.Y > 0 && pos.Y < height - 1 && pos.X > 0 && pos.X < width - 1)
        {
            byte limiter = 0; //Переменная, контролирующая проверку всех направлений и выход из цикла, если ни одно не подходит
            bool dirFound = false;

            byte k = (byte)(pos.X & 1); //Учитываем чётность/нечётность ряда хексов
            do //TODO Провести рефакторинг.
            {
                switch (Random.Range(0, 7))//Выбираем случайное направление
                {
                    case (int)HexDirection.BOTTOM_LEFT:
                        if ((limiter & 1) == 0 && !RiverStack.Contains(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y - (k ^ 1)))) && heightMatrix[pos.Y - (k ^ 1), pos.X - 1] * flowHeightKoef <= heightMatrix[pos.Y, pos.X] && (terrainMatrix[pos.Y - (k ^ 1), pos.X - 1] & TerrainType.RIVER) == TerrainType.NONE && RiverNeighbours(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y - (k ^ 1))), terrainMatrix) < 2)
                        {
                            DirectRiver(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y - (k ^ 1))), heightMatrix, terrainMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter |= 1;
                        break;
                    case (int)HexDirection.TOP_LEFT:
                        if ((limiter & 2) == 0 && !RiverStack.Contains(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y + k))) && heightMatrix[pos.Y + k, pos.X - 1] * flowHeightKoef <= heightMatrix[pos.Y, pos.X] && (terrainMatrix[pos.Y + k, pos.X - 1] & TerrainType.RIVER) == TerrainType.NONE && RiverNeighbours(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y + k)), terrainMatrix) < 2)
                        {
                            DirectRiver(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y + k)), heightMatrix, terrainMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter |= 2;
                        break;
                    case (int)HexDirection.BOTTOM:
                        if ((limiter & 4) == 0 && !RiverStack.Contains(new U16Vec2(pos.X, (ushort)(pos.Y - 1))) && heightMatrix[pos.Y - 1, pos.X] * flowHeightKoef <= heightMatrix[pos.Y, pos.X] && (terrainMatrix[pos.Y - 1, pos.X] & TerrainType.RIVER) == TerrainType.NONE && RiverNeighbours(new U16Vec2(pos.X, (ushort)(pos.Y - 1)), terrainMatrix) < 2)
                        {
                            DirectRiver(new U16Vec2(pos.X, (ushort)(pos.Y - 1)), heightMatrix, terrainMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter |= 4;
                        break;
                    case (int)HexDirection.TOP:
                        if ((limiter & 8) == 0 && !RiverStack.Contains(new U16Vec2((ushort)pos.X, (ushort)(pos.Y + 1))) && heightMatrix[pos.Y + 1, pos.X] * flowHeightKoef <= heightMatrix[pos.Y, pos.X] && (terrainMatrix[pos.Y + 1, pos.X] & TerrainType.RIVER) == TerrainType.NONE && RiverNeighbours(new U16Vec2(pos.X, (ushort)(pos.Y + 1)), terrainMatrix) < 2)
                        {
                            DirectRiver(new U16Vec2(pos.X, (ushort)(pos.Y + 1)), heightMatrix, terrainMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter |= 8;
                        break;
                    case (int)HexDirection.BOTTOM_RIGHT:
                        if ((limiter & 16) == 0 && !RiverStack.Contains(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y - (k ^ 1)))) && heightMatrix[pos.Y - (k ^ 1), pos.X + 1] * flowHeightKoef <= heightMatrix[pos.Y, pos.X] && (terrainMatrix[pos.Y - (k ^ 1), pos.X + 1] & TerrainType.RIVER) == TerrainType.NONE && RiverNeighbours(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y - (k ^ 1))), terrainMatrix) < 2)
                        {
                            DirectRiver(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y - (k ^ 1))), heightMatrix, terrainMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter |= 16;
                        break;
                    case (int)HexDirection.TOP_RIGHT:
                        if ((limiter & 32) == 0 && !RiverStack.Contains(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y + k))) && heightMatrix[pos.Y + k, pos.X + 1] * flowHeightKoef <= heightMatrix[pos.Y, pos.X] && (terrainMatrix[pos.Y + k, pos.X + 1] & TerrainType.RIVER) == TerrainType.NONE && RiverNeighbours(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y + k)), terrainMatrix) < 2)
                        {
                            DirectRiver(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y + k)), heightMatrix, terrainMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter |= 32;
                        break;
                }
            }
            while (!dirFound && limiter != 63);
        }
    }

    /// <summary>
    /// Подсчитывает сколько рядом "рек".
    /// </summary>
    /// <returns>Число хексов вокруг, занятых рекой.</returns>
    /// <param name="y"> y координата.</param>
    /// <param name="x"> x координата.</param>
    /// <param name="matrix">Карта рек.</param>
    /// Функция подсчитывает количство соседних клеток, помеченных как "Река" или находящихся в "стеке реки"
    /// TODO Проверить работу стека реки
    static byte RiverNeighbours(U16Vec2 pos, TerrainType[,] terrainMatrix)
    {
        ushort height = (ushort)terrainMatrix.GetLength(0);
        ushort width = (ushort)terrainMatrix.GetLength(1);

        byte k = (byte)(pos.X & 1); //TODO Провести рефакторинг.

        byte riversCount = 0;
        if (pos.Y > 0 && pos.X > 0 && ((terrainMatrix[pos.Y - (k ^ 1), pos.X - 1] & TerrainType.RIVER) != TerrainType.NONE || RiverStack.Contains(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y - (k ^ 1))))))
            riversCount++;
        if (pos.X > 0 && pos.Y < height - 1 && ((terrainMatrix[pos.Y + k, pos.X - 1] & TerrainType.RIVER) != TerrainType.NONE || RiverStack.Contains(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y + k)))))
            riversCount++;
        if (pos.Y > 0 && ((terrainMatrix[pos.Y - 1, pos.X] & TerrainType.RIVER) != TerrainType.NONE || RiverStack.Contains(new U16Vec2(pos.X, (ushort)(pos.Y - 1)))))
            riversCount++;
        if (pos.Y < height - 1 && ((terrainMatrix[pos.Y + 1, pos.X] & TerrainType.RIVER) != TerrainType.NONE || RiverStack.Contains(new U16Vec2(pos.X, (ushort)(pos.Y + 1)))))
            riversCount++;
        if (pos.Y > 0 && pos.X < width - 1 && ((terrainMatrix[pos.Y - (k ^ 1), pos.X + 1] & TerrainType.RIVER) != TerrainType.NONE || RiverStack.Contains(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y - (k ^ 1))))))
            riversCount++;
        if (pos.X < width - 1 && pos.Y < height - 1 && ((terrainMatrix[pos.Y + k, pos.X + 1] & TerrainType.RIVER) != TerrainType.NONE || RiverStack.Contains(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y + k)))))
            riversCount++;

        return riversCount;
    }

    //UNDONE
    public static List<List<U16Vec2>> CreateClusters(ref Chunk map, GlobalTerrainSettings.ClustersSettings clustersParam)
    {
        ushort height = map.Height;
        ushort width = map.Width;

        List<List<U16Vec2>> clusters = new List<List<U16Vec2>>(clustersParam.Count);

        for (byte i = 0; i < clustersParam.Count; ++i)
        {
            U16Vec2 pos;
            pos.X = (ushort)Random.Range(1, width - 1);
            pos.Y = (ushort)Random.Range(1, height - 1);
            if ((map.TerrainMatrix[pos.Y, pos.X] & TerrainType.RIVER) == TerrainType.NONE)
            {
                clusters.Add(new List<U16Vec2>());
                SpreadCluster(map, pos, clustersParam.Size, clusters[i]);
            }
            else
                --i;
        }
        return clusters;
    }

    static void SpreadCluster(Chunk map, U16Vec2 pos, byte remainingSize, List<U16Vec2> cluster)
    {
        ushort height = map.Height;
        ushort width = map.Width;

        cluster.Add(pos);
        map.TerrainMatrix[pos.Y, pos.X] |= TerrainType.BUILDING;

        if (pos.Y > 0 && pos.Y < height - 1 && pos.X > 0 && pos.X < width - 1 && remainingSize != 0)
        {
            byte k = (byte)(pos.X & 1); //TODO Провести рефакторинг.

            if ((map.TerrainMatrix[pos.Y - (k ^ 1), pos.X - 1] & (TerrainType.RIVER | TerrainType.BUILDING)) == TerrainType.NONE)
                SpreadCluster(map, new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y - (k ^ 1))), (byte)(remainingSize - 1), cluster);
            if ((map.TerrainMatrix[pos.Y + k, pos.X - 1] & (TerrainType.RIVER | TerrainType.BUILDING)) == TerrainType.NONE)
                SpreadCluster(map, new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y + k)), (byte)(remainingSize - 1), cluster);
            if ((map.TerrainMatrix[pos.Y - 1, pos.X] & (TerrainType.RIVER | TerrainType.BUILDING)) == TerrainType.NONE)
                SpreadCluster(map, new U16Vec2(pos.X, (ushort)(pos.Y - 1)), (byte)(remainingSize - 1), cluster);
            if ((map.TerrainMatrix[pos.Y + 1, pos.X] & (TerrainType.RIVER | TerrainType.BUILDING)) == TerrainType.NONE)
                SpreadCluster(map, new U16Vec2(pos.X, (ushort)(pos.Y + 1)), (byte)(remainingSize - 1), cluster);
            if ((map.TerrainMatrix[pos.Y - (k ^ 1), pos.X + 1] & (TerrainType.RIVER | TerrainType.BUILDING)) == TerrainType.NONE)
                SpreadCluster(map, new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y - (k ^ 1))), (byte)(remainingSize - 1), cluster);
            if ((map.TerrainMatrix[pos.Y + k, pos.X + 1] & (TerrainType.RIVER | TerrainType.BUILDING)) == TerrainType.NONE)
                SpreadCluster(map, new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y + k)), (byte)(remainingSize - 1), cluster);
        }
    }

    public static List<List<U16Vec2>> CreateRoads(float[,] heightMap, ref TerrainType[,] terrainMap, List<List<U16Vec2>> clusters, GlobalTerrainSettings.RoadsSettings roadsParam)
    {
        List<List<U16Vec2>> roads = new List<List<U16Vec2>>();
        for (byte i = 0; i < clusters.Count >> 1; ++i)
        {
            roads.Add(new List<U16Vec2>());
            DirectRoad(heightMap, terrainMap, clusters[i][0], clusters[(clusters.Count >> 1) + i][0], roads[i], roadsParam);
        }
        return roads;
    }

    static void DirectRoad(float[,] heightMatrix, TerrainType[,] terrainMatrix, U16Vec2 pos, U16Vec2 destination, List<U16Vec2> road, GlobalTerrainSettings.RoadsSettings roadsParam)
    {
        ushort height = (ushort)heightMatrix.GetLength(0);
        ushort width = (ushort)heightMatrix.GetLength(1);

        road.Add(pos);
        terrainMatrix[pos.Y, pos.X] |= TerrainType.ROAD;

        if (pos.Y > 0 && pos.Y < height - 1 && pos.X > 0 && pos.X < width - 1 && !(destination.X == pos.X && destination.Y == pos.Y))
        {
            byte k = (byte)(pos.X & 1); //TODO Провести рефакторинг.

            float max = heightMatrix[pos.Y + k, pos.X - 1], avg = 0;
            float cur = heightMatrix[pos.Y + k, pos.X - 1];
            avg += cur;

            cur = heightMatrix[pos.Y + 1, pos.X];
            if (max < cur)
                max = cur;
            avg += cur;

            cur = heightMatrix[pos.Y + k, pos.X + 1];
            if (max < cur)
                max = cur;
            avg += cur;

            cur = heightMatrix[pos.Y - 1 + k, pos.X + 1];
            if (max < cur)
                max = cur;
            avg += cur;

            cur = heightMatrix[pos.Y - 1, pos.X];
            if (max < cur)
                max = cur;
            avg += cur;

            cur = heightMatrix[pos.Y - 1 + k, pos.X - 1];
            if (max < cur)
                max = cur;
            avg += cur;

            avg /= 6;
            max += 0.0001f; //!

            float weight = 0, buf;
            sbyte dx = 0, dy = 0;

            if (!road.Contains(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y + k))) && ((buf = (Mathf.Abs(destination.X - pos.X) - Mathf.Abs(destination.X - (pos.X - 1)) + Mathf.Abs(destination.Y - pos.Y) - Mathf.Abs(destination.Y - (pos.Y + k)) + 3) * (max - Mathf.Abs(heightMatrix[pos.Y + k, pos.X - 1] - avg))) * ((terrainMatrix[pos.Y + k, pos.X - 1] & TerrainType.ROAD) != TerrainType.NONE ? roadsParam.RoadMergeMultiplier : 1) * ((terrainMatrix[pos.Y, pos.X] & terrainMatrix[pos.Y + k, pos.X - 1] & TerrainType.RIVER) != TerrainType.NONE ? roadsParam.GoingAlongRiverMultiplier : 1) > weight))
            {
                weight = buf;
                dx = -1;
                dy = (sbyte)k;
            }
            if (!road.Contains(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y + k))) && ((buf = (Mathf.Abs(destination.X - pos.X) - Mathf.Abs(destination.X - (pos.X + 1)) + Mathf.Abs(destination.Y - pos.Y) - Mathf.Abs(destination.Y - (pos.Y + k)) + 3) * (max - Mathf.Abs(heightMatrix[pos.Y + k, pos.X + 1] - avg))) * ((terrainMatrix[pos.Y + k, pos.X + 1] & TerrainType.ROAD) != TerrainType.NONE ? roadsParam.RoadMergeMultiplier : 1) * ((terrainMatrix[pos.Y, pos.X] & terrainMatrix[pos.Y + k, pos.X + 1] & TerrainType.RIVER) != TerrainType.NONE ? roadsParam.GoingAlongRiverMultiplier : 1) > weight))
            {
                weight = buf;
                dx = 1;
                dy = (sbyte)k;
            }
            if (!road.Contains(new U16Vec2(pos.X, (ushort)(pos.Y + 1))) && ((buf = (Mathf.Abs(destination.Y - pos.Y) - Mathf.Abs(destination.Y - (pos.Y + 1)) + 3) * (max - Mathf.Abs(heightMatrix[pos.Y + 1, pos.X] - avg))) * ((terrainMatrix[pos.Y + 1, pos.X] & TerrainType.ROAD) != TerrainType.NONE ? roadsParam.RoadMergeMultiplier : 1) * ((terrainMatrix[pos.Y, pos.X] & terrainMatrix[pos.Y + 1, pos.X] & TerrainType.RIVER) != TerrainType.NONE ? roadsParam.GoingAlongRiverMultiplier : 1) > weight))//!После диагонали
            {
                weight = buf;
                dx = 0;
                dy = 1;
            }
            if (!road.Contains(new U16Vec2((ushort)(pos.X + 1), (ushort)(pos.Y - (k ^ 1)))) && ((buf = (Mathf.Abs(destination.X - pos.X) - Mathf.Abs(destination.X - (pos.X + 1)) + Mathf.Abs(destination.Y - pos.Y) - Mathf.Abs(destination.Y - (pos.Y - (k ^ 1))) + 3) * (max - Mathf.Abs(heightMatrix[pos.Y - (k ^ 1), pos.X + 1] - avg))) * ((terrainMatrix[pos.Y - (k ^ 1), pos.X + 1] & TerrainType.ROAD) != TerrainType.NONE ? roadsParam.RoadMergeMultiplier : 1) * ((terrainMatrix[pos.Y, pos.X] & terrainMatrix[pos.Y - (k ^ 1), pos.X + 1] & TerrainType.RIVER) != TerrainType.NONE ? roadsParam.GoingAlongRiverMultiplier : 1) > weight))
            {
                weight = buf;
                dx = 1;
                dy = (sbyte)(-(k ^ 1));
            }
            if (!road.Contains(new U16Vec2((ushort)(pos.X - 1), (ushort)(pos.Y - (k ^ 1)))) && ((buf = (Mathf.Abs(destination.X - pos.X) - Mathf.Abs(destination.X - (pos.X - 1)) + Mathf.Abs(destination.Y - pos.Y) - Mathf.Abs(destination.Y - (pos.Y - (k ^ 1))) + 3) * (max - Mathf.Abs(heightMatrix[pos.Y - (k ^ 1), pos.X - 1] - avg))) * ((terrainMatrix[pos.Y - (k ^ 1), pos.X - 1] & TerrainType.ROAD) != TerrainType.NONE ? roadsParam.RoadMergeMultiplier : 1) * ((terrainMatrix[pos.Y, pos.X] & terrainMatrix[pos.Y - (k ^ 1), pos.X - 1] & TerrainType.RIVER) != TerrainType.NONE ? roadsParam.GoingAlongRiverMultiplier : 1) > weight))
            {
                weight = buf;
                dx = -1;
                dy = (sbyte)(-(k ^ 1));
            }
            if (!road.Contains(new U16Vec2(pos.X, (ushort)(pos.Y - 1))) && ((buf = (Mathf.Abs(destination.Y - pos.Y) - Mathf.Abs(destination.Y - (pos.Y - 1)) + 3) * (max - Mathf.Abs(heightMatrix[pos.Y - 1, pos.X] - avg))) * ((terrainMatrix[pos.Y - 1, pos.X] & TerrainType.ROAD) != TerrainType.NONE ? roadsParam.RoadMergeMultiplier : 1) * ((terrainMatrix[pos.Y, pos.X] & terrainMatrix[pos.Y - 1, pos.X] & TerrainType.RIVER) != TerrainType.NONE ? roadsParam.GoingAlongRiverMultiplier : 1) > weight))//!После диагонали
            {
                weight = buf;
                dx = 0;
                dy = -1;
            }

            if (dx == 0 && dy == 0)
                throw new System.Exception("Infinite recursion detected. Try to check heightmap values.");

            DirectRoad(heightMatrix, terrainMatrix, new U16Vec2((ushort)(pos.X + dx), (ushort)(pos.Y + dy)), destination, road, roadsParam);
        }
    }
}
