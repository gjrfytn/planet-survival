using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PickUpItem : Entity {

    public AttachedItem AttachedItem;

    private InventoryManager InventoryManager;
    private Inventory Inventory;

    //InventoryTooltip Tooltip;
    Player Player;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        //Tooltip = InventoryManager.Tooltip;
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

    void OnMouseUpAsButton()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (Inventory.CheckIfItemAlreadyExist(AttachedItem.Item.Id, AttachedItem.StackSize))
            {
                InventoryEvents.PickUpItem(AttachedItem);
                Destroy(gameObject);
                InventoryManager.UpdateStacks(Inventory.Slots);
            }
            else if (Inventory.ItemsInInventory.Count < Inventory.Width * Inventory.Height)
            {
                InventoryEvents.PickUpItem(AttachedItem);
                Inventory.AddItem(AttachedItem.Item.Id, AttachedItem.StackSize);
                Inventory.UpdateItemList();
                InventoryManager.UpdateStacks(Inventory.Slots);
                Destroy(gameObject);
            }
        }

    }
}
