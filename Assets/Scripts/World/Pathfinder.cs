using System.Collections.Generic;

public static class Pathfinder
{
    class DistCoordPair
    {
        public ushort Dist;
        public LocalPos Coord;
    }

    public static List<LocalPos> MakePath(bool[,] blockMatrix, LocalPos from, LocalPos to, bool destIsBlocked)
    {
        UnityEngine.Debug.Assert(from != to);
        ushort height = (ushort)blockMatrix.GetLength(0);
        ushort width = (ushort)blockMatrix.GetLength(1);
        List<LocalPos> path = new List<LocalPos>();
        List<DistCoordPair> queue = new List<DistCoordPair>();
        queue.Add(new DistCoordPair() { Dist = 1, Coord = from });
        do
        {
            LocalPos cur = queue[0].Coord;
            blockMatrix[cur.Y, cur.X] = true;
            path.Add(cur);
			if (queue[0].Dist == 0)
                return path;
            else
            {
                queue.RemoveAt(0);

                for (byte i = 0; i < 6; ++i)
                {
                    GlobalPos node = HexNavigHelper.GetNeighborMapCoords(cur, (TurnedHexDirection)i);
                    if (node.Y >= 0 && node.Y < height && node.X >= 0 && node.X < width && !blockMatrix[node.Y, node.X])
                    {
                        queue.Add(new DistCoordPair() { Dist = HexNavigHelper.Distance(node, to, true), Coord = (LocalPos)node });
                        blockMatrix[node.Y, node.X] = true;
                    }
                    else if (destIsBlocked && node == to)
                        return path;
                }

                queue.Sort((a, b) => a.Dist - b.Dist);
            }
        }
        while (queue.Count != 0);
        UnityEngine.Debug.Log("No path");
        return null;
    }
}
