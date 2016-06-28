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

    byte width;
    byte height;

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
        EditorGUILayout.BeginVertical();
        CreateSlots();
        RemoveSlots();
        EditorGUILayout.EndVertical();
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
            if (itemDatabase.Items[i].ItemType == ItemTypeToAdd)
            {
                items[i] = itemDatabase.Items[i].Name;
            }
            else
            {

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
        //EditorGUILayout.BeginHorizontal();
        width = (byte)EditorGUILayout.IntSlider("Width: ", width, 1, byte.MaxValue);
        height = (byte)EditorGUILayout.IntSlider("Height: ", height, 1, byte.MaxValue);
        //EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Create slots"))
        {
            Inventory.Width = width;
            Inventory.Height = height;
            Inventory.CreateSlots(height, width);
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