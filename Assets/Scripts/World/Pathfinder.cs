using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Pathfinder
{
    class DistCoordPair
    {
        public ushort Dist;
        public Vector2 Coord;
    }

    public static List<Vector2> MakePath(bool[,] blockMatrix, Vector2 from, Vector2 to)
    {
        Debug.Assert(from != to);
        bool[,] blocks = (bool[,])blockMatrix.Clone(); //TODO Возможно есть более быстрый метод.
        ushort height = (ushort)blocks.GetLength(0);
        ushort width = (ushort)blocks.GetLength(1);
        List<Vector2> path = new List<Vector2>();
        List<DistCoordPair> queue = new List<DistCoordPair>();
        queue.Add(new DistCoordPair() { Dist = 1, Coord = from });
        do
        {
            Vector2 cur = queue[0].Coord;
            blocks[(int)cur.y, (int)cur.x] = true;
            path.Add(cur);
            ushort dist = queue[0].Dist;
            if (dist == 0)
            {
                return path;
            }
            else
            {
                queue.RemoveAt(0);

                for (byte i = 0; i < 6; ++i)
                {
                    Vector2 node = World.GetNeighborMapCoords(cur, (HexDirection)i);
                    if (node.y >= 0 && node.y < height && node.x >= 0 && node.x < width && !blocks[(int)node.y, (int)node.x])
                    {
                        queue.Add(new DistCoordPair() { Dist = (ushort)(Mathf.Abs(to.x - node.x) + Mathf.Abs(to.y - node.y)), Coord = node });
                        blocks[(int)node.y, (int)node.x] = true;
                    }
                    else if (node == to)//UNDONE
                    {
                        Debug.Log("Incomplete path");
                        return path;
                    }
                }

                queue = queue.OrderBy(a => a.Dist).ToList(); //TODO Заменить на Sort (убрать system.linq)
            }
        }
        while (queue.Count != 0);
        Debug.Log("No path");
        return null;
    }
}
