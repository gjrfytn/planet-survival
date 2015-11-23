using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDataBaseList : ScriptableObject
{   

    [SerializeField]
    public List<Item> itemList = new List<Item>();  

    public Item getItemByID(int id)
    {
		Item tmpItem=itemList.Find(item=>item.itemID==id);

		return tmpItem==null ? null : tmpItem.getCopy();
    }

    public Item getItemByName(string name)
    {
		Item tmpItem=itemList.Find(item=>item.itemName.ToLower()==name.ToLower());

		return tmpItem==null ? null : tmpItem.getCopy();
    }
}
