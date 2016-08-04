using UnityEngine;
using System.Collections;

public static class CraftingEvents {

    public delegate void CraftingDelegate(Blueprint blueprint);

    public static event CraftingDelegate ItemCrafting;



    public static void CraftItem(Blueprint blueprint)
    {
        ItemCrafting(blueprint);
    }
}
