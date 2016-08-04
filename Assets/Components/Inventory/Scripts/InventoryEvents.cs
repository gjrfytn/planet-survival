using UnityEngine;
using System.Collections;

public static class InventoryEvents
{

    public delegate void ItemDelegate(AttachedItem attachedItem);

    public static event ItemDelegate ItemConsumed;
    public static event ItemDelegate ItemEquipped;
    public static event ItemDelegate ItemUnequipped;
    public static event ItemDelegate ItemSplited;
    public static event ItemDelegate ItemDropped;
    public static event ItemDelegate ItemPickedUp;

    public static event ItemDelegate ItemRightClicked;


    public static void ConsumeItem(AttachedItem attachedItem)
    {
        if (ItemConsumed != null)
        {
            ItemConsumed(attachedItem);
        }
    }

    public static void EquipItem(AttachedItem attachedItem)
    {
        if (ItemEquipped != null)
        {
            ItemEquipped(attachedItem);
        }
    }

    public static void UnequipItem(AttachedItem attachedItem)
    {
        if (ItemUnequipped != null)
        {
            ItemUnequipped(attachedItem);
        }
    }

    public static void SplitItem(AttachedItem attachedItem)
    {
        if (ItemSplited != null)
        {
            ItemSplited(attachedItem);
        }
    }

    public static void DropItem(AttachedItem attachedItem)
    {
        if(ItemDropped != null)
        {
            ItemDropped(attachedItem);
        }
    }

    public static void PickUpItem(AttachedItem attachedItem)
    {
        if (ItemPickedUp != null)
        {
            ItemPickedUp(attachedItem);
        }
    }

    public static void ItemRightClick(AttachedItem attachedItem)
    {
        ItemRightClicked(attachedItem);
    }
}
