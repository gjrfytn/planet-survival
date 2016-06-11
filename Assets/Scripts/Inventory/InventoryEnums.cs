using UnityEngine;
using System.Collections;

public enum ItemType : byte
{
    Weapon,
    OffHand,
    Head,
    Chest,
    Belt,
    Hands,
    Pants,
    Boots,
    Socket,
    Backpack,
    Consumable,
    Book,
    AudioPlayer,
    AudioRecord,
    Other,
}


public enum EquipmentItemType : byte
{
    Head,
    Chest,
    Pants,
    Boots,
    Weapon,
    OffHand,
    Socket,
    Backpack,
}


public enum ItemQuality : byte
{
    Legendary,
    Rare,
    Unusual,
    Normal,
    Junk
}

public enum OtherItemType : byte
{
    Consumable,
    Socket,
    Book,
}

public enum SlotType : byte
{
    Inventory,
    Equipment,
    Crafting,
    Hotbar,
}

