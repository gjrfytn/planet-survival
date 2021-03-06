﻿using UnityEngine;

public class InventoryItemActions : MonoBehaviour {

    InventoryManager InventoryManager;
    Inventory Inventory;
    Equipment Equipment;
    //Crafting Crafting;

    void OnEnable()
    {
        InventoryEvents.OnConsume += ConsumeItem;
        InventoryEvents.OnEquip += EquipItem;
        InventoryEvents.OnUnequip += UnequipItem;
        InventoryEvents.OnDrop += DropItem;
        InventoryEvents.OnPickUp += PickUpItem;
        InventoryEvents.OnRightClick += ItemRightClick;
    }
    void OnDisable()
    {
        InventoryEvents.OnConsume -= ConsumeItem;
        InventoryEvents.OnEquip -= EquipItem;
        InventoryEvents.OnUnequip -= UnequipItem;
        InventoryEvents.OnDrop -= DropItem;
        InventoryEvents.OnPickUp -= PickUpItem;
        InventoryEvents.OnRightClick -= ItemRightClick;
    }

    void Start()
    {
        InventoryManager = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        Equipment = InventoryManager.Equipment;
        //Crafting = InventoryManager.Crafting;
    }

    void ConsumeItem(AttachedItem attachedItem)
    {
        PlayUseSound(attachedItem);

        Item duplicateItem = null;

        if (attachedItem.Duplicate != null)
        {
            duplicateItem = attachedItem.Duplicate.GetComponent<AttachedItem>().Item;
        }
        attachedItem.StackSize--;
        InventoryManager.UpdateItemStack(attachedItem);
        if (duplicateItem != null)
        {
            attachedItem.Duplicate.GetComponent<AttachedItem>().StackSize--;
            InventoryManager.UpdateItemStack(attachedItem);
            if (attachedItem.StackSize <= 0)
            {
                if (attachedItem.Tooltip != null)
                {
                    attachedItem.Tooltip.DeactivateTooltip();
                }
                Inventory.RemoveItem(attachedItem.Item);
                Destroy(attachedItem.Duplicate);
            }
        }
        if (attachedItem.StackSize <= 0)
        {
            if (attachedItem.Tooltip != null)
            {
                attachedItem.Tooltip.DeactivateTooltip();
            }
            if (attachedItem.ItemInfoPanel != null)
            {
                attachedItem.ItemInfoPanel.DeactivatePanel();
            }
            Inventory.RemoveItemAndGameObject(attachedItem);
        }
    }
    void EquipItem(AttachedItem attachedItem)
    {
        PlayUseSound(attachedItem);

        for (int i = 0; i < Equipment.Slots.Count; i++)
        {
            if (Equipment.Slots[i].GetComponent<EquipmentSlot>().EquipmentType.Equals(attachedItem.Item.ItemType) || attachedItem.Item.ItemType == ItemType.Socket)
            {
                if (Equipment.Slots[i].transform.childCount == 0)
                {
                    attachedItem.gameObject.transform.SetParent(Equipment.Slots[i].transform);
                    attachedItem.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;

                    //attachedItem.CurrentSlot = Equipment.Slots[i].transform;

                    Inventory.UpdateItemList();
                    if (attachedItem.Duplicate != null)
                    {
                        Destroy(attachedItem.Duplicate);
                    }
                    break;

                }
                else if (Equipment.Slots[i].transform.childCount != 0)
                {
                    GameObject equippedItemGameObject = Equipment.Slots[i].transform.GetChild(0).gameObject;
                    AttachedItem equippedItem = equippedItemGameObject.GetComponent<AttachedItem>();

                    if (attachedItem != equippedItem)
                    {
                        UnequipItem(equippedItem);
                        EquipItem(attachedItem);
                    }

                }
            }
        }
    }

    void UnequipItem(AttachedItem equippedItem)
    {
        if (Inventory.CheckItemFit())
        {
            Inventory.MoveItemIntoSlot(equippedItem);

            Inventory.UpdateItemList();
        }
        else
        {
            return;
        }
    }

    void DropItem(AttachedItem attachedItem)
    {
        GameObject dropItem = (GameObject)Instantiate(attachedItem.Item.DroppedItem);
        if (attachedItem.ItemInfoPanel != null)
        {
            attachedItem.ItemInfoPanel.DeactivatePanel();
        }
        if (dropItem.GetComponent<SpriteRenderer>().sprite == null)
        {
            dropItem.GetComponent<SpriteRenderer>().sprite = attachedItem.Item.Icon;
        }
        if (attachedItem == InventoryManager.SelectedItem && InventoryManager.SplitPanel != null)
        {
            InventoryManager.SplitPanel.gameObject.SetActive(false);
        }
        dropItem.AddComponent<PickUpItem>();
        dropItem.GetComponent<PickUpItem>().AttachedItem = attachedItem;
        dropItem.AddComponent<BoxCollider2D>();
        dropItem.GetComponent<BoxCollider2D>().isTrigger = true;
        //dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
        Inventory.UpdateItemList();
        Destroy(attachedItem.gameObject);
        Debug.Log("Предмет выброшен");
    }
    void PickUpItem(AttachedItem attachedItem)
    {

    }

    void ItemRightClick(AttachedItem attachedItem)
    {

    }

    void PlayUseSound(AttachedItem attachedItem)
    {
        if (attachedItem.Item.UseSound != null)
        {
            InventoryManager.GetComponent<AudioSource>().PlayOneShot(attachedItem.Item.UseSound);
        }
    }
}
