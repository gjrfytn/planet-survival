using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class BlueprintEditor : BaseInventoryEditor {

    private Blueprint Blueprint = new Blueprint();
    private Blueprint BlueprintToManage;

    private uint CraftItemId;
    private uint CraftItemToAddId;

    private static BlueprintDatabase BlueprintDatabase;

    internal enum SelectAction
    {
        CreateBlueprint,
        ManageBlueprint,
    }

    private SelectAction SelectedAction;

    [MenuItem("Game manager/Inventory/Blueprint editor")]
    private static void Init()
    {
        EditorWindow EditorWindow = GetWindow(typeof(BlueprintEditor));
        EditorWindow.titleContent = new GUIContent("Blueprint editor");
        EditorWindow.minSize = new Vector2(800, 600);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BlueprintDatabase = (BlueprintDatabase)AssetDatabase.LoadAssetAtPath("Assets/Components/Inventory/Extensions/Crafting/Data/BlueprintDatabase.asset", typeof(BlueprintDatabase)) as BlueprintDatabase;
    }

    protected override void OnGUI()
    {
        base.OnGUI();
        //BlueprintDatabase = (BlueprintDatabase)AssetDatabase.LoadAssetAtPath("Assets/Components/Inventory/Extensions/Crafting/Data/BlueprintDatabase.asset", typeof(BlueprintDatabase)) as BlueprintDatabase;
        if (ItemDatabase != null && BlueprintDatabase != null)
        {

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            if (SelectedAction == SelectAction.CreateBlueprint)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            if (GUILayout.Button("Create blueprint"))
            {
                SelectedAction = SelectAction.CreateBlueprint;
                //CraftItem = new CraftedItem();
                //CraftItem.MaterialsId = new List<int>();
            }

            if (SelectedAction == SelectAction.ManageBlueprint)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            if (GUILayout.Button("Edit blueprints"))
            {
                SelectedAction = SelectAction.ManageBlueprint;
            }

            GUILayout.EndHorizontal();

            GUI.color = Color.white;

            GUILayout.Space(25);

            if (SelectedAction == SelectAction.CreateBlueprint)
            {
                CreateBlueprint();
            }
            else if (SelectedAction == SelectAction.ManageBlueprint)
            {
                ManageBlueprint();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(BlueprintDatabase);
                PrefabUtility.SetPropertyModifications(PrefabUtility.GetPrefabObject(BlueprintDatabase), PrefabUtility.GetPropertyModifications(BlueprintDatabase));
            }
        }
        else
        {
            CreateBlueprintDatabase();
        }
    }

    private void CreateBlueprint()
    {
        ScrollManageItem = GUILayout.BeginScrollView(ScrollManageItem);

        List<string> itemNames = new List<string>();

        EditorGUILayout.BeginHorizontal(GUILayout.Width(800));
        //ItemTypeToSearch = (ItemType)EditorGUILayout.EnumPopup("Item type", ItemTypeToSearch);
        foreach (Item item in ItemDatabase)
        {
            itemNames.Add(item.Name);
        }
        GUILayout.Label(ItemDatabase[CraftItemId].Icon.texture, GUILayout.Width(60f), GUILayout.Height(60f));
        CraftItemId = (uint)EditorGUILayout.Popup("Result item: ", (int)CraftItemId, itemNames.ToArray(), GUILayout.Width(300));
		GUILayout.Label("Result item ID: " + /*ItemDatabase[CraftItemId].Id*/CraftItemId.ToString()); //???
        Blueprint.Item = ItemDatabase[CraftItemId];

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(25);

        EditorGUILayout.BeginHorizontal();

        CraftItemToAddId = (uint)EditorGUILayout.Popup("Required item: ", (int)CraftItemToAddId, itemNames.ToArray(), GUILayout.Width(300));
        if (GUILayout.Button("+", GUILayout.Width(25)))
        {
            Blueprint.ItemIds.Add(CraftItemToAddId);
            Blueprint.ItemsForCraft.Add(ItemDatabase[CraftItemToAddId]);
            Blueprint.RequiredAmount.Add(1);
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(25);

        for (int i = 0; i < Blueprint.ItemIds.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(800));

            GUILayout.Label(Blueprint.ItemsForCraft[i].Icon.texture, GUILayout.Width(60f), GUILayout.Height(60f));
            EditorGUILayout.LabelField("Item ID: " + Blueprint.ItemIds[i].ToString());
            EditorGUILayout.LabelField("Name: " + ItemDatabase[Blueprint.ItemIds[i]].Name);
            Blueprint.RequiredAmount[i] = EditorGUILayout.IntSlider(new GUIContent("Amount: "), Blueprint.RequiredAmount[i], 1, ushort.MaxValue);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                Blueprint.ItemIds.Remove(Blueprint.ItemIds[i]);
                Blueprint.ItemsForCraft.Remove(Blueprint.ItemsForCraft[i]);
                Blueprint.RequiredAmount.Remove(Blueprint.RequiredAmount[i]);
            }
            EditorGUILayout.EndHorizontal();
        }
        Blueprint.CraftLevel = (byte)EditorGUILayout.IntSlider(new GUIContent("Craft level: "), Blueprint.CraftLevel, byte.MinValue, byte.MaxValue, GUILayout.Width(600));
        Blueprint.CraftTime = (byte)EditorGUILayout.IntSlider(new GUIContent("Craft time: "), Blueprint.CraftTime, byte.MinValue, byte.MaxValue, GUILayout.Width(600));

        if (GUILayout.Button("Create blueprint"))
        {
            if (Blueprint.ItemIds.Count > 0 && ItemDatabase != null)
            {
                for (uint i = 0; i < BlueprintDatabase.Count; i++)
                {
                    if (BlueprintDatabase[i].Item.Name == Blueprint.Item.Name)
                    {
                        EditorUtility.DisplayDialog("Item already exist.", "The item you're trying to add already exists", "Ok");
                        return;
                    }
                }
                Blueprint.Id = BlueprintDatabase.Count;
                /*for (int i = 0; i < Blueprint.ItemIds.Count; i++)
                {
                    Blueprint.ItemsForCraft.Add(ItemDatabase[Blueprint.ItemIds[i]]);
                }*/
                BlueprintDatabase.Add(Blueprint);
                Blueprint = new Blueprint();
                //UpdateBlueprintDatabase();
                EditorUtility.SetDirty(ItemDatabase);
            }
            else
            {
                EditorUtility.DisplayDialog("Materials error.", "You must add at least 1 material to the list", "Ok");
            }
        }
        GUILayout.EndScrollView();
    }

    private void ManageBlueprint()
    {
        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
        for (uint i = 0; i < BlueprintDatabase.Count; i++)
        {
            if (BlueprintDatabase[i] != BlueprintToManage)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(BlueprintDatabase[i].Item.Name))
                {
                    BlueprintToManage = BlueprintDatabase[i];
                }
                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Remove item", "Are you sure?", "Remove", "Cancel"))
                    {
                        BlueprintDatabase.Remove(i);
                        //UpdateBlueprintDatabase();
                    }
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.indentLevel++;
                GUI.color = Color.gray;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(BlueprintDatabase[i].Item.Name))
                {
                    BlueprintToManage = null;
                    return;
                }
                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Remove item", "Are you sure?", "Remove", "Cancel"))
                    {
                        BlueprintDatabase.Remove(i);
                        //UpdateBlueprintDatabase();
                    }
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                List<string> itemNames = new List<string>();

                for (uint j = 0; j < ItemDatabase.Count; j++)
                {
                    itemNames.Add(ItemDatabase[j].Name);
                }

                GUILayout.BeginHorizontal(GUILayout.Width(800));
				CraftItemId = /*BlueprintToManage.Item.Id*/ItemDatabase.GetID(BlueprintToManage.Item);
                GUILayout.Label(ItemDatabase[CraftItemId].Icon.texture, GUILayout.Width(60f), GUILayout.Height(60f));
                EditorGUILayout.LabelField("Blueprint ID: " + BlueprintToManage.Id.ToString());
                CraftItemId = (uint)EditorGUILayout.Popup("Result item: ", (int)CraftItemId, itemNames.ToArray()); //
                GUILayout.Label("Result item ID: " + CraftItemId.ToString());
                GUILayout.EndHorizontal();
                //SerializedProperty serPropItemIds = SerObj.FindProperty("BlueprintToManage").FindPropertyRelative("ItemIds");
                //SerializedProperty serPropMaterials = SerObj.FindProperty("BlueprintToManage").FindPropertyRelative("ItemsForCraft");
                //serPropMaterials.arraySize = serPropItemIds.arraySize;

                GUILayout.Space(25);

                EditorGUILayout.BeginHorizontal();
                CraftItemToAddId = (uint)EditorGUILayout.Popup("Required item: ", (int)CraftItemToAddId, itemNames.ToArray(), GUILayout.Width(300));
                if (GUILayout.Button("+", GUILayout.Width(25f)))
                {
                    BlueprintToManage.ItemIds.Add(CraftItemToAddId);
                    BlueprintToManage.ItemsForCraft.Add(ItemDatabase[CraftItemToAddId]);
                    BlueprintToManage.RequiredAmount.Add(1);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(25);

                for (int j = 0; j < BlueprintToManage.ItemIds.Count; j++)
                {
                    BlueprintToManage.ItemsForCraft[j] = ItemDatabase[BlueprintToManage.ItemIds[j]];
                    EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(800));

                    GUILayout.Label(BlueprintToManage.ItemsForCraft[j].Icon.texture, GUILayout.Width(60f), GUILayout.Height(60f));
                    EditorGUILayout.LabelField("Item ID: " + BlueprintToManage.ItemIds[j]);
                    EditorGUILayout.LabelField("Name: " + ItemDatabase[BlueprintToManage.ItemIds[j]].Name);
                    BlueprintToManage.RequiredAmount[j] = EditorGUILayout.IntSlider(new GUIContent("Amount: "), BlueprintToManage.RequiredAmount[j], 1, ushort.MaxValue);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        BlueprintToManage.ItemIds.Remove(BlueprintToManage.ItemIds[j]);
                        BlueprintToManage.RequiredAmount.Remove(BlueprintToManage.RequiredAmount[j]);
                        //UpdateBlueprintDatabase();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                BlueprintToManage.CraftLevel = (byte)EditorGUILayout.IntSlider(new GUIContent("Craft level: "), BlueprintToManage.CraftLevel, byte.MinValue, byte.MaxValue, GUILayout.Width(600));
                BlueprintToManage.CraftTime = (byte)EditorGUILayout.IntSlider(new GUIContent("Craft time: "), BlueprintToManage.CraftTime, byte.MinValue, byte.MaxValue, GUILayout.Width(600));

                BlueprintToManage.Item = ItemDatabase[CraftItemId];
                EditorGUI.indentLevel--;
            }
            EditorUtility.SetDirty(ItemDatabase);
        }
        GUILayout.EndScrollView();
    }

    /*void UpdateBlueprintDatabase()
    {
        for (uint i = 0; i < ItemDatabase.Blueprints.Count; i++)
        {
            ItemDatabase.Blueprints[i].Id = i;
        }
    }*/

    private void CreateBlueprintDatabase()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("The blueprint database could not be found");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Create", GUILayout.Width(200)))
        {
            BlueprintDatabase = ScriptableObject.CreateInstance<BlueprintDatabase>();
            AssetDatabase.CreateAsset(BlueprintDatabase, "Assets/Components/Inventory/Extensions/Crafting/Data/BlueprintDatabase.asset");
            AssetDatabase.SaveAssets();
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("The database will be installed in the following directory: Assets/Components/Inventory/Extensions/Crafting/Data/BlueprintDatabase.asset");
        GUILayout.EndHorizontal();
    }
}
