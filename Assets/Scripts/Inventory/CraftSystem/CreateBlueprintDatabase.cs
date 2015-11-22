using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class CreateBlueprintDatabase
{
    public static BlueprintDatabase asset;   

#if UNITY_EDITOR
    public static BlueprintDatabase createBlueprintDatabase()  
    {
        asset = ScriptableObject.CreateInstance<BlueprintDatabase>();

        AssetDatabase.CreateAsset(asset, "Assets/InventoryMaster/Resources/BlueprintDatabase.asset");
        AssetDatabase.SaveAssets();                                                       
        asset.blueprints.Add(new Blueprint());
        return asset;
    }
#endif



}
