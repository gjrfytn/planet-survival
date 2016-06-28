using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PickUpItem : Entity, IPointerClickHandler {

    public Item Item;

    private InventoryManager InventoryManager;
    private Inventory Inventory;
    Player Player;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        Pos = Player.Pos;

	}

	// Update is called once per frame
	void Update() {
	
        if(Input.GetKeyUp(KeyCode.E))
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

    public void OnPointerClick(PointerEventData data)
    {
        if(data.button == PointerEventData.InputButton.Left)
        {
            bool check = Inventory.CheckIfItemAlreadyExist(Item.Id, Item.StackSize);
            if (check)
            {
                Destroy(gameObject);
                InventoryManager.Stackable(Inventory.Slots);
            }

            else if (Inventory.ItemsInInventory.Count < (Inventory.Width * Inventory.Height))
            {
                Inventory.AddItem(Item.Id, Item.StackSize);
                Inventory.UpdateItemList();
                InventoryManager.Stackable(Inventory.Slots);
                Destroy(gameObject);
            }
        }
    }
}
