using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    public ItemInfoPanel ItemInfoPanel;
    public SplitItem SplitPanel;
    //public GameObject DropPanel;

    [Header("Prefabs")]
    public GameObject SlotPrefab;
    public GameObject EquipmentSlotPrefab;
    public GameObject ItemPrefab;

    [Header("Options")]
    public bool EnableTooltip;
    public bool EnableInfoPanel;

    [Header("Other")]
    public GameObject DraggingSlot;

    [Header("Quality colors")]
    [SerializeField]
    private Color JunkColor;
    [SerializeField]
    private Color NormalColor;
    [SerializeField]
    private Color UnusualColor;
    [SerializeField]
    private Color RareColor;
    [SerializeField]
    private Color LegendaryColor;

    [HideInInspector]
    public AttachedItem SelectedItem;

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
                        UpdateItemStack(itemFromSlot);
                        if (itemFromSlotGameObject != null && itemFromSlot.Duplicate != null)
                        {
                            itemFromSlot.Duplicate.GetComponent<AttachedItem>().StackSize = stack;
                            UpdateItemStack(itemFromSlot);
                        }
                        Destroy(draggingItemGameObject);

                        return true;
                    }
                    else
                    {
                        itemFromSlot.StackSize = itemFromSlot.Item.MaxStackSize;
                        UpdateItemStack(itemFromSlot);
                        if (itemFromSlotGameObject != null && itemFromSlot.Duplicate != null)
                        {
                            itemFromSlot.Duplicate.GetComponent<AttachedItem>().StackSize = stack;
                            UpdateItemStack(itemFromSlot);
                        }
                        draggingItem.StackSize = stack - itemFromSlot.Item.MaxStackSize;
                        UpdateItemStack(draggingItem);

                        return false;
                    }
                }
                else
                {
                    if (!draggingItem.Item.IsEquipment && draggingItem.LastSlot.GetComponent<Slot>() != null)
                    {
                        itemFromSlotGameObject.transform.SetParent(draggingItem.LastSlot);
                        itemFromSlotRect.localPosition = Vector2.zero;
                        return true;
                    }
                    else
                    {
                        if (itemFromSlot.Item.ItemType.Equals(draggingItem.Item.ItemType) && draggingItem.LastSlot.GetComponent<Slot>() != null)
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

    //Update stacks for all items
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

    //Update stack for specific item
    public void UpdateItemStack(AttachedItem attachedItem)
    {
        attachedItem.StackSizeText = attachedItem.transform.GetComponentInChildren<Text>();
        if (attachedItem.Item.MaxStackSize > 1)
        {
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
        if (attachedItem.StackSize <= 0)
        {
            if (attachedItem.Duplicate != null)
            {
                Destroy(attachedItem.Duplicate);
            }
            Destroy(attachedItem.gameObject);
        }
    }

    public void Consume(AttachedItem attachedItem)
    {
        InventoryEvents.ConsumeItem(attachedItem);
    }

    public void Equip(AttachedItem attachedItem)
    {
        InventoryEvents.EquipItem(attachedItem);
        ItemInfoPanel.ActivatePanel(attachedItem);
    }

    public void Unequip(AttachedItem attachedItem)
    {
        InventoryEvents.UnequipItem(attachedItem);
        ItemInfoPanel.ActivatePanel(attachedItem);
    }

    public void Split(AttachedItem attachedItem)
    {
        SplitPanel.Split(attachedItem);
    }

    public void Drop(AttachedItem attachedItem)
    {
        InventoryEvents.DropItem(attachedItem);
    }


    public virtual void OnDrop(PointerEventData data)
    {
        GameObject draggingItem = AttachedItem.DraggingItem;
        if (draggingItem != null)
        {
            if (draggingItem.GetComponent<AttachedItem>().Item.IsEquipment)
            {
                if (draggingItem.GetComponent<AttachedItem>().LastSlot.GetComponent<Slot>().SlotType == SlotType.Equipment)
                {
                    InventoryEvents.EquipItem(draggingItem.GetComponent<AttachedItem>());
                }
            }
            if (draggingItem.GetComponent<AttachedItem>().LastSlot.GetComponent<Slot>().SlotType == SlotType.Hotbar)
            {
                draggingItem.GetComponent<AttachedItem>().CreateDuplicate(draggingItem);
            }
            draggingItem.transform.SetParent(draggingItem.GetComponent<AttachedItem>().LastSlot);
            draggingItem.GetComponent<RectTransform>().localPosition = Vector3.zero;
        }
    }

}