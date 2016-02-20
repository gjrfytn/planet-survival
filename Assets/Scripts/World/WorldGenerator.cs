using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RiversParameters
{
    public float Height; //Коэффициент высоты реки относительно средней высоты
    public byte Count;
    public ushort MinimumLength;
    public byte Attempts; //Количество попыток построения реки из одного хекса
    public float FlowHeightKoef; //Насколько реалистично река распространяется относительно высоты (1 - самое реалистичное)
}

[System.Serializable]
public class ClustersParameters
{
    public byte Count;
    public byte Size;
}

public static class WorldGenerator
{
    static Stack<Vector2> RiverStack = new Stack<Vector2>(); //Стек для постройки реки

    /// <summary>
    /// Создаёт карту высот. 
    /// </summary>
    /// <param name="matrix">[out] Карта высот.</param>
    /// <param name="r">Шероховатость.</param>
    /// Использует фрактальный алгоритм Diamond Square
    public static void CreateHeightmap(float[,] matrix, float roughness, float topLeft, float topRight, float bottomLeft, float bottomRight)
    {
        //Debug.Assert(topLeft >= 0 && topRight >= 0 && bottomLeft >= 0 && bottomRight >= 0);

        ushort height = (ushort)matrix.GetLength(0);
        ushort width = (ushort)matrix.GetLength(1);

        //Задаём начальные значения по углам
        matrix[0, 0] = topLeft;
        matrix[0, width - 1] = topRight;
        matrix[height - 1, 0] = bottomLeft;
        matrix[height - 1, width - 1] = bottomRight;
        bool end = false;
        for (ushort stepY = (ushort)(height - 1), stepX = (ushort)(width - 1), halfY = (ushort)(stepY / 2), halfX = (ushort)(stepX / 2); !end; end = (halfY == 1 && halfX == 1), halfY -= (ushort)(halfY / 2), stepY = (ushort)(halfY * 2), halfX -= (ushort)(halfX / 2), stepX = (ushort)(halfX * 2))
        {
            float randRange = (((halfY + halfX) / 2f) / ((height + width) / 2f)) * roughness;

            for (ushort y = halfY; y < height; y += stepY)
                for (ushort x = halfX; x < width; x += stepX)
                    matrix[y, x] = (matrix[y - halfY, x - halfX] + matrix[y + halfY, x - halfX] + matrix[y - halfY, x + halfX] + matrix[y + halfY, x + halfX]) / 4 + Random.Range(-randRange, randRange);

            for (ushort y = halfY; y < height; y += stepY)
            {
                matrix[y, 0] = (matrix[y - halfY, 0] + matrix[y + halfY, 0] + matrix[y, 0 + halfX]) / 3 + Random.Range(-randRange, randRange);
                matrix[y, width - 1] = (matrix[y - halfY, width - 1] + matrix[y, width - 1 - halfX] + matrix[y + halfY, width - 1]) / 3 + Random.Range(-randRange, randRange);
            }

            for (ushort x = halfX; x < width; x += stepX)
            {
                matrix[0, x] = (matrix[0, x - halfX] + matrix[0 + halfY, x] + matrix[0, x + halfX]) / 3 + Random.Range(-randRange, randRange);
                matrix[height - 1, x] = (matrix[height - 1 - halfY, x] + matrix[height - 1, x - halfX] + matrix[height - 1, x + halfX]) / 3 + Random.Range(-randRange, randRange);
            }

            for (ushort y = halfY; y < height - 1; y += stepY)
                for (ushort x = stepX; x < width - 1; x += stepX)
                    matrix[y, x] = (matrix[y, x - halfX] + matrix[y - halfY, x] + matrix[y, x + halfX] + matrix[y + halfY, x]) / 4 + Random.Range(-randRange, randRange);

            for (ushort x = halfX; x < width - 1; x += stepX)
                for (ushort y = stepY; y < height - 1; y += stepY)
                    matrix[y, x] = (matrix[y, x - halfX] + matrix[y - halfY, x] + matrix[y, x + halfX] + matrix[y + halfY, x]) / 4 + Random.Range(-randRange, randRange);
        }
    }

