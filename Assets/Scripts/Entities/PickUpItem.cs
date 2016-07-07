using UnityEngine;

public class PickUpItem : Entity
{
    public Item Item;

    InventoryManager InventoryManager;
    Inventory Inventory;
    //InventoryTooltip Tooltip;

    protected override void Start()
    {
        base.Start();

        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        //Tooltip = InventoryManager.Tooltip;
        World world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();

        if (world.IsCurrentMapLocal())
        {
            Pos = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Pos;
            (world.CurrentMap as LocalMap).AddObject(this);
            transform.position = WorldVisualiser.GetTransformPosFromMapPos(Pos);
        }
    }

    void OnMouseUpAsButton()
    {
        if (Inventory.CheckIfItemAlreadyExist(Item.Id, Item.StackSize))
        {
            InventoryEvents.PickUpItem(Item);
            Destroy(gameObject);
            InventoryManager.Stackable(Inventory.Slots);
        }
        else if (Inventory.ItemsInInventory.Count < Inventory.Width * Inventory.Height)
        {
            InventoryEvents.PickUpItem(Item);
            Inventory.AddItem(Item.Id, Item.StackSize);
            Inventory.UpdateItemList();
            InventoryManager.Stackable(Inventory.Slots);
            Destroy(gameObject);
        }
    }
}
