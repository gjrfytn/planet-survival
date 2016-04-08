using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour {

	public List<ItemClass> Items;
	public List<CraftedItem> CraftItems;

	public void AddItem(ItemClass item)
	{
		Items.Add(item);
	}

	public ItemClass FindItem(int id)
	{
		for(int i = 0; id < Items.Count; i++)
		{
			if(int.Parse(Items[i].ID) == id)
			{
				return GenerateItem(Items[i]);
			}
		}
		return new ItemClass();
	}

	public void AddCraftItem(CraftedItem item)
	{
		CraftItems.Add(item);
	}

	public CraftedItem FindCraftItem(int id)
	{
		for(int i = 0; i < CraftItems.Count; i++ )
		{
			if(int.Parse(CraftItems[i].ID) == id)
			{
				return CraftItems[i];
			}
		}
		return new CraftedItem();
	}

	public ItemClass GenerateItem(ItemClass item)
	{
		item = Inventory.DeepCopy(item);

		return item;
	}

}
