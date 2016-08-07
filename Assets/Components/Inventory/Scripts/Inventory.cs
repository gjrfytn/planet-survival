using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.IO;

public class Inventory : MonoBehaviour, IDropHandler {

    [HideInInspector]
    public InventoryManager InventoryManager;
    [HideInInspector]
    [Range(1,255)]
    public byte Width;
    [HideInInspector]
    [Range(1,255)]
    public byte Height;

    public Transform SlotContainer;

    public List<Slot> Slots;

    [SerializeField]
    public List<AttachedItem> ItemsInInventory = new List<AttachedItem>();

    private void Awake()
    {
        InventoryManager = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>();
    }
    ///<summary>
    ///Обновляет список предметов в инвентаре
    ///</summary>
    public void UpdateItemList() //TODO: Уменьшить количество вызовов этого метода. Причина: вызывается дважды при старте
    {
        ItemsInInventory.Clear();
        for(int i = 0; i < Slots.Count; i++)
        {
            Transform transform = Slots[i].gameObject.transform;
            if(transform.childCount != 0)
            {
                ItemsInInventory.Add(transform.GetChild(0).GetComponent<AttachedItem>());
            }
        }
    }
    ///<summary>
    ///Добавляет предмет в инвентарь по его Id
    ///</summary>
    public GameObject AddItem(uint id, int stackSize)
    {
        if (InventoryManager.ItemDatabase != null)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].transform.childCount == 0)
                {
                    GameObject item = (GameObject)Instantiate(InventoryManager.ItemPrefab);
                    AttachedItem attachedItem = item.GetComponent<AttachedItem>();

                    RectTransform itemRectTransform = item.GetComponent<RectTransform>();

                    attachedItem.Item = InventoryManager.ItemDatabase[id];
                    if (attachedItem.StackSize <= attachedItem.Item.MaxStackSize && stackSize <= attachedItem.Item.MaxStackSize)
                    {
                        attachedItem.StackSize = stackSize;
                    }
                    else
                    {
                        attachedItem.StackSize = 1;
                    }
                    item.transform.SetParent(Slots[i].gameObject.transform, false);
                    itemRectTransform.localPosition = Vector2.zero;
                    //itemRectTransform.sizeDelta = Vector2.zero;
                    //itemRectTransform.localScale = Vector3.one;
                    item.transform.GetChild(0).GetComponent<Image>().sprite = attachedItem.Item.Icon;
                    item.transform.GetChild(2).GetComponent<Image>().color = InventoryManager.FindColor(attachedItem.Item);

                    item.name = attachedItem.Item.Name;
                    attachedItem.Item.ItemStartNumber = ItemsInInventory.Count;
                    InventoryManager.UpdateItemStack(attachedItem);
                    return item;
                }
            }
            InventoryManager.UpdateStacks(Slots);
            UpdateItemList();
        }
        else
        {
            Debug.LogError("Item database is null");
        }
        return null;
    }
    ///<summary>
    ///Удаляет Item из списка ItemsInInventory
    ///</summary>
    public void RemoveItem(Item item)
    {
        for(int i = 0; i < ItemsInInventory.Count; i++)
        {
            if(item.Equals(ItemsInInventory[i]))
            {
                ItemsInInventory.RemoveAt(item.ItemStartNumber);
            }
        }
    }

    ///<summary>
    ///Удаляет Item из списка ItemsInInventory вместе с игровым объектом
    ///</summary>
    public void RemoveItemAndGameObject(AttachedItem attachedItem)
    {
        for (int i = 0; i < ItemsInInventory.Count; i++)
        {
            if (attachedItem == ItemsInInventory[i])
            {
                ItemsInInventory.RemoveAt(i);
            }
        }

        for(int k = 0; k < Slots.Count; k++)
        {
            if(Slots[k].gameObject.transform.childCount != 0)
            {
                GameObject tempItemGameObject = Slots[k].gameObject.transform.GetChild(0).gameObject;
                AttachedItem tempAttachedItem = tempItemGameObject.GetComponent<AttachedItem>();
                if(tempAttachedItem == attachedItem)
                {
                    Destroy(tempItemGameObject);
                    break;
                }
            }
        }
    }

    ///<summary>
    ///Очищает слоты инвентаря от предметов
    ///</summary>
    public void RemoveAllItems()
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].gameObject.transform.childCount != 0)
            {
                DestroyImmediate(Slots[i].gameObject.transform.GetChild(0).gameObject);
            }
        }
    }

    ///<summary>
    ///Проверяет есть ли добавляемый предмет в инвентаре. Если есть то добавляет его в соседний слот или стакается 
    ///</summary>
    public bool CheckIfItemAlreadyExist(uint id, int stackSize)
    {
        UpdateItemList();
        int stack;
        for(int i = 0; i < ItemsInInventory.Count; i++)
        {
            if(ItemsInInventory[i].Item.Id == id && ItemsInInventory[i].Item.IsStackable)
            {
                stack = ItemsInInventory[i].StackSize + stackSize;
                int rest = (stack) % ItemsInInventory[i].Item.MaxStackSize;
                GameObject tempItem = GetItemGameObject(ItemsInInventory[i].Item);
                if (stack <= ItemsInInventory[i].Item.MaxStackSize)
                {
                    ItemsInInventory[i].StackSize = stack;
                    InventoryManager.UpdateItemStack(tempItem.GetComponent<AttachedItem>());
                    if(tempItem != null && tempItem.GetComponent<AttachedItem>().Duplicate != null)
                    {
                        tempItem.GetComponent<AttachedItem>().Duplicate.GetComponent<AttachedItem>().StackSize = stack;
                        InventoryManager.UpdateItemStack(tempItem.GetComponent<AttachedItem>());
                    }
                    return true;
                }
                else if (stack > ItemsInInventory[i].Item.MaxStackSize)
                {
                    ItemsInInventory[i].StackSize = ItemsInInventory[i].Item.MaxStackSize;
                    if (tempItem != null && tempItem.GetComponent<AttachedItem>().Duplicate != null)
                    {
                        tempItem.GetComponent<AttachedItem>().Duplicate.GetComponent<AttachedItem>().StackSize = stack;
                    }
                    AddItem(ItemsInInventory[i].Item.Id, rest);
                    InventoryManager.UpdateItemStack(GetItemGameObject(ItemsInInventory[i].Item).GetComponent<AttachedItem>());
                    return true;
                }
            }
        }
        return false;
    }

    public void MoveItemIntoSlot(AttachedItem attachedItem)
    {
        if (CheckItemFit())
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if(Slots[i].gameObject.transform.childCount == 0)
                {
                    attachedItem.gameObject.transform.SetParent(Slots[i].transform);
                    attachedItem.gameObject.transform.localPosition = Vector2.zero;
                    break;
                }
            }
        }
    }

    public void CloneItemIntoSlot(AttachedItem attachedItem)
    {
        if (CheckItemFit())
        {
            GameObject tempItem = Instantiate(attachedItem.gameObject);
            tempItem.name = attachedItem.Item.Name;
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].gameObject.transform.childCount == 0)
                {
                    tempItem.transform.SetParent(Slots[i].transform, false);
                    tempItem.transform.localPosition = Vector2.zero;
                    //tempItem.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    //tempItem.GetComponent<RectTransform>().localScale = Vector3.one;
                    break;
                }
            }
        }
    }

    public bool CheckItemFit()
    {

        if (ItemsInInventory.Count < (Width * Height))
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].transform.childCount == 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public GameObject GetItemGameObject(Item item)
    {
        for(int i = 0; i < Slots.Count; i++)
        {
            if(Slots[i].transform.childCount != 0)
            {
                GameObject itemGameObject = Slots[i].transform.GetChild(0).gameObject;
                Item itemObject = itemGameObject.GetComponent<AttachedItem>().Item;
                if(itemObject.Equals(item))
                {
                    return itemGameObject;
                }
            }
        }
        return null;
    }

    ///<summary>
    ///Удаляет слоты и очищает список Slots
    ///</summary>
    public void RemoveSlots()
    {

        for (int i = 0; i < Slots.Count; i++)
        {
            DestroyImmediate(Slots[i].gameObject);
        }
        Slots = new List<Slot>();

    }

    ///<summary>
    ///Добавляет слоты
    ///</summary>
    public void CreateSlots(byte height, byte width)
    {
        if (Slots.Count < height * width)
        {
            RemoveSlots();
            for (int i = 0; i < height * width; i++)
            {
                GameObject slot = (GameObject)Instantiate(InventoryManager.SlotPrefab);
                slot.transform.SetParent(SlotContainer.transform);
                slot.GetComponent<Slot>().SlotType = SlotType.Inventory;
                slot.name = "Slot " + i;
                Slots.Add(slot.GetComponent<Slot>());
                Slots[i].GetComponent<Slot>().SlotNumber += i;
            }
        }
        else if (Slots.Count >= height * width)
        {
            RemoveSlots();
            CreateSlots(height, width);
        }
    }

    public void OnDrop(PointerEventData data)
    {
        InventoryManager.OnDrop(data);
    }

}
