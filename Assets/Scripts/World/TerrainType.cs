
[System.Flags]
public enum TerrainType : int //TODO Должен быть ushort, ждём, когда исправят баг
{
    NONE = 0x0,
    WATER = 0x1,
    SWAMP = 0x2,
    MEADOW = 0x4,
    DESERT = 0x8,
    MOUNTAIN = 0x10,
    FOREST = 0x20,
    RIVER = 0x40,
    ROAD = 0x80,
    BUILDING = 0x100
};