    public static void CreateHeightmap(float?[,] matrix, float roughness, float topLeft, float topRight, float bottomLeft, float bottomRight)
    {
        //Debug.Assert(topLeft >= 0 && topRight >= 0 && bottomLeft >= 0 && bottomRight >= 0);

        ushort height = (ushort)matrix.GetLength(0);
        ushort width = (ushort)matrix.GetLength(1);

        //Задаём начальные значения по углам
        matrix[0, 0] = topLeft;
        matrix[0, width - 1] = topRight;
        matrix[height - 1, 0] = bottomLeft;
        matrix[height - 1, width - 1] = bottomRight;

        bool end = false;
        for (ushort stepY = (ushort)(height - 1), stepX = (ushort)(width - 1), halfY = (ushort)(stepY / 2), halfX = (ushort)(stepX / 2); !end; end = (halfY == 1 && halfX == 1), halfY -= (ushort)(halfY / 2), stepY = (ushort)(halfY * 2), halfX -= (ushort)(halfX / 2), stepX = (ushort)(halfX * 2))
        {
            float randRange = (((halfY + halfX) / 2f) / ((height + width) / 2f)) * roughness;

            for (ushort y = halfY; y < height; y += stepY)
                for (ushort x = halfX; x < width; x += stepX)
                    if (!matrix[y, x].HasValue)
                        matrix[y, x] = (matrix[y - halfY, x - halfX] + matrix[y + halfY, x - halfX] + matrix[y - halfY, x + halfX] + matrix[y + halfY, x + halfX]) / 4 + Random.Range(-randRange, randRange);

            for (ushort y = halfY; y < height; y += stepY)
            {
				if (!matrix[y, 0].HasValue)
                    matrix[y, 0] = (matrix[y - halfY, 0] + matrix[y + halfY, 0] + matrix[y, 0 + halfX]) / 3 + Random.Range(-randRange, randRange);
				if (!matrix[y, width - 1].HasValue)
                    matrix[y, width - 1] = (matrix[y - halfY, width - 1] + matrix[y, width - 1 - halfX] + matrix[y + halfY, width - 1]) / 3 + Random.Range(-randRange, randRange);
            }

            for (ushort x = halfX; x < width; x += stepX)
            {
				if (!matrix[0, x].HasValue)
                    matrix[0, x] = (matrix[0, x - halfX] + matrix[0 + halfY, x] + matrix[0, x + halfX]) / 3 + Random.Range(-randRange, randRange);
				if (!matrix[height - 1, x].HasValue)
                    matrix[height - 1, x] = (matrix[height - 1 - halfY, x] + matrix[height - 1, x - halfX] + matrix[height - 1, x + halfX]) / 3 + Random.Range(-randRange, randRange);
            }

            for (ushort y = halfY; y < height - 1; y += stepY)
                for (ushort x = stepX; x < width - 1; x += stepX)
					if (!matrix[y, x].HasValue)
                        matrix[y, x] = (matrix[y, x - halfX] + matrix[y - halfY, x] + matrix[y, x + halfX] + matrix[y + halfY, x]) / 4 + Random.Range(-randRange, randRange);

            for (ushort x = halfX; x < width - 1; x += stepX)
                for (ushort y = stepY; y < height - 1; y += stepY)
					if (!matrix[y, x].HasValue)
                        matrix[y, x] = (matrix[y, x - halfX] + matrix[y - halfY, x] + matrix[y, x + halfX] + matrix[y + halfY, x]) / 4 + Random.Range(-randRange, randRange);
        }
    }

    public static void MakeEqualHeightLine(float[,] matrix, Vector2[] vertices, float height)
    {
        foreach (Vector2 v in vertices)
            if (v.x < 0 || v.x >= matrix.GetLength(1) || v.y < 0 || v.y >= matrix.GetLength(0))
                throw new System.ArgumentOutOfRangeException("vertices", v.ToString(), "Vector is out of matrix length.");

        for (byte i = 0; i < vertices.Length - 1; ++i)
        {
            Vector2 v = vertices[i];
            while (v != vertices[i + 1])
            {
                matrix[(int)v.y, (int)v.x] = height;
                v.y += Mathf.Clamp(vertices[i + 1].y - v.y, -1, 1);
                v.x += Mathf.Clamp(vertices[i + 1].x - v.x, -1, 1);
            }
        }
        matrix[(int)vertices[vertices.Length - 1].y, (int)vertices[vertices.Length - 1].x] = height;
    }

    /// <summary>
    /// Создаёт реки.
    /// </summary>
    /// <returns>Список рек.</returns>
    /// <param name="heightMatrix">Карта высот.</param>
    /// <param name="riverMatrix">[out] Карта рек.</param>
    public static List<List<Vector2>> CreateRivers(float[,] heightMap, bool[,] riverMap, RiversParameters riversParam)
    {
        ushort height = (ushort)riverMap.GetLength(0);
        ushort width = (ushort)riverMap.GetLength(1);

        double avg = 0;
        foreach (float h in heightMap)
            avg += h;
        avg /= height * width;
        float minRiverHeight = (float)avg * riversParam.Height;

        List<List<Vector2>> rivers = new List<List<Vector2>>();

        for (byte i = 0; i < riversParam.Count; ++i)
        {
            bool riverCreated = false;
            for (ushort y = 1; y < height - 1 && !riverCreated; ++y) //TODO
                for (ushort x = 1; x < width - 1 && !riverCreated; ++x) //TODO
                    if (heightMap[y, x] > minRiverHeight && !riverMap[y, x] && RiverNeighbours(y, x, riverMap) == 0) //Проверяем, можно ли нам начать создание реки с этого хекса
                        for (byte k = 0; k < riversParam.Attempts && !riverCreated; ++k)
                        {
                            DirectRiver(y, x, heightMap, riverMap, riversParam.FlowHeightKoef); //Запускаем рекурсию
                            if (RiverStack.Count >= riversParam.MinimumLength)
                            { //Если река получилась больше необходим длины, то помечаем ячейки матрицы, иначе пробуем ещё раз 
                                foreach (Vector2 hex in RiverStack)
                                    riverMap[(int)hex.y, (int)hex.x] = true;
                                riverCreated = true;
                                rivers.Add(new List<Vector2>(RiverStack));
                                rivers[rivers.Count - 1].Reverse();
                            }
                            RiverStack.Clear();
                        }
        }
        return rivers;
    }

    enum Direction
    {
        TOP_LEFT,
        BOTTOM_LEFT,
        TOP,
        BOTTOM,
        TOP_RIGHT,
        BOTTOM_RIGHT
    }

