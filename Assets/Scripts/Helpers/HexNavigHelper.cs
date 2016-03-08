using UnityEngine;

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
    public static Vector2 GetNeighborMapCoords(Vector2 mapCoords, HexDirection direction)
    {
        switch (direction)
        {
            case HexDirection.TOP_LEFT:
                return new Vector2(mapCoords.x - 1, mapCoords.y + 1 - ((mapCoords.x % 2) == 0 ? 1 : 0));
            case HexDirection.TOP:
                return new Vector2(mapCoords.x, mapCoords.y + 1);
            case HexDirection.TOP_RIGHT:
                return new Vector2(mapCoords.x + 1, mapCoords.y + 1 - ((mapCoords.x % 2) == 0 ? 1 : 0));
            case HexDirection.BOTTOM_RIGHT:
                return new Vector2(mapCoords.x + 1, mapCoords.y - ((mapCoords.x % 2) == 0 ? 1 : 0));
            case HexDirection.BOTTOM:
                return new Vector2(mapCoords.x, mapCoords.y - 1);
            case HexDirection.BOTTOM_LEFT:
                return new Vector2(mapCoords.x - 1, mapCoords.y - ((mapCoords.x % 2) == 0 ? 1 : 0));
        }
        throw new System.ArgumentException("Invalid direction", "direction");
    }

    public static Vector2 GetNeighborMapCoords(Vector2 mapCoords, TurnedHexDirection direction)
    {
        switch (direction)
        {
            case TurnedHexDirection.LEFT_TOP:
                return new Vector2(mapCoords.x - ((mapCoords.y % 2) == 0 ? 1 : 0), mapCoords.y + 1);
            case TurnedHexDirection.RIGHT_TOP:
                return new Vector2(mapCoords.x + 1 - ((mapCoords.y % 2) == 0 ? 1 : 0), mapCoords.y + 1);
            case TurnedHexDirection.RIGHT:
                return new Vector2(mapCoords.x + 1, mapCoords.y);
            case TurnedHexDirection.RIGHT_BOTTOM:
                return new Vector2(mapCoords.x + 1 - ((mapCoords.y % 2) == 0 ? 1 : 0), mapCoords.y - 1);
            case TurnedHexDirection.LEFT_BOTTOM:
                return new Vector2(mapCoords.x - ((mapCoords.y % 2) == 0 ? 1 : 0), mapCoords.y - 1);
            case TurnedHexDirection.LEFT:
                return new Vector2(mapCoords.x - 1, mapCoords.y);
        }
        throw new System.ArgumentException("Invalid turned direction", "direction");
    }

    /// <summary>
    /// Определяет, прилегают ли к друг другу данные координаты.
    /// </summary>
    /// <returns><c>true</c> если прилегают, иначе <c>false</c>.</returns>
    /// <param name="mapCoords1">1 координаты.</param>
    /// <param name="mapCoords2">2 координаты.</param>
    public static bool IsMapCoordsAdjacent(Vector2 mapCoords1, Vector2 mapCoords2, bool turnedHexes)
    {
        if (turnedHexes)
            for (byte i = 0; i < 6; ++i)
            {
                if (GetNeighborMapCoords(mapCoords1, (TurnedHexDirection)i) == mapCoords2)
                    return true;
            }
        else
            for (byte i = 0; i < 6; ++i)
            {
                if (GetNeighborMapCoords(mapCoords1, (HexDirection)i) == mapCoords2)
                    return true;
            }
        return false;
    }
}