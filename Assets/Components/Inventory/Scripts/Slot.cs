using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler {

    public SlotType SlotType;

    [HideInInspector]
    public int SlotNumber;
    public bool ContainsItem;

    private GameObject DraggingItemGameObject;
    private AttachedItem DraggingItem;
    protected InventoryManager InventoryManager;
    protected Inventory Inventory;

	private void Start ()
    {
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
	}

	private void Update ()
    {
	
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
            DraggingItem.CurrentSlot = gameObject.transform;

            bool addItemIntoSlot = InventoryManager.AddItemIntoSlot(this);
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
                Debug.Log("addItemIntoSlot: " + addItemIntoSlot);
            }
        }
    }
}
