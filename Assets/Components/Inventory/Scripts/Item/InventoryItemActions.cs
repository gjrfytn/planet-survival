using UnityEngine;
using System.Collections;

public class InventoryItemActions : MonoBehaviour {

    InventoryManager InventoryManager;
    Inventory Inventory;
    Equipment Equipment;
    //Crafting Crafting;

    void OnEnable()
    {
        InventoryEvents.ItemConsumed += ConsumeItem;
        InventoryEvents.ItemEquipped += EquipItem;
        InventoryEvents.ItemUnequipped += UnequipItem;
        InventoryEvents.ItemSplited += SplitItem;
        InventoryEvents.ItemDropped += DropItem;
        InventoryEvents.ItemPickedUp += PickUpItem;
        InventoryEvents.ItemRightClicked += ItemRightClick;
    }
    void OnDisable()
    {
        InventoryEvents.ItemEquipped -= EquipItem;
        InventoryEvents.ItemUnequipped -= UnequipItem;
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
        attachedItem.UpdateStackSize();
        if (duplicateItem != null)
        {
            attachedItem.Duplicate.GetComponent<AttachedItem>().StackSize--;
            attachedItem.UpdateStackSize();
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
                    GameObject equippedItem = Equipment.Slots[i].transform.GetChild(0).gameObject;
                    //Item itemFromSlot = equippedItem.GetComponent<AttachedItem>().Item;

                    InventoryEvents.UnequipItem(equippedItem.GetComponent<AttachedItem>());

                    equippedItem.transform.SetParent(transform.parent);
                    equippedItem.GetComponent<RectTransform>().localPosition = Vector3.zero;

                    /*if (attachedItem.GetComponentInParent<Slot>().SlotType == SlotType.Hotbar)
                    {
                        attachedItem.CreateDuplicate(equippedItem);
                    }
                    else if (attachedItem.GetComponentInParent<Slot>().SlotType == SlotType.Equipment)
                    {*/
                    // Нужно реализовать это условие по другому
                    if (Inventory.CheckItemFit())
                    {
                        if (attachedItem.Tooltip != null)
                        {
                            attachedItem.Tooltip.DeactivateTooltip();
                        }
                        InventoryEvents.UnequipItem(equippedItem.GetComponent<AttachedItem>());

                        //Inventory.AddItem(attachedItem.Item.Id, attachedItem.StackSize);
                        Inventory.AddItemIntoSlot(equippedItem);
                        Inventory.UpdateItemList();
                        InventoryManager.UpdateStacks(Inventory.Slots);
                        //Destroy(attachedItem.gameObject);
                    }
                    //}
                    attachedItem.gameObject.transform.SetParent(Equipment.Slots[i].gameObject.transform);
                    attachedItem.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;

                    if (attachedItem.Duplicate != null)
                    {
                        Destroy(attachedItem.Duplicate);
                    }

                    Inventory.UpdateItemList();
                    break;
                }

            }
        }
    }
    void UnequipItem(AttachedItem attachedItem)
    {

    }

    void SplitItem(AttachedItem attachedItem)
    {

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
        dropItem.AddComponent<PickUpItem>();
        dropItem.GetComponent<PickUpItem>().AttachedItem = attachedItem;
        dropItem.AddComponent<BoxCollider2D>();
        dropItem.GetComponent<BoxCollider2D>().isTrigger = true;
        InventoryEvents.DropItem(attachedItem);
        //dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
        Inventory.UpdateItemList();
        Inventory.RemoveItemAndGameObject(attachedItem);
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
