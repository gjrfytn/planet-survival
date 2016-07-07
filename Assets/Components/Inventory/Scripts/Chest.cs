using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chest : Entity {

    public byte ChestSize;
    public byte MinItemsInChest = 1;
    public byte MaxItemsInChest;

    //ItemDatabase ItemDatabase;
    InventoryManager InventoryManager;
    //Inventory Inventory;
    //Tooltip Tooltip;
    //GameObject Player;
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        //Player = GameObject.FindGameObjectWithTag("Player");
        InventoryManager = GameObject.FindGameObjectWithTag("InvetoryManager").GetComponent<InventoryManager>();
        //ItemDatabase = InventoryManager.ItemDatabase;
        //Inventory = InventoryManager.Inventory;

        MaxItemsInChest = ChestSize;

    }


    // Update is called once per frame
    void Update()
    {

    }



}
