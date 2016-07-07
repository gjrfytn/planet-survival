using UnityEngine;
using System.Collections;

public static class InventoryEvents
{

    public delegate void ItemDelegate(Item item);

    public static event ItemDelegate ItemUsed;
    public static event ItemDelegate ItemEquipped;
    public static event ItemDelegate ItemUnequipped;
    public static event ItemDelegate ItemDropped;
    public static event ItemDelegate ItemPickedUp;

    public static void UseItem(Item item)
    {
        if (ItemUsed != null)
        {
            ItemUsed(item);
        }
    }

    public static void EquipItem(Item item)
    {
        if (ItemEquipped != null)
        {
            ItemEquipped(item);
        }
    }

    public static void UnequipItem(Item item)
    {
        if (ItemUnequipped != null)
        {
            ItemUnequipped(item);
        }
    }

    //Временно
    public static void DropItem(Item item)
    {
        if(ItemDropped != null)
        {
            ItemDropped(item);
        }
    }

    public static void PickUpItem(Item item)
    {
        if (ItemPickedUp != null)
        {
            ItemPickedUp(item);
        }
    }
}
