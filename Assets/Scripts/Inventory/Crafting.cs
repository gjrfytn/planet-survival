using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Crafting : MonoBehaviour {

    public InventoryManager InventoryManager;
    public Inventory Inventory;
    public ItemDatabase ItemDatabase;
    public CraftedItem SelectedItem;
    public List<CraftedItem> AbleToCraftItems;

    public Button AcceptButton;

    private bool Craft;
    private List<int> MaterialAmounts;

    void Awake()
    {
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        ItemDatabase = InventoryManager.ItemDatabase;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
