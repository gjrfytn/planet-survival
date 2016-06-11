using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Slot : MonoBehaviour, IDropHandler {

    public SlotType SlotType;

    [HideInInspector]
    public int SlotNumber;
    public bool ContainsItem;

    GameObject DraggingItem;
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
        DraggingItem = AttachedItem.DraggingItem;
        Item draggingItem = DraggingItem.GetComponent<AttachedItem>().Item;
        Item itemFromSlot = new Item();
        bool sameItem = draggingItem.Name == itemFromSlot.Name;
        if(DraggingItem != null)
        {
            if (GetComponentInChildren<AttachedItem>() == null)
            {
                if (SlotType == SlotType.Inventory)
                {
                    AddItemIntoSlot(DraggingItem);
                }
                if(SlotType == SlotType.Equipment && DraggingItem.GetComponent<AttachedItem>().Item.IsEquipment && GetComponent<EquipmentSlot>().EquipmentType.Equals(DraggingItem.GetComponent<AttachedItem>().Item.ItemType))
                {
                    AddItemIntoSlot(DraggingItem);
                    Inventory.EquipItem(transform.GetComponentInChildren<AttachedItem>().Item);
                }
                if(SlotType == SlotType.Hotbar)
                {
                    DraggingItem.GetComponent<AttachedItem>().CreateDuplicate(DraggingItem);
                    AddItemIntoSlot(DraggingItem);
                    InventoryManager.Stackable(Inventory.Slots);
                }
            }
            else
            {
                if (SlotType == SlotType.Inventory)
                {
                    itemFromSlot = GetComponentInChildren<AttachedItem>().Item;
                    AddItemIntoSlot(DraggingItem);
                }
                if (SlotType == SlotType.Equipment && DraggingItem.GetComponent<AttachedItem>().Item.IsEquipment && GetComponent<EquipmentSlot>().EquipmentType.Equals(DraggingItem.GetComponent<AttachedItem>().Item.ItemType))
                {
                    gameObject.transform.GetChild(0).SetParent(DraggingItem.GetComponent<AttachedItem>().LastSlot);
                    Inventory.UnequipItem(transform.GetComponentInChildren<AttachedItem>().Item);
                    Destroy(transform.GetChild(0).gameObject);
                    AddItemIntoSlot(DraggingItem);
                    Inventory.EquipItem(transform.GetComponentInChildren<AttachedItem>().Item);
                }
            }
        }
    }

    public void AddItemIntoSlot(GameObject itemGameObject)
    {
        itemGameObject.transform.SetParent(gameObject.transform);
        itemGameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
    }

    public bool CheckItemFit(Item item, Slot slot)
    {
        return true;
    }
}
