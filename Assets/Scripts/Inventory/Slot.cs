using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Slot : MonoBehaviour, IDropHandler {

    public SlotType SlotType;

    [HideInInspector]
    public int SlotNumber;
    public bool ContainsItem;

    GameObject DraggingItemGameObject;
    AttachedItem DraggingItem;
    protected InventoryManager InventoryManager;
    protected Inventory Inventory;
	// Use this for initialization
	public void Start () {

        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;

	}
	
	// Update is called once per frame
	void Update () {
	
        if(GetComponentInChildren<AttachedItem>() != null)
        {
            ContainsItem = true;
        }
        else
        {
            ContainsItem = false;
        }
    }

    public void OnDrop(PointerEventData data)
    {
        if(AttachedItem.DraggingItem != null)
        {
            DraggingItemGameObject = AttachedItem.DraggingItem;
            DraggingItem = DraggingItemGameObject.GetComponent<AttachedItem>();

            bool addItemIntoSlot = AddItemIntoSlot();
            if(addItemIntoSlot)
            {
                DraggingItemGameObject.transform.SetParent(gameObject.transform);
                DraggingItemGameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
                Debug.Log("addItemIntoSlot: " + addItemIntoSlot);
            }
            else
            {
                DraggingItemGameObject.transform.SetParent(DraggingItem.LastSlot.transform);
                DraggingItemGameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
                Debug.LogWarning("addItemIntoSlot: " + addItemIntoSlot);
            }
        }
    }

    public bool AddItemIntoSlot()
    {
        GameObject draggingItemGameObject = AttachedItem.DraggingItem;
        AttachedItem draggingItem = draggingItemGameObject.GetComponent<AttachedItem>();
        if (GetComponentInChildren<AttachedItem>() == null)
        {
            if (SlotType == SlotType.Inventory)
            {
                //Debug.Log("Inventory");
                return true;
            }
            if (SlotType == SlotType.Equipment && DraggingItem.GetComponent<AttachedItem>().Item.IsEquipment && GetComponent<EquipmentSlot>().EquipmentType.Equals(DraggingItem.GetComponent<AttachedItem>().Item.ItemType))
            {
                //Debug.Log("Equipment");
                Inventory.EquipItem(draggingItem.Item);
                return true;
            }
            if (SlotType == SlotType.Hotbar)
            {
                //Debug.Log("Hotbar");
                DraggingItem.CreateDuplicate(DraggingItemGameObject);
                InventoryManager.Stackable(Inventory.Slots);
                return true;
            }
            draggingItem.CurrentSlot = gameObject.transform;
        }
        else
        {
            GameObject itemFromSlotGameObject = transform.GetChild(0).gameObject; ;
            AttachedItem itemFromSlot = itemFromSlotGameObject.GetComponent<AttachedItem>();
            bool sameItem = DraggingItem.Item.Id == itemFromSlot.Item.Id;
            if (SlotType == SlotType.Inventory)
            {
                if (sameItem && GetComponentInChildren<AttachedItem>().Item.IsStackable)
                {
                    int stackSize = draggingItem.Item.StackSize;
                    int stack;
                    stack = itemFromSlot.Item.StackSize + stackSize;
                    int rest = (stack) % itemFromSlot.Item.MaxStackSize;
                    GameObject tempItem = itemFromSlotGameObject;
                    if (stack <= itemFromSlot.Item.MaxStackSize)
                    {
                        itemFromSlot.Item.StackSize = stack;
                        tempItem.GetComponent<AttachedItem>().UpdateStackSize();
                        if (tempItem != null && tempItem.GetComponent<AttachedItem>().Duplicate != null)
                        {
                            tempItem.GetComponent<AttachedItem>().Duplicate.GetComponent<AttachedItem>().Item.StackSize = stack;
                            tempItem.GetComponent<AttachedItem>().UpdateStackSize();
                        }
                        Destroy(draggingItemGameObject);
                    }
                    if (stack > itemFromSlot.Item.MaxStackSize)
                    {
                        itemFromSlot.Item.StackSize = itemFromSlot.Item.MaxStackSize;
                        tempItem.GetComponent<AttachedItem>().UpdateStackSize();
                        if (tempItem != null && tempItem.GetComponent<AttachedItem>().Duplicate != null)
                        {
                            tempItem.GetComponent<AttachedItem>().Duplicate.GetComponent<AttachedItem>().Item.StackSize = stack;
                            tempItem.GetComponent<AttachedItem>().UpdateStackSize();
                        }
                        draggingItem.Item.StackSize = rest;
                        draggingItem.UpdateStackSize();
                    }
                    return false;
                }
                else
                {
                    if (!DraggingItem.Item.IsEquipment)
                    {
                        itemFromSlotGameObject.transform.SetParent(DraggingItem.GetComponent<AttachedItem>().LastSlot);
                        itemFromSlotGameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
                        return true;
                    }
                    else
                    {
                        if (itemFromSlot.GetComponent<AttachedItem>().Item.ItemType.Equals(DraggingItem.GetComponent<AttachedItem>().Item.ItemType))
                        {
                            itemFromSlotGameObject.transform.SetParent(DraggingItem.LastSlot);
                            itemFromSlotGameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
                            return true;
                        }
                    }
                }
                return false;
            }
            if (SlotType == SlotType.Equipment)
            {
                if (GetComponent<EquipmentSlot>().EquipmentType.Equals(DraggingItem.Item.ItemType))
                {
                    itemFromSlotGameObject.transform.SetParent(DraggingItem.LastSlot.transform);
                    itemFromSlotGameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
                    return true;
                }
            }
        }
        return false;
    }

    public bool CheckItemFit(Item item, Slot slot)
    {
        return true;
    }
}
