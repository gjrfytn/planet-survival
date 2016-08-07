using UnityEngine;
using System.Collections;

public static class CraftingEvents {

    public delegate void CraftingDelegate(Blueprint blueprint);

    public static event CraftingDelegate OnItemCraft;



    public static void CraftItem(Blueprint blueprint)
    {
        if (OnItemCraft != null)
        {
            OnItemCraft(blueprint);
        }
    }
}
