using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DropItem : MonoBehaviour, IDropHandler {

    InventoryManager InventoryManager;
    //Inventory Inventory;
    GameObject DropPanel;
    GameObject DraggingItem;

    // Use this for initialization
    void Start () {

        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        //Inventory = InventoryManager.Inventory;
        DropPanel = InventoryManager.DropPanel;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnDrop(PointerEventData data)
    {
            DropPanel.SetActive(true);

    }

    public void Accept()
    {
        DraggingItem = AttachedItem.DraggingItem;
        GameObject dropItem = Instantiate(DraggingItem.GetComponent<AttachedItem>().Item.DroppedItem);
            dropItem.AddComponent<PickUpItem>();
            dropItem.GetComponent<PickUpItem>().AttachedItem.Item = DraggingItem.GetComponent<AttachedItem>().Item;
            dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
            //Inventory.UpdateItemList();
            Destroy(DraggingItem);
        DropPanel.SetActive(false);
    }

    public void Cancel()
    {
        DropPanel.SetActive(false);
    }
}
