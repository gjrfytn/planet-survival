using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor
{
    Inventory Inventory;
    //InventoryManager InventoryManager;

    private int ItemId;
    private int ItemStackSize = 1;
    private ItemType ItemTypeToAdd;

    static void Init()
    {

    }

    void OnEnable()
    {
        Inventory = target as Inventory;

    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label("Items management");
        AddItem();
        EditorGUILayout.Space();
        RemoveItems();
        EditorGUILayout.Space();
        GUILayout.Label("Slots management");
        EditorGUILayout.BeginHorizontal();
        CreateSlots();
        RemoveSlots();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }

    void AddItem()
    {
        ItemDatabase itemDatabase = (ItemDatabase)Resources.Load("Inventory/ItemDatabase", typeof(ItemDatabase)) as ItemDatabase;
        EditorGUILayout.BeginHorizontal();
        ItemTypeToAdd = (ItemType)EditorGUILayout.EnumPopup("Type of item: ", ItemTypeToAdd);
        EditorGUILayout.EndHorizontal();

        string[] items = new string[itemDatabase.Items.Count];
        for (int i = 0; i < items.Length; i++)
        {
            if (itemDatabase.Items[i].ItemType.Equals(ItemTypeToAdd))
            {
                items[i] = itemDatabase.Items[i].Name;
            }
        }

        EditorGUILayout.BeginHorizontal();
        ItemId = EditorGUILayout.Popup("", ItemId, items, EditorStyles.popup);
        ItemStackSize = EditorGUILayout.IntField("", ItemStackSize, GUILayout.Width(40));
        if (ItemStackSize <= 0)
        {
            return;
        }
        GUI.color = Color.green;
        if (GUILayout.Button("Add item"))
        {
            Inventory.AddItem(ItemId, ItemStackSize);
            Inventory.InventoryManager.Stackable(Inventory.Slots);
        }
        Inventory.UpdateItemList();
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
    }


    void RemoveItems()
    {
        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.red;
        if (GUILayout.Button("Remove items"))
        {
            Inventory.RemoveAllItems();
        }
        Inventory.UpdateItemList();
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    void CreateSlots()
    {
        if (GUILayout.Button("Create slots"))
        {
            Inventory.CreateSlots(Inventory.Height, Inventory.Width);
        }
    }

    void RemoveSlots()
    {
        GUI.color = Color.red;
        if (GUILayout.Button("Remove slots"))
        {
            Inventory.RemoveSlots();
        }

        GUI.color = Color.white;
    }

}