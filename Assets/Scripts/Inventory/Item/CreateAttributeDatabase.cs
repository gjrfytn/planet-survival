using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public class CreateAttributeDatabase : MonoBehaviour
{

    public static ItemAttributeList asset;

#if UNITY_EDITOR
    public static ItemAttributeList createItemAttributeDatabase() 
    {
        asset = ScriptableObject.CreateInstance<ItemAttributeList>();  

        AssetDatabase.CreateAsset(asset, "Assets/Resources/Inventory/AttributeDatabase.asset"); 
        AssetDatabase.SaveAssets();     
        return asset;
    }
#endif

}
