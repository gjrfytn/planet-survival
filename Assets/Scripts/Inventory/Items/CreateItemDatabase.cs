/*using UnityEngine;
using System.Collections;

using UnityEditor;

public class CreateItemDatabase {

	public static ItemDatabase asset;

	public static ItemDataBase createItemDatabse() 
	{
		asset = ScriptableObject.CreateInstance<ItemDatabase>();

		AssetDatabase.CreateAsset(asset, "Assets/Resources/Inventory/ItemDatabase.asset");
		AssetDatabase.SaveAssets();
		asset.Items.Add(new ItemClass());
		asset.CraftItems.Add(new CraftedItem());
		return asset;
	}
}*/