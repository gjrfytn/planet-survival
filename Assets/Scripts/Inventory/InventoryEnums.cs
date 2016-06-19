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
    Book,
    AudioPlayer,
    Potion,
    Consumable,
    Other,
}

//Предметы которые можно одеть на персонажа
public enum EquipmentItemType : byte
{
    Weapon,
    Head,
    Chest,
    Pants,
    Boots,
    OffHand,
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

//Любые предметы которые нельзя надеть на персонажа
public enum OtherItemType : byte
{
    Potion, //
    Socket,
    Book,
    AudioRecord,
}

public enum SlotType : byte
{
    Inventory,
    Equipment,
    Crafting,
    Hotbar,
}

