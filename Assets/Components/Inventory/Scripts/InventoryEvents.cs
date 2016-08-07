using UnityEngine;
using System.Collections;

public static class InventoryEvents
{

    public delegate void ItemDelegate(AttachedItem attachedItem);

    public static event ItemDelegate OnConsume;
    public static event ItemDelegate OnEquip;
    public static event ItemDelegate OnUnequip;
    public static event ItemDelegate OnSplit;
    public static event ItemDelegate OnDrop;
    public static event ItemDelegate OnPickUp;

    public static event ItemDelegate OnLeftClick;
    public static event ItemDelegate OnRightClick;


    public static void ConsumeItem(AttachedItem attachedItem)
    {
        if (OnConsume != null)
        {
            OnConsume(attachedItem);
        }
    }

    public static void EquipItem(AttachedItem attachedItem)
    {
        if (OnEquip != null)
        {
            OnEquip(attachedItem);
        }
    }

    public static void UnequipItem(AttachedItem attachedItem)
    {
        if (OnUnequip != null)
        {
            OnUnequip(attachedItem);
        }
    }

    public static void SplitItem(AttachedItem attachedItem)
    {
        if (OnSplit != null)
        {
           OnSplit(attachedItem);
        }
    }

    public static void DropItem(AttachedItem attachedItem)
    {
        if(OnDrop != null)
        {
            OnDrop(attachedItem);
        }
    }

    public static void PickUpItem(AttachedItem attachedItem)
    {
        if (OnPickUp != null)
        {
            OnPickUp(attachedItem);
        }
    }

    public static void ItemLeftClick(AttachedItem attachedItem)
    {
        if(OnLeftClick != null)
        {
            OnLeftClick(attachedItem);
        }
    }
    public static void ItemRightClick(AttachedItem attachedItem)
    {
        if (OnRightClick != null)
        {
            OnRightClick(attachedItem);
        }
    }
}
