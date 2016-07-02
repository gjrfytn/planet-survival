using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PickUpItem : Entity {

    public Item Item;

    private InventoryManager InventoryManager;
    private Inventory Inventory;

    InventoryTooltip Tooltip;
    Player Player;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        Tooltip = InventoryManager.Tooltip;
        World world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();

        if (world.IsCurrentMapLocal())
        {
            Entity obj = this;
            LocalMap localMap = world.CurrentMap as LocalMap;
            obj.Pos = Player.Pos;
            localMap.AddObject(obj);
            obj.transform.position = WorldVisualiser.GetTransformPosFromMapPos(obj.Pos);
        }

        //map.AddObject(obj);

	}

	// Update is called once per frame
	void Update() {

	}

    void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0))
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
