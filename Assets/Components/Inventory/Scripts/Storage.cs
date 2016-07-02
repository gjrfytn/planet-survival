using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Storage : MonoBehaviour {


    public List<Item> ItemsInStorage = new List<Item>();

    public int ItemsAmount;


    ItemDatabase ItemDatabase;
    InventoryManager InventoryManager;
    Inventory Inventory;
    //Tooltip Tooltip;
    //GameObject Player;
	// Use this for initialization
	void Start () {

        //Player = GameObject.FindGameObjectWithTag("Player");
        InventoryManager = GameObject.FindGameObjectWithTag("InvetoryManager").GetComponent<InventoryManager>();
        ItemDatabase = InventoryManager.ItemDatabase;
        Inventory = InventoryManager.Inventory;

	}

	
	// Update is called once per frame
	void Update () {
	
	}

    public void AddItem(int id, int stackSize)
    {
        Item item = ItemDatabase.FindItemById(id);
        item.StackSize = stackSize;
        ItemsInStorage.Add(item);
    }
}
