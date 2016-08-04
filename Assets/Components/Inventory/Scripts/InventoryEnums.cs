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
    Consumable,
    Socket,
    Other,
}

public enum SlotType : byte
{
    Inventory,
    Equipment,
    Crafting,
    Hotbar,
}

