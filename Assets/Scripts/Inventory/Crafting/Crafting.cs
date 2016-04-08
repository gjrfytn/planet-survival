using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CraftedItem {

	public ItemClass Item;
	public CraftButton Button;
	public CraftingTabType BaseType;

	public List<ItemClass> Materials;
	public List<int> MaterialsID;
	public List<int> MaterialRequiredAmount;

	public string ID;
	public float CraftTime;
	public float CraftTimer;

}

public class Crafting : MonoBehaviour {

	public List<int> CraftAbleItemsIDs;

	public ItemDatabase Database;
	public Tooltip Tooltip;

	public Text AmountToCraftLabel;
	public Sprite ArmorTab;
	public Sprite WeaponTab;
	public Button AcceptButton;

	public float MaterialIconSlotSize;
	public float IconSlotSize;
	public float TabWidth;
	public float TabHeight;

	[HideInInspector]
	public List<CraftButton> Items;
	[HideInInspector]
	public List<CraftedItem> CraftAbleItems;
	[HideInInspector]
	public List<CraftingMaterial> Materials;
	[HideInInspector]
	public Inventory Inventory;
	[HideInInspector]
	public CraftedItem SelectedItem;
	[HideInInspector]
	public CraftingTab SelectedType;
	[HideInInspector]
	public int AmountToCraft = 1;

	private Player Player;
	public List<int> CraftAbleItemIDs;
	private List<int> MaterialAmounts;
	private List<CraftingTab> CraftingTabs;
	private bool IsCrafting;

	// Use this for initialization
	void Start () {
	
		Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		Inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();

		CraftingTabs = new List<CraftingTab>();
		SelectedItem = new CraftedItem();

		for(int i = 0; i < CraftAbleItemIDs.Count; i++)
		{
			CraftedItem Item = Database.FindCraftItem(CraftAbleItemIDs[i]);
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
