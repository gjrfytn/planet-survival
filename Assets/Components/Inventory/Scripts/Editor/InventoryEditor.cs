using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor
{
    Inventory Inventory;
    InventoryManager InventoryManager;

    private Item Item;
    private int ItemId;
    private int ItemStackSize = 1;
    private ItemType ItemTypeToAdd;
    string label;
    byte width;
    byte height;
    bool foldout = false;

    static void Init()
    {

    }

    void OnEnable()
    {
        Inventory = target as Inventory;
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
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

        //string[] items = new string[itemDatabase.Items.Count];
        EditorGUILayout.BeginVertical();
        EditorGUI.indentLevel++;
        foldout = EditorGUILayout.Foldout(foldout, "Add item");
        if (foldout)
        {
            EditorGUILayout.BeginHorizontal();
            ItemTypeToAdd = (ItemType)EditorGUILayout.EnumPopup("Type of item: ", ItemTypeToAdd);
            EditorGUILayout.EndHorizontal();
            foreach (Item item in itemDatabase)
            {
                if (item.ItemType == ItemTypeToAdd)
                {
                    //items[i] = itemDatabase.Items[i].Name;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(item.Name);
                    if (item.MaxStackSize > 1)
                    {
                        ItemStackSize = EditorGUILayout.IntField("", ItemStackSize, GUILayout.Width(40));
                        if (ItemStackSize <= 0)
                        {
                            ItemStackSize = 1;
                        }
                    }
                    GUI.color = Color.green;
                    if (GUILayout.Button("Add item"))
                    {
                        Inventory.AddItem(item.Id, ItemStackSize);
                        Inventory.InventoryManager.Stackable(Inventory.Slots);
                    }
                    GUI.color = Color.white;
                    Inventory.UpdateItemList();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndHorizontal();
        /*EditorGUILayout.BeginHorizontal();
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
        EditorGUILayout.EndHorizontal();*/
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

    void GetItemNames(out List<string> itemNamesList)
    {
        itemNamesList = new List<string>();

    }

}