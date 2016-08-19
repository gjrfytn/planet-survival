using UnityEngine;
using UnityEditor;
using System.Collections;

public abstract class BaseInventoryEditor : EditorWindow {

	protected static ItemDatabase<Item> ItemDatabase;

    private Texture2D Logo = null;

    protected SerializedObject SerObj;

    protected Vector2 ScrollPosition;
    protected Vector2 ScrollManageItem;

    protected virtual void OnEnable()
    {
        SerObj = new SerializedObject(this);
        Logo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Components/Inventory/Textures/Logo.png", typeof(Texture2D));
		ItemDatabase = (ItemDatabase<Item>)AssetDatabase.LoadAssetAtPath("Assets/Components/Inventory/Data/ItemDatabase.asset", typeof(ItemDatabase<Item>)) as ItemDatabase<Item>;
    }

    protected virtual void OnGUI()
    {
        SerObj.Update();
        //ItemDatabase = (ItemDatabase)AssetDatabase.LoadAssetAtPath("Assets/Components/Inventory/Data/ItemDatabase.asset", typeof(ItemDatabase)) as ItemDatabase;
        GUILayout.Label(Logo);
        if(ItemDatabase == null)
        {
            GUI.color = Color.red;
            GUILayout.Label("Database disabled");
        }
        else
        {
            GUI.color = Color.green;
            GUILayout.Label("Item database enabled");
            GUI.color = Color.white;
        }
        SerObj.ApplyModifiedProperties();
    }
}