    /// <summary>
    /// Выбирает направление распространения реки.
    /// </summary>
    /// <param name="y">y координата.</param>
    /// <param name="x">x координата.</param>
    /// <param name="heightMatrix">Карта высот.</param>
    /// <param name="matrix">Карта рек.</param>
    static void DirectRiver(ushort y, ushort x, float[,] heightMatrix, bool[,] riverMatrix, float flowHeightKoef)
    {
        ushort height = (ushort)riverMatrix.GetLength(0);
        ushort width = (ushort)riverMatrix.GetLength(1);

        RiverStack.Push(new Vector2(x, y));

        if (y > 0 && y < height - 1 && x > 0 && x < width - 1)
        {
            byte limiter = 0; //Переменная, контролирующая проверку всех направлений и выход из цикла, если ни одно не подходит
            bool dirFound = false;

            byte k = (byte)((x % 2) != 0 ? 1 : 0); //Учитываем чётность/нечётность ряда хексов
            do
            {
                switch (Random.Range(0, 7))
                { //Выбираем случайное направление
                    case (int)Direction.BOTTOM_LEFT:
                        if (heightMatrix[y - 1 + k, x - 1] * flowHeightKoef <= heightMatrix[y, x] && !riverMatrix[y - 1 + k, x - 1] && RiverNeighbours((ushort)(y - 1 + k), (ushort)(x - 1), riverMatrix) < 2)
                        {
                            DirectRiver((ushort)(y - 1 + k), (ushort)(x - 1), heightMatrix, riverMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter++;
                        break;
                    case (int)Direction.TOP_LEFT:
                        if (heightMatrix[y + k, x - 1] * flowHeightKoef <= heightMatrix[y, x] && !riverMatrix[y + k, x - 1] && RiverNeighbours((ushort)(y + k), (ushort)(x - 1), riverMatrix) < 2)
                        {
                            DirectRiver((ushort)(y + k), (ushort)(x - 1), heightMatrix, riverMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter++;
                        break;
                    case (int)Direction.BOTTOM:
                        if (heightMatrix[y - 1, x] * flowHeightKoef <= heightMatrix[y, x] && !riverMatrix[y - 1, x] && RiverNeighbours((ushort)(y - 1), x, riverMatrix) < 2)
                        {
                            DirectRiver((ushort)(y - 1), x, heightMatrix, riverMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter++;
                        break;
                    case (int)Direction.TOP:
                        if (heightMatrix[y + 1, x] * flowHeightKoef <= heightMatrix[y, x] && !riverMatrix[y + 1, x] && RiverNeighbours((ushort)(y + 1), x, riverMatrix) < 2)
                        {
                            DirectRiver((ushort)(y + 1), x, heightMatrix, riverMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter++;
                        break;
                    case (int)Direction.BOTTOM_RIGHT:
                        if (heightMatrix[y - 1 + k, x + 1] * flowHeightKoef <= heightMatrix[y, x] && !riverMatrix[y - 1 + k, x + 1] && RiverNeighbours((ushort)(y - 1 + k), (ushort)(x + 1), riverMatrix) < 2)
                        {
                            DirectRiver((ushort)(y - 1 + k), (ushort)(x + 1), heightMatrix, riverMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter++;
                        break;
                    case (int)Direction.TOP_RIGHT:
                        if (heightMatrix[y + k, x + 1] * flowHeightKoef <= heightMatrix[y, x] && !riverMatrix[y + k, x + 1] && RiverNeighbours((ushort)(y + k), (ushort)(x + 1), riverMatrix) < 2)
                        {
                            DirectRiver((ushort)(y + k), (ushort)(x + 1), heightMatrix, riverMatrix, flowHeightKoef);
                            dirFound = true;
                        }
                        limiter++;
                        break;
                }
            }
            while (!dirFound && limiter != 6);
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
    static byte RiverNeighbours(ushort y, ushort x, bool[,] riverMatrix)
    {
        ushort height = (ushort)riverMatrix.GetLength(0);
        ushort width = (ushort)riverMatrix.GetLength(1);

        byte k = (byte)((x % 2) != 0 ? 1 : 0);

        byte riversCount = 0;
        if (y > 0 && x > 0 && (riverMatrix[y - 1 + k, x - 1] || RiverStack.Contains(new Vector2(x - 1, y - 1 + k))))
            riversCount++;
        if (x > 0 && y < height - 1 && (riverMatrix[y + k, x - 1] || RiverStack.Contains(new Vector2(x - 1, y + k))))
            riversCount++;
        if (y > 0 && (riverMatrix[y - 1, x] || RiverStack.Contains(new Vector2(x, y - 1))))
            riversCount++;
        if (y < height - 1 && (riverMatrix[y + 1, x] || RiverStack.Contains(new Vector2(x, y + 1))))
            riversCount++;
        if (y > 0 && x < width - 1 && (riverMatrix[y - 1 + k, x + 1] || RiverStack.Contains(new Vector2(x + 1, y - 1 + k))))
            riversCount++;
        if (x < width - 1 && y < height - 1 && (riverMatrix[y + k, x + 1] || RiverStack.Contains(new Vector2(x + 1, y + k))))
            riversCount++;

        return riversCount;
    }

    //UNDONE
    public static List<List<Vector2>> CreateClusters(GlobalMap map, ClustersParameters clustersParam)
    {
        ushort height = (ushort)map.HeightMatrix.GetLength(0);
        ushort width = (ushort)map.HeightMatrix.GetLength(1);

        List<List<Vector2>> clusters = new List<List<Vector2>>(clustersParam.Count);

        for (byte i = 0; i < clustersParam.Count; ++i)
        {
            ushort x = (ushort)Random.Range(1, width - 1);
            ushort y = (ushort)Random.Range(1, height - 1);
            if (!map.RiverMatrix[y, x])
            {
                clusters.Add(new List<Vector2>());
                SpreadCluster(map, y, x, clustersParam.Size, clusters[i]);
            }
            else
                --i;
        }
        return clusters;
    }

    static void SpreadCluster(GlobalMap map, ushort y, ushort x, byte remainingSize, List<Vector2> cluster)
    {
        ushort height = (ushort)map.ClusterMatrix.GetLength(0);
        ushort width = (ushort)map.ClusterMatrix.GetLength(1);

        cluster.Add(new Vector2(x, y));
        map.ClusterMatrix[y, x] = true;

        if (y > 0 && y < height - 1 && x > 0 && x < width - 1 && remainingSize != 0)
        {
            byte k = (byte)((x % 2) != 0 ? 1 : 0);

            if (!map.RiverMatrix[y - 1 + k, x - 1] && !map.ClusterMatrix[y - 1 + k, x - 1])
                SpreadCluster(map, (ushort)(y - 1 + k), (ushort)(x - 1), (byte)(remainingSize - 1), cluster);
            if (!map.RiverMatrix[y + k, x - 1] && !map.ClusterMatrix[y + k, x - 1])
                SpreadCluster(map, (ushort)(y + k), (ushort)(x - 1), (byte)(remainingSize - 1), cluster);
            if (!map.RiverMatrix[y - 1, x] && !map.ClusterMatrix[y - 1, x])
                SpreadCluster(map, (ushort)(y - 1), x, (byte)(remainingSize - 1), cluster);
            if (!map.RiverMatrix[y + 1, x] && !map.ClusterMatrix[y + 1, x])
                SpreadCluster(map, (ushort)(y + 1), x, (byte)(remainingSize - 1), cluster);
            if (!map.RiverMatrix[y - 1 + k, x + 1] && !map.ClusterMatrix[y - 1 + k, x + 1])
                SpreadCluster(map, (ushort)(y - 1 + k), (ushort)(x + 1), (byte)(remainingSize - 1), cluster);
            if (!map.RiverMatrix[y + k, x + 1] && !map.ClusterMatrix[y + k, x + 1])
                SpreadCluster(map, (ushort)(y + k), (ushort)(x + 1), (byte)(remainingSize - 1), cluster);
        }
    }

    public static List<List<Vector2>> CreateRoads(float[,] heightMap, bool[,] roadMap, List<List<Vector2>> clusters)
    {
        List<List<Vector2>> roads = new List<List<Vector2>>();

        for (byte i = 0; i < clusters.Count / 2; ++i)
        {
            roads.Add(new List<Vector2>());
            DirectRoad(heightMap, roadMap, (ushort)clusters[i][0].y, (ushort)clusters[i][0].x, clusters[clusters.Count / 2 + i][0], roads[i]);
        }
        return roads;
    }

    static void DirectRoad(float[,] heightMatrix, bool[,] roadMatrix, ushort y, ushort x, Vector2 destination, List<Vector2> road)
    {
        ushort height = (ushort)roadMatrix.GetLength(0);
        ushort width = (ushort)roadMatrix.GetLength(1);

        road.Add(new Vector2(x, y));
        roadMatrix[y, x] = true;

        if (y > 0 && y < height - 1 && x > 0 && x < width - 1 && !(destination.x == x && destination.y == y))
        {
            byte k = (byte)((x % 2) != 0 ? 1 : 0);

            float max = heightMatrix[y + k, x - 1], avg = 0;
            float cur = heightMatrix[y + k, x - 1];
            avg += cur;

            cur = heightMatrix[y + 1, x];
            if (max < cur)
                max = cur;
            avg += cur;

            cur = heightMatrix[y + k, x + 1];
            if (max < cur)
                max = cur;
            avg += cur;

            cur = heightMatrix[y - 1 + k, x + 1];
            if (max < cur)
                max = cur;
            avg += cur;

            cur = heightMatrix[y - 1, x];
            if (max < cur)
                max = cur;
            avg += cur;

            cur = heightMatrix[y - 1 + k, x - 1];
            if (max < cur)
                max = cur;
            avg += cur;

            avg /= 6;
            max += 0.0001f; //!

            float weight = 0, buf;
            sbyte dx = 0, dy = 0;
            if (!road.Contains(new Vector2(x - 1, y + k)) && ((buf = (Mathf.Abs(destination.x - x) - Mathf.Abs(destination.x - (x - 1)) + Mathf.Abs(destination.y - y) - Mathf.Abs(destination.y - (y + k)) + 3) * (max - Mathf.Abs(heightMatrix[y + k, x - 1] - avg))) > weight))
            {
                weight = buf;
                dx = -1;
                dy = (sbyte)k;
            }
            if (!road.Contains(new Vector2(x + 1, y + k)) && ((buf = (Mathf.Abs(destination.x - x) - Mathf.Abs(destination.x - (x + 1)) + Mathf.Abs(destination.y - y) - Mathf.Abs(destination.y - (y + k)) + 3) * (max - Mathf.Abs(heightMatrix[y + k, x + 1] - avg))) > weight))
            {
                weight = buf;
                dx = 1;
                dy = (sbyte)k;
            }
            if (!road.Contains(new Vector2(x, y + 1)) && ((buf = (Mathf.Abs(destination.y - y) - Mathf.Abs(destination.y - (y + 1)) + 3) * (max - Mathf.Abs(heightMatrix[y + 1, x] - avg))) > weight))//!После диагонали
            {
                weight = buf;
                dx = 0;
                dy = 1;
            }
            if (!road.Contains(new Vector2(x + 1, y - 1 + k)) && ((buf = (Mathf.Abs(destination.x - x) - Mathf.Abs(destination.x - (x + 1)) + Mathf.Abs(destination.y - y) - Mathf.Abs(destination.y - (y - 1 + k)) + 3) * (max - Mathf.Abs(heightMatrix[y - 1 + k, x + 1] - avg))) > weight))
            {
                weight = buf;
                dx = 1;
                dy = (sbyte)(-1 + k);
            }
            if (!road.Contains(new Vector2(x - 1, y - 1 + k)) && ((buf = (Mathf.Abs(destination.x - x) - Mathf.Abs(destination.x - (x - 1)) + Mathf.Abs(destination.y - y) - Mathf.Abs(destination.y - (y - 1 + k)) + 3) * (max - Mathf.Abs(heightMatrix[y - 1 + k, x - 1] - avg))) > weight))
            {
                weight = buf;
                dx = -1;
                dy = (sbyte)(-1 + k);
            }
            if (!road.Contains(new Vector2(x, y - 1)) && ((buf = (Mathf.Abs(destination.y - y) - Mathf.Abs(destination.y - (y - 1)) + 3) * (max - Mathf.Abs(heightMatrix[y - 1, x] - avg))) > weight))//!После диагонали
            {
                weight = buf;
                dx = 0;
                dy = -1;
            }

            if (dx == 0 && dy == 0)
                throw new System.Exception("Infinite recursion detected. Check heightmap values.");

            DirectRoad(heightMatrix, roadMatrix, (ushort)(y + dy), (ushort)(x + dx), destination, road);
        }
    }
}
