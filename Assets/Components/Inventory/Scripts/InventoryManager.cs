using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{

    public ItemDatabase ItemDatabase;

    [Header("Inventory components")]
    public Inventory Inventory;
    public Equipment Equipment;
    public Crafting Crafting;
    public Hotbar Hotbar;

    [Header("Panels & buttons")]
    public InventoryTooltip Tooltip;
    //public GameObject BackpackPanel;
    public ItemInfoPanel ItemInfoPanel; // На будущее. Замена тултипов для сенсорных экранов
    public GameObject SplitPanel;
    public GameObject DropPanel;

    [Header("Prefabs")]
    public GameObject SlotPrefab;
    public GameObject EquipmentSlotPrefab;
    public GameObject ItemPrefab;
    public GameObject DefaultDroppedItem;

    [Header("Options")]
    public bool EnableTooltip;
    public bool EnableInfoPanel;

    [Header("Other")]
    public GameObject DraggingSlot;

    [Header("Quality colors")]
    public Color JunkColor;
    public Color NormalColor;
    public Color UnusualColor;
    public Color RareColor;
    public Color LegendaryColor;


    public bool AddItemIntoSlot(Slot slot)
    {
        GameObject draggingItemGameObject = AttachedItem.DraggingItem;
        AttachedItem draggingItem = draggingItemGameObject.GetComponent<AttachedItem>();
        if (slot.GetComponentInChildren<AttachedItem>() == null)
        {
            if (slot.SlotType == SlotType.Inventory)
            {
                return true;
            }
            if (slot.SlotType == SlotType.Equipment && draggingItem.Item.IsEquipment && slot.GetComponent<EquipmentSlot>().EquipmentType.Equals(draggingItem.Item.ItemType))
            {
                InventoryEvents.EquipItem(draggingItem);
                return true;
            }
            if (slot.SlotType == SlotType.Hotbar)
            {
                draggingItem.CreateDuplicate(draggingItemGameObject);
                UpdateStacks(Inventory.Slots);
                return true;
            }
            draggingItem.CurrentSlot = slot.gameObject.transform;
        }
        else
        {
            GameObject itemFromSlotGameObject = slot.transform.GetChild(0).gameObject; ;
            AttachedItem itemFromSlot = itemFromSlotGameObject.GetComponent<AttachedItem>();
            RectTransform itemFromSlotRect = itemFromSlotGameObject.GetComponent<RectTransform>();
            bool sameItem = draggingItem.Item.Id == itemFromSlot.Item.Id;
            if (slot.SlotType == SlotType.Inventory)
            {
                if (sameItem && itemFromSlot.Item.IsStackable)
                {
                    int stack = itemFromSlot.StackSize + draggingItem.StackSize;

                    if (stack <= itemFromSlot.Item.MaxStackSize)
                    {
                        itemFromSlot.StackSize = stack;
                        itemFromSlot.UpdateStackSize();
                        if (itemFromSlotGameObject != null && itemFromSlot.Duplicate != null)
                        {
                            itemFromSlot.Duplicate.GetComponent<AttachedItem>().StackSize = stack;
                            itemFromSlot.UpdateStackSize();
                        }
                        Destroy(draggingItemGameObject);

                        return true;
                    }
                    else
                    {
                        itemFromSlot.StackSize = itemFromSlot.Item.MaxStackSize;
                        itemFromSlot.UpdateStackSize();
                        if (itemFromSlotGameObject != null && itemFromSlot.Duplicate != null)
                        {
                            itemFromSlot.Duplicate.GetComponent<AttachedItem>().StackSize = stack;
                            itemFromSlot.UpdateStackSize();
                        }
                        draggingItem.StackSize = stack - itemFromSlot.Item.MaxStackSize;
                        draggingItem.UpdateStackSize();

                        return false;
                    }
                }
                else
                {
                    if (!draggingItem.Item.IsEquipment)
                    {
                        itemFromSlotGameObject.transform.SetParent(draggingItem.LastSlot);
                        itemFromSlotRect.localPosition = Vector2.zero;
                        return true;
                    }
                    else
                    {
                        if (itemFromSlot.Item.ItemType.Equals(draggingItem.Item.ItemType))
                        {
                            itemFromSlotGameObject.transform.SetParent(draggingItem.LastSlot);
                            itemFromSlotRect.localPosition = Vector2.zero;
                            return true;
                        }
                    }
                }
                return false;
            }
            if (slot.SlotType == SlotType.Equipment)
            {
                if (slot.GetComponent<EquipmentSlot>().EquipmentType.Equals(draggingItem.Item.ItemType))
                {
                    itemFromSlotGameObject.transform.SetParent(draggingItem.LastSlot.transform);
                    itemFromSlotRect.localPosition = Vector2.zero;
                    return true;
                }
                return false;
            }
        }
        return false;
    }



    public Color FindColor(Item item)
    {

        switch(item.ItemQuality)
        {
            case ItemQuality.Junk :
                return JunkColor;
            case ItemQuality.Normal:
                return NormalColor;
            case ItemQuality.Unusual:
                return UnusualColor;
            case ItemQuality.Rare:
                return RareColor;
            case ItemQuality.Legendary:
                return LegendaryColor;

        }
        return Color.clear;
    }

    public void UpdateStacks(List<Slot> slots)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].gameObject.transform.childCount > 0)
            {
                AttachedItem attachedItem = slots[i].gameObject.transform.GetChild(0).GetComponent<AttachedItem>();
                if (attachedItem.Item.MaxStackSize > 1)
                {
                    attachedItem.StackSizeText = transform.GetComponentInChildren<Text>();

                    attachedItem.StackSizeText.text = attachedItem.StackSize.ToString();
                    attachedItem.StackSizeText.enabled = true;
                    if (attachedItem.Duplicate != null)
                    {
                        attachedItem.Duplicate.GetComponent<AttachedItem>().StackSizeText = attachedItem.Duplicate.GetComponentInChildren<Text>();
                        attachedItem.Duplicate.GetComponent<AttachedItem>().StackSizeText.text = attachedItem.StackSize.ToString();
                        attachedItem.Duplicate.GetComponent<AttachedItem>().StackSizeText.enabled = true;
                    }
                }
                else
                {
                    attachedItem.StackSizeText.enabled = false;
                    if (attachedItem.Duplicate != null)
                    {
                        attachedItem.Duplicate.GetComponent<AttachedItem>().StackSizeText.enabled = false;
                    }
                }
            }
        }
    }

    public void Consume(AttachedItem attachedItem)
    {
        InventoryEvents.ConsumeItem(attachedItem);
    }

    public void Equip(AttachedItem attachedItem)
    {
        InventoryEvents.EquipItem(attachedItem);
    }

    public void Split(AttachedItem attachedItem)
    {
        InventoryEvents.SplitItem(attachedItem);
    }

    public void Drop(AttachedItem attachedItem)
    {
        InventoryEvents.DropItem(attachedItem);
    }


}