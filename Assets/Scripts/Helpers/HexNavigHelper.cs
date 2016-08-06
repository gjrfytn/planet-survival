
using GlobalPos = S32Vec2;

public enum HexDirection : byte
{
    TOP_LEFT,
    TOP,
    TOP_RIGHT,
    BOTTOM_RIGHT,
    BOTTOM,
    BOTTOM_LEFT
}

public enum TurnedHexDirection : byte
{
    LEFT_TOP,
    RIGHT_TOP,
    RIGHT,
    RIGHT_BOTTOM,
    LEFT_BOTTOM,
    LEFT
}

public static class HexNavigHelper
{
    public static GlobalPos GetNeighborMapCoords(GlobalPos pos, HexDirection direction)
    {
        switch (direction)
        {
            case HexDirection.TOP_LEFT:
                return new GlobalPos(pos.X - 1, pos.Y + (pos.X & 1));
            case HexDirection.TOP:
                return new GlobalPos(pos.X, pos.Y + 1);
            case HexDirection.TOP_RIGHT:
                return new GlobalPos(pos.X + 1, pos.Y + (pos.X & 1));
            case HexDirection.BOTTOM_RIGHT:
                return new GlobalPos(pos.X + 1, pos.Y - (pos.X & 1 ^ 1));
            case HexDirection.BOTTOM:
                return new GlobalPos(pos.X, pos.Y - 1);
            case HexDirection.BOTTOM_LEFT:
                return new GlobalPos(pos.X - 1, pos.Y - (pos.X & 1 ^ 1));
        }
        throw new System.ArgumentException("Invalid direction", "direction");
    }

    public static GlobalPos GetNeighborMapCoords(GlobalPos pos, TurnedHexDirection direction)
    {
        switch (direction)
        {
            case TurnedHexDirection.LEFT_TOP:
                return new GlobalPos(pos.X - (pos.Y & 1 ^ 1), pos.Y + 1);
            case TurnedHexDirection.RIGHT_TOP:
                return new GlobalPos(pos.X + (pos.Y & 1), pos.Y + 1);
            case TurnedHexDirection.RIGHT:
                return new GlobalPos(pos.X + 1, pos.Y);
            case TurnedHexDirection.RIGHT_BOTTOM:
                return new GlobalPos(pos.X + (pos.Y & 1), pos.Y - 1);
            case TurnedHexDirection.LEFT_BOTTOM:
                return new GlobalPos(pos.X - (pos.Y & 1 ^ 1), pos.Y - 1);
            case TurnedHexDirection.LEFT:
                return new GlobalPos(pos.X - 1, pos.Y);
        }
        throw new System.ArgumentException("Invalid turned direction", "direction");
    }

    /// <summary>
    /// Определяет, прилегают ли к друг другу данные координаты.
    /// </summary>
    /// <returns><c>true</c> если прилегают, иначе <c>false</c>.</returns>
    /// <param name="mapCoords1">1 координаты.</param>
    /// <param name="mapCoords2">2 координаты.</param>
    public static bool IsMapCoordsAdjacent(GlobalPos coords1, GlobalPos coords2, bool turnedHexes)
    {
        if (turnedHexes)
            for (byte i = 0; i < 6; ++i)
            {
                if (GetNeighborMapCoords(coords1, (TurnedHexDirection)i) == coords2)
                    return true;
            }
        else
            for (byte i = 0; i < 6; ++i)
            {
                if (GetNeighborMapCoords(coords1, (HexDirection)i) == coords2)
                    return true;
            }
        return false;
    }

    public static ushort Distance(GlobalPos coords1, GlobalPos coords2, bool turnedHexes)
    {
        int q1, r1, s1, q2, r2, s2;
        if (turnedHexes)
        {
            q1 = coords1.X - (coords1.Y + (coords1.Y & 1 ^ 1) >> 1);
            r1 = coords1.Y;
            s1 = -q1 - r1;
            q2 = coords2.X - (coords2.Y + (coords2.Y & 1 ^ 1) >> 1);
            r2 = coords2.Y;
            s2 = -q2 - r2;
        }
        else
        {
            q1 = coords1.X;
            r1 = coords1.Y - (coords1.X + (coords1.X & 1 ^ 1) >> 1);
            s1 = -q1 - r1;
            q2 = coords2.X;
            r2 = coords2.Y - (coords2.X + (coords2.X & 1 ^ 1) >> 1);
            s2 = -q2 - r2;
        }
        return (ushort)(UnityEngine.Mathf.Abs(q1 - q2) + UnityEngine.Mathf.Abs(r1 - r2) + UnityEngine.Mathf.Abs(s1 - s2) >> 1);
    }
}