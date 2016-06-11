using UnityEngine;
using System.Collections;

public class PickUpItem : MonoBehaviour {

    public Item Item;

    private InventoryManager InventoryManager;
    private Inventory Inventory;

	// Use this for initialization
	void Start () {

        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;

	}
	

	// Update is called once per frame
	void Update() {
	
        if(Inventory != null &&  Input.GetKeyUp(KeyCode.E))
        {
            bool check = Inventory.CheckIfItemAlreadyExist(Item.Id, Item.StackSize);
            if(check)
            {
                Destroy(gameObject);
                InventoryManager.Stackable(Inventory.Slots);
            }

            else if(Inventory.ItemsInInventory.Count < (Inventory.Width * Inventory.Height))
            {
                Inventory.AddItem(Item.Id, Item.StackSize);
                Inventory.UpdateItemList();
                InventoryManager.Stackable(Inventory.Slots);
                Destroy(gameObject);
            }
        }

	}
}
