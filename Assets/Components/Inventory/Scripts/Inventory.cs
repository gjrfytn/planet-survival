using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour, IDropHandler {

    public InventoryManager InventoryManager;

    [HideInInspector]
    [Range(1,255)]
    public byte Width;
    [HideInInspector]
    [Range(1,255)]
    public byte Height;

    public Transform SlotContainer;

    public List<GameObject> Slots;

    [SerializeField]
    public List<Item> ItemsInInventory = new List<Item>();

    void Awake()
    {

    }


	// Use this for initialization
	void Start () {
        UpdateItemList();
	}
	
	// Update is called once per frame
	void Update () {
	
	}



    ///<summary>
    ///Обновляет список предметов в инвентаре
    ///</summary>
    public void UpdateItemList()
    {
        ItemsInInventory.Clear();
        for(int i = 0; i < Slots.Count; i++)
        {
            Transform transform = Slots[i].gameObject.transform;
            if(transform.childCount != 0)
            {
                ItemsInInventory.Add(transform.GetChild(0).GetComponent<AttachedItem>().Item);
            }
        }
    }
    ///<summary>
    ///Добавляет предмет в инвентарь по его Id
    ///</summary>
    public GameObject AddItem(int id, int stackSize)
    {
        for(int i = 0; i < Slots.Count; i++)
        {
            if(Slots[i].gameObject.transform.childCount == 0)
            {
                GameObject item = (GameObject)Instantiate(InventoryManager.ItemPrefab);
                AttachedItem attachedItem = item.GetComponent<AttachedItem>();
                attachedItem.Item = InventoryManager.ItemDatabase.FindItemById(id);
                if (attachedItem.Item.StackSize <= attachedItem.Item.MaxStackSize && stackSize <= attachedItem.Item.MaxStackSize)
                {
                    attachedItem.Item.StackSize = stackSize;
                }
                else
                {
                    attachedItem.Item.StackSize = 1;
                }
                item.transform.SetParent(Slots[i].gameObject.transform);
                item.GetComponent<RectTransform>().localPosition = Vector2.zero;
                item.transform.GetChild(0).GetComponent<Image>().sprite = attachedItem.Item.Icon;
                item.transform.GetChild(2).GetComponent<Image>().color = InventoryManager.FindColor(attachedItem.Item);

                item.name = attachedItem.Item.Name;
                attachedItem.Item.ItemStartNumber = ItemsInInventory.Count;
                attachedItem.UpdateStackSize();
                return item;
            }
        }
        InventoryManager.Stackable(Slots);
        UpdateItemList();
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
    public void RemoveItemAndGameObject(Item item)
    {
        for (int i = 0; i < ItemsInInventory.Count; i++)
        {
            if (item.Equals(ItemsInInventory[i]))
            {
                ItemsInInventory.RemoveAt(i);
            }
        }

        for(int k = 0; k < Slots.Count; k++)
        {
            if(Slots[k].gameObject.transform.childCount != 0)
            {
                GameObject itemGameObject = Slots[k].gameObject.transform.GetChild(0).gameObject;
                Item attachedItem = itemGameObject.GetComponent<AttachedItem>().Item;
                if(attachedItem.Equals(item))
                {
                    Destroy(itemGameObject);
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



    /*public void Stackable()
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].gameObject.transform.childCount > 0)
            {
                AttachedItem item = Slots[i].gameObject.transform.GetChild(0).GetComponent<AttachedItem>();
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

    }*/

    ///<summary>
    ///Проверяет есть ли добавляемый предмет в инвентаре. Если есть то добавляет его в соседний слот или стакается 
    ///</summary>
    public bool CheckIfItemAlreadyExist(int id, int stackSize)
    {
        UpdateItemList();
        int stack;
        for(int i = 0; i < ItemsInInventory.Count; i++)
        {
            if(ItemsInInventory[i].Id == id && ItemsInInventory[i].IsStackable)
            {
                stack = ItemsInInventory[i].StackSize + stackSize;
                int rest = (stack) % ItemsInInventory[i].MaxStackSize;
                GameObject tempItem = GetItemGameObject(ItemsInInventory[i]);
                if (stack <= ItemsInInventory[i].MaxStackSize)
                {
                    ItemsInInventory[i].StackSize = stack;
                    tempItem.GetComponent<AttachedItem>().UpdateStackSize();
                    //GameObject temp = GetItemGameObject(ItemsInInventory[i]);
                    if(tempItem != null && tempItem.GetComponent<AttachedItem>().Duplicate != null)
                    {
                        tempItem.GetComponent<AttachedItem>().Duplicate.GetComponent<AttachedItem>().Item.StackSize = stack;
                        tempItem.GetComponent<AttachedItem>().UpdateStackSize();
                    }
                    return true;
                }
                if (stack > ItemsInInventory[i].MaxStackSize)
                {
                    //int rest = (stack) % ItemsInInventory[i].MaxStackSize;
                    ItemsInInventory[i].StackSize = ItemsInInventory[i].MaxStackSize;
                    if (tempItem != null && tempItem.GetComponent<AttachedItem>().Duplicate != null)
                    {
                        tempItem.GetComponent<AttachedItem>().Duplicate.GetComponent<AttachedItem>().Item.StackSize = stack;
                    }
                    AddItem(ItemsInInventory[i].Id, rest);
                    GetItemGameObject(ItemsInInventory[i]).GetComponent<AttachedItem>().UpdateStackSize();
                    return true;
                }
            }
        }
        return false;
    }

    public void StackItem(Item item)
    {

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
        Slots = new List<GameObject>();

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
                Slots.Add(slot);
                Slots[i].gameObject.transform.GetComponent<Slot>().SlotNumber += i;
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
        GameObject draggingItem = AttachedItem.DraggingItem;
        if (draggingItem != null)
        {
            if (draggingItem.GetComponent<AttachedItem>().Item.IsEquipment)
            {
                if (draggingItem.GetComponent<AttachedItem>().LastSlot.GetComponent<Slot>().SlotType == SlotType.Equipment)
                {
                    InventoryEvents.EquipItem(draggingItem.GetComponent<AttachedItem>().Item);
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
