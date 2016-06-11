﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour {

    public List<Item> Items;


    public Item FindItemById(int id)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].Id == id)
            {
                return Items[i].ItemCopy();
            }
        }
        return null;
    }

    public void AddItem(Item item)
    {
        Items.Add(item);
    }
}
