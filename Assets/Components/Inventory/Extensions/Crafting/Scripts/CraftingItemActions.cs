using UnityEngine;

public class CraftingItemActions : MonoBehaviour {

    InventoryManager InventoryManager;
    Inventory Inventory;
    Crafting Crafting;

    
    private void OnEnable()
    {
        CraftingEvents.OnItemCraft += CraftItem;
    }

    private void OnDisable()
    {
        CraftingEvents.OnItemCraft -= CraftItem;
    }

    private void Awake()
    {
        InventoryManager = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        Crafting = InventoryManager.Crafting;
    }

    private void CraftItem(Blueprint blueprint)
    {
        bool EnoughSpace = Inventory.ItemsInInventory.Count < Inventory.Height * Inventory.Width;
        if (EnoughSpace)
        {
            Crafting.RemoveMaterials();
            Crafting.AddItem();
        }
        else
        {
            Debug.Log("Недостаточно места");
            return;
        }
    }

}
