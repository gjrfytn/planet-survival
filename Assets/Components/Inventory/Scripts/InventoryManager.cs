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
    public GameObject ItemActionButtons; // TODO Вслпывающие кнопки при нажатии правой кнопки мыши на предмете

    [Header("Prefabs")]
    public GameObject SlotPrefab;
    public GameObject EquipmentSlotPrefab;
    public GameObject ItemPrefab;
    public GameObject DefaultDroppedItem;

    [Header("Options")]
    public bool EnableTooltip;
    public bool EnableInfoPanel;

    [Header("Other")] // Неудачное название
    public GameObject DraggingSlot;

    [Header("Quality colors")]
    public Color JunkColor;
    public Color NormalColor;
    public Color UnusualColor;
    public Color RareColor;
    public Color LegendaryColor;

    [HideInInspector]
    public Item item;

    public void InfoPanel(Item item)
    {

    }


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
                InventoryEvents.EquipItem(draggingItem.Item);
                return true;
            }
            if (slot.SlotType == SlotType.Hotbar)
            {
                draggingItem.CreateDuplicate(draggingItemGameObject);
                Stackable(Inventory.Slots);
                return true;
            }
            draggingItem.CurrentSlot = slot.gameObject.transform;
        }
        else
        {
            GameObject itemFromSlotGameObject = slot.transform.GetChild(0).gameObject; ;
            AttachedItem itemFromSlot = itemFromSlotGameObject.GetComponent<AttachedItem>();
            bool sameItem = draggingItem.Item.Id == itemFromSlot.Item.Id;
            if (slot.SlotType == SlotType.Inventory)
            {
                if (sameItem && itemFromSlot.Item.IsStackable)
                {
                    int stack = itemFromSlot.Item.StackSize + draggingItem.Item.StackSize;
                    //GameObject tempItem = itemFromSlotGameObject;

                    if (stack <= itemFromSlot.Item.MaxStackSize)
                    {
                        itemFromSlot.Item.StackSize = stack;
                        itemFromSlot.UpdateStackSize();
                        if (itemFromSlotGameObject != null && itemFromSlot.Duplicate != null)
                        {
                            itemFromSlot.Duplicate.GetComponent<AttachedItem>().Item.StackSize = stack;
                            itemFromSlot.UpdateStackSize();
                        }
                        Destroy(draggingItemGameObject);

                        return true;
                    }
                    else
                    {
                        itemFromSlot.Item.StackSize = itemFromSlot.Item.MaxStackSize;
                        itemFromSlot.UpdateStackSize();
                        if (itemFromSlotGameObject != null && itemFromSlot.Duplicate != null)
                        {
                            itemFromSlot.Duplicate.GetComponent<AttachedItem>().Item.StackSize = stack;
                            itemFromSlot.UpdateStackSize();
                        }
                        draggingItem.Item.StackSize = stack - itemFromSlot.Item.MaxStackSize;
                        draggingItem.UpdateStackSize();

                        return false;
                    }
                }
                else
                {
                    if (!draggingItem.Item.IsEquipment)
                    {
                        itemFromSlotGameObject.transform.SetParent(draggingItem.LastSlot);
                        itemFromSlotGameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
                        return true;
                    }
                    else
                    {
                        if (itemFromSlot.Item.ItemType.Equals(draggingItem.Item.ItemType))
                        {
                            itemFromSlotGameObject.transform.SetParent(draggingItem.LastSlot);
                            itemFromSlotGameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
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
                    itemFromSlotGameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
                    return true;
                }
                return false;
            }
        }
        return false;
    }



    public Color FindColor(Item item)
    {

        if (item.ItemQuality == ItemQuality.Junk)
        {
            return JunkColor;
        }
        if (item.ItemQuality == ItemQuality.Normal)
        {
            return NormalColor;
        }
        if (item.ItemQuality == ItemQuality.Unusual)
        {
            return UnusualColor;
        }
        if (item.ItemQuality == ItemQuality.Rare)
        {
            return RareColor;
        }
        if (item.ItemQuality == ItemQuality.Legendary)
        {
            return LegendaryColor;
        }
        return Color.clear;
    }

    public void Stackable(List<GameObject> slots)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].gameObject.transform.childCount > 0)
            {
                //AttachedItem item = slots[i].gameObject.transform.GetChild(0).GetComponent<AttachedItem>();
                AttachedItem item = slots[i].gameObject.transform.GetChild(0).GetComponent<AttachedItem>();
                if (item.Item.MaxStackSize > 1)
                {
                    //Подразумевается, что буде использоваться только 1 объект с присоединенным текстом
                    item.transform.GetComponentInChildren<Text>().text = item.Item.StackSize.ToString();
                    //StackSizeText.text = item.Item.StackSize.ToString();
                    item.transform.GetComponentInChildren<Text>().enabled = true; //IsStackable;
                }
                else
                {
                    item.transform.GetComponentInChildren<Text>().enabled = false;
                }
            }
        }
    }

    public void DropItem()
    {

    }

    public void SplitItem()
    {

    }

}