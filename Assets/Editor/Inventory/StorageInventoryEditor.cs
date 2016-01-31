using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[CustomEditor(typeof(StorageInventory))]
public class StorageInventoryEditor : Editor
{

    StorageInventory inv;

    private int itemID;
    private int itemValue = 1;
    private int imageTypeIndex;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void OnEnable()
    {
        inv = target as StorageInventory;

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        addItemGUI();
        serializedObject.ApplyModifiedProperties();
    }

    void addItemGUI()                                   
    {
        inv.setImportantVariables();
        EditorGUILayout.BeginHorizontal();                                                             
		ItemDataBaseList inventoryItemList = (ItemDataBaseList)Resources.Load("Inventory/ItemDatabase");            
        string[] items = new string[inventoryItemList.itemList.Count];                      
        for (int i = 1; i < items.Length; i++)                         
        {
            items[i] = inventoryItemList.itemList[i].itemName;                         
        }
        itemID = EditorGUILayout.Popup("", itemID, items, EditorStyles.popup);           
        itemValue = EditorGUILayout.IntField("", itemValue, GUILayout.Width(40));
        GUI.color = Color.green;                                                              
        if (GUILayout.Button("Add Item"))              
        {
            inv.addItemToStorage(itemID, itemValue);
        }

        EditorGUILayout.EndHorizontal();                                       
    }
}
