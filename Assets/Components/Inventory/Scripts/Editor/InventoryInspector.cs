using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Inventory))]
public class InventoryInspector : Editor
{
    private Inventory Inventory;

    private Item Item;
    private int ItemId;
    private int ItemStackSize = 1;
    private ItemType ItemTypeToAdd;
    private string label;
    private byte Width;
    private byte Height;
    private bool AddItemFoldout = false;

    SerializedObject SerializedObject;

    private void OnEnable()
    {
        Inventory = target as Inventory;
        SerializedObject = new SerializedObject(this);
    }


    public override void OnInspectorGUI()
    {
        SerializedObject.Update();
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


    private void AddItem()
    {
        ItemDatabase itemDatabase = (ItemDatabase)AssetDatabase.LoadAssetAtPath("Assets/Components/Inventory/Data/ItemDatabase.asset", typeof(ItemDatabase)) as ItemDatabase;

        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        EditorGUI.indentLevel++;
        AddItemFoldout = Foldout(AddItemFoldout, "Add Item", true, EditorStyles.foldout);
        if (AddItemFoldout)
        {
            EditorGUILayout.BeginHorizontal();
            ItemTypeToAdd = (ItemType)EditorGUILayout.EnumPopup("Type of item: ", ItemTypeToAdd);
            EditorGUILayout.EndHorizontal();
            foreach (Item item in itemDatabase)
            {
                if (item.ItemType == ItemTypeToAdd)
                {
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
                    if (GUILayout.Button("Add", GUILayout.Width(40)))
                    {
						Inventory.AddItem(/*item.Id*/itemDatabase.GetID(item), ItemStackSize);
                        Inventory.InventoryManager.UpdateStacks(Inventory.Slots);
                    }
                    GUI.color = Color.white;
                    //Inventory.UpdateItemList();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }


    private void RemoveItems()
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

    private void CreateSlots()
    {
        //EditorGUILayout.BeginHorizontal();
        Width = (byte)EditorGUILayout.IntSlider("Width: ", Width, 1, byte.MaxValue);
        Height = (byte)EditorGUILayout.IntSlider("Height: ", Height, 1, byte.MaxValue);
        //EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Create slots"))
        {
            Inventory.Width = Width;
            Inventory.Height = Height;
            Inventory.CreateSlots(Height, Width);
        }
    }

    private void RemoveSlots()
    {
        GUI.color = Color.red;
        if (GUILayout.Button("Remove slots"))
        {
            Inventory.RemoveSlots();
        }

        GUI.color = Color.white;
    }



    public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style)
    {
        Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, style);

        return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, style);
    }
    public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick, GUIStyle style)
    {
        return Foldout(foldout, new GUIContent(content), toggleOnLabelClick, style);
    }
}