using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class CreateItemDatabase
{
    public static ItemDataBaseList asset;          

#if UNITY_EDITOR
    public static ItemDataBaseList createItemDatabase()   
    {
        asset = ScriptableObject.CreateInstance<ItemDataBaseList>();  

        AssetDatabase.CreateAsset(asset, "Assets/Resources/Inventory/ItemDatabase.asset");   
        AssetDatabase.SaveAssets();            
        asset.itemList.Add(new Item());
        return asset;
    }
#endif



}
