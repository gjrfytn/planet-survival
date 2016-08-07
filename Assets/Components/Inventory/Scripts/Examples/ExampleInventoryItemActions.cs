using UnityEngine;
using System.Collections;

public class ExampleInventoryItemActions : MonoBehaviour {

    InventoryManager InventoryManager;
    Inventory Inventory;
    Equipment Equipment;
    //Crafting Crafting;

    SplitItem SplitPanel;

    void OnEnable()
    {
        InventoryEvents.OnConsume += ConsumeItem;
        InventoryEvents.OnEquip += EquipItem;
        InventoryEvents.OnUnequip += UnequipItem;
        InventoryEvents.OnSplit += SplitItem;
        InventoryEvents.OnDrop += DropItem;
        InventoryEvents.OnPickUp += PickUpItem;
        InventoryEvents.OnRightClick += ItemRightClick;
    }
    void OnDisable()
    {
        InventoryEvents.OnEquip -= EquipItem;
        InventoryEvents.OnUnequip -= UnequipItem;
    }

    void Start()
    {
        InventoryManager = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        Equipment = InventoryManager.Equipment;
        //Crafting = InventoryManager.Crafting;

        SplitPanel = InventoryManager.SplitPanel;
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

                    UnequipItem(equippedItem);

                    if (attachedItem != equippedItem)
                    {
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

    void SplitItem(AttachedItem attachedItem)
    {
        SplitPanel.Slider.onValueChanged.RemoveAllListeners();
        SplitPanel.InputField.onValueChanged.RemoveAllListeners();
        SplitPanel.gameObject.SetActive(true);
        GameObject tempItemGameObject = Instantiate(attachedItem.gameObject);
        AttachedItem tempItem = tempItemGameObject.GetComponent<AttachedItem>();

        tempItemGameObject.transform.SetParent(SplitPanel.Slot.transform);
        tempItemGameObject.name = attachedItem.Item.Name;
        tempItemGameObject.transform.localPosition = Vector2.zero;

        SplitPanel.Slider.minValue = 1;
        SplitPanel.Slider.maxValue = attachedItem.StackSize - 1;
        tempItem.StackSize = (int)SplitPanel.Slider.minValue;
        attachedItem.StackSize = (int)SplitPanel.Slider.maxValue;
        InventoryManager.UpdateItemStack(tempItem);
        InventoryManager.UpdateItemStack(attachedItem);

        SplitPanel.Slider.onValueChanged.AddListener(delegate { ValueToSplit(attachedItem, tempItem); });
        //SplitPanel.InputField.onValueChanged.AddListener(delegate { ValueToSplit(tempItem); });
        SplitPanel.CancelButton.onClick.RemoveAllListeners();
        SplitPanel.CancelButton.onClick.AddListener(delegate { SplitPanel.Cancel(attachedItem, tempItem); });
    }

    void ValueToSplit(AttachedItem itemFromSlot, AttachedItem tempItem)
    {

        tempItem.StackSize = (int)SplitPanel.Slider.value;

        itemFromSlot.StackSize = Mathf.Abs(tempItem.StackSize - (int)SplitPanel.Slider.maxValue - 1);
        //SplitPanel.InputField.text = SplitPanel.Slider.value.ToString();
        /*if (int.Parse(SplitPanel.InputField.text) >= SplitPanel.Slider.minValue && int.Parse(SplitPanel.InputField.text) <= SplitPanel.Slider.maxValue)
        {
            SplitPanel.Slider.value = float.Parse(SplitPanel.InputField.text);
        }*/
        InventoryManager.UpdateItemStack(tempItem);
        InventoryManager.UpdateItemStack(itemFromSlot);
    }

    void DropItem(AttachedItem attachedItem)
    {
        //
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
