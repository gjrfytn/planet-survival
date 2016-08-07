using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class ItemEditor : BaseInventoryEditor {

    //ItemType ItemTypeToCreate;
    EquipmentItemType EquipmentItemTypeToCreate; //Предметы которые можно одеть на персонажа
    OtherItemType OtherItemTypeToCreate; //Любые предметы которые нельзя надеть на персонажа
    ItemType ItemTypeToSearch;
    ItemType CraftItemTypeToAdd;

    Item Item = new Item();
    Item ItemToManage;
    bool OffHandOnly;
    bool IsOther;
    bool EnableAttributes;
    bool IsReturnsItem;
    string AttributeName;
    int AttributeValue;

    string ItemToSearch = string.Empty;

    internal enum SelectAction
    {
        CreateItem,
        ManageItem,
    }

    private SelectAction SelectedAction;


    [MenuItem("Game manager/Inventory/Item editor")]
    static void Init()
    {
        EditorWindow EditorWindow = GetWindow(typeof(ItemEditor));
        EditorWindow.titleContent = new GUIContent("Item editor");
        EditorWindow.minSize = new Vector2(800, 600);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        if (ItemDatabase != null)
        {

            GUILayout.Space(10);

            GUI.color = Color.green;
            GUILayout.BeginHorizontal();

            if (SelectedAction == SelectAction.CreateItem)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            if (GUILayout.Button("Create item"))
            {
                SelectedAction = SelectAction.CreateItem;
            }

            if (SelectedAction == SelectAction.ManageItem)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            if (GUILayout.Button("Edit items"))
            {
                SelectedAction = SelectAction.ManageItem;
            }
            GUILayout.EndHorizontal();

            GUI.color = Color.white;

            GUILayout.Space(25);

            if (SelectedAction == SelectAction.CreateItem)
            {
                CreateItems();
            }
            else if (SelectedAction == SelectAction.ManageItem)
            {
                ManageItems();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(ItemDatabase);
                PrefabUtility.SetPropertyModifications(PrefabUtility.GetPrefabObject(ItemDatabase), PrefabUtility.GetPropertyModifications(ItemDatabase));
            }
        }
        else
        {
            CreateItemDatabase();
        }
    }



}

public partial class ItemEditor
{
    void CreateItems()
    {
        ScrollManageItem = GUILayout.BeginScrollView(ScrollManageItem);

        CommonItemSettings(Item);

        GUILayout.Space(20);
        if (!IsOther)
        {
            Item.IsEquipment = EditorGUILayout.Toggle("Equipment: ", Item.IsEquipment);
        }
        if (!Item.IsEquipment)
        {
            IsOther = EditorGUILayout.Toggle("Other: ", IsOther);
        }

        if (Item.IsEquipment)
        {
            Item.IsStackable = false;
            EquipmentItemTypeToCreate = (EquipmentItemType)EditorGUILayout.EnumPopup("Equipment type: ", EquipmentItemTypeToCreate);
            Item.ItemActionType = (ItemActionType)EditorGUILayout.EnumPopup("Item action type: ", Item.ItemActionType);

            string[] itemTypes = System.Enum.GetNames(typeof(ItemType));

            for (int i = 0; i < itemTypes.Length; i++)
            {
                if (itemTypes[i] == EquipmentItemTypeToCreate.ToString())
                {
                    Item.ItemType = (ItemType)System.Enum.Parse(typeof(ItemType), itemTypes[i]);
                    break;
                }
            }

            if (EquipmentItemTypeToCreate == EquipmentItemType.Weapon)
            {
                WeaponItemSettings(Item);
            }
            else if (EquipmentItemTypeToCreate != EquipmentItemType.Weapon)
            {
                ArmorItemSettings(Item);
            }
        }
        else if (IsOther)
        {

            OtherItemTypeToCreate = (OtherItemType)EditorGUILayout.EnumPopup("Item type: ", OtherItemTypeToCreate);

            //string[] itemTypes = System.Enum.GetNames(typeof(ItemType));

            string[] itemTypes = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Except(System.Enum.GetNames(typeof(ItemType)), System.Enum.GetNames(typeof(EquipmentItemType))));

            //=========================================

            for (int i = 0; i < itemTypes.Length; i++)
            {
                if (itemTypes[i] == OtherItemTypeToCreate.ToString())
                {
                    Item.ItemType = (ItemType)System.Enum.Parse(typeof(ItemType), itemTypes[i]);
                    break;
                }
            }




            Item.IsStackable = EditorGUILayout.Toggle("Stackable: ", Item.IsStackable);

            if (Item.IsStackable)
            {
                Item.MaxStackSize = EditorGUILayout.IntField("Max stack size: ", Item.MaxStackSize);
            }

            EnableAttributes = EditorGUILayout.Toggle("Enable attributes: ", EnableAttributes);
            //SerializedProperty serPropAttributes = SerObj.FindProperty("Item").FindPropertyRelative("ItemAttributes");
            if (EnableAttributes)
            {
                EditorGUILayout.BeginHorizontal();
                AttributeName = EditorGUILayout.TextField("Name: ", AttributeName);
                AttributeValue = EditorGUILayout.IntSlider(new GUIContent("Value: "), AttributeValue, int.MinValue, int.MaxValue);
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    Item.ItemAttributes.Add(new ItemAttribute(AttributeName, AttributeValue));
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(25);
                for (int i = 0; i < Item.ItemAttributes.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Item.ItemAttributes[i].AttributeName = EditorGUILayout.TextField("Name: ", Item.ItemAttributes[i].AttributeName);
                    Item.ItemAttributes[i].AttributeValue = EditorGUILayout.IntSlider(new GUIContent("Value: "), Item.ItemAttributes[i].AttributeValue, int.MinValue, int.MaxValue);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        Item.ItemAttributes.RemoveAt(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            IsReturnsItem = EditorGUILayout.Toggle("Return item: ", IsReturnsItem);

            if (IsReturnsItem)
            {
                string[] items = new string[ItemDatabase.Count];
                uint i = 0;
                foreach (Item item in ItemDatabase)
                    items[i++] = item.Name;

                Item.ReturnItemId = EditorGUILayout.Popup("Returned item: ", Item.ReturnItemId, items, EditorStyles.popup);
                Item.ReturnItemStackSize = EditorGUILayout.IntField("Stack size: ", Item.ReturnItemStackSize);
            }
            else
            {
                Item.ReturnItemId = -1;
            }

        }

        if (GUILayout.Button("Create item"))
        {
            ItemDatabase.Add(Item);
            Item = new Item();
            EditorUtility.SetDirty(ItemDatabase);
        }
        GUILayout.EndScrollView();
    }

    void ManageItems()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        ItemToSearch = GUILayout.TextField(ItemToSearch, EditorStyles.toolbarTextField, GUILayout.Width(400));
        GUILayout.Label("Search in the item database");
        //ItemTypeToSearch = (ItemType)EditorGUILayout.EnumPopup("Filter by type: ", ItemTypeToSearch);
        GUILayout.EndHorizontal();
        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
        foreach (Item item in ItemDatabase)
        {
            if (item != ItemToManage)
            {
                if (item.Name.ToLower().Contains(ItemToSearch.ToLower()) /*&& item.ItemType.Equals(ItemTypeToSearch)*/)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button(item.Name))
                    {
                        ItemToManage = item;
                    }
                    GUI.color = Color.red;

                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        if (EditorUtility.DisplayDialog("Remove item", "Are you sure?", "Remove", "Cancel"))
                        {
                            ItemDatabase.Remove(item.Id);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUI.color = Color.white;
                }
            }
            else
            {
                GUI.color = Color.gray;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(item.Name))
                {
                    ItemToManage = null;
                    return;
                }
                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Remove item", "Are you sure?", "Remove", "Cancel"))
                    {
                        ItemDatabase.Remove(item.Id);
                    }
                }
                GUILayout.EndHorizontal();

                GUI.color = Color.white;

                ScrollManageItem = GUILayout.BeginScrollView(ScrollManageItem);

                EditorGUILayout.LabelField("Item ID: " + ItemToManage.Id.ToString());
                ItemToManage.ItemType = (ItemType)EditorGUILayout.EnumPopup("item type: ", ItemToManage.ItemType);

                CommonItemSettings(ItemToManage);

                if (ItemToManage.IsEquipment)
                {
                    if (ItemToManage.ItemType == ItemType.Weapon)
                    {

                        WeaponItemSettings(ItemToManage);

                    }
                    else
                    {

                        ArmorItemSettings(ItemToManage);
                    }
                }
                else
                {

                    ItemToManage.UseSound = (AudioClip)EditorGUILayout.ObjectField("Use sound: ", ItemToManage.UseSound, typeof(AudioClip), false);

                    ItemToManage.IsStackable = EditorGUILayout.Toggle("Stackable: ", ItemToManage.IsStackable);

                    if (ItemToManage.IsStackable)
                    {
                        ItemToManage.MaxStackSize = EditorGUILayout.IntField("Max stack size: ", ItemToManage.MaxStackSize);
                    }

                    EnableAttributes = EditorGUILayout.Toggle("Enable attributes: ", EnableAttributes);
                    if (EnableAttributes)
                    {
                        EditorGUILayout.BeginHorizontal();
                        AttributeName = EditorGUILayout.TextField("Name: ", AttributeName);
                        AttributeValue = EditorGUILayout.IntSlider(new GUIContent("Value: "), AttributeValue, int.MinValue, int.MaxValue);
                        if (GUILayout.Button("+", GUILayout.Width(25)))
                        {
                            ItemToManage.ItemAttributes.Add(new ItemAttribute(AttributeName, AttributeValue));
                        }
                        EditorGUILayout.EndHorizontal();
                        for (int i = 0; i < ItemToManage.ItemAttributes.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            ItemToManage.ItemAttributes[i].AttributeName = EditorGUILayout.TextField("Name: ", ItemToManage.ItemAttributes[i].AttributeName);
                            ItemToManage.ItemAttributes[i].AttributeValue = EditorGUILayout.IntSlider(new GUIContent("Value: "), ItemToManage.ItemAttributes[i].AttributeValue, int.MinValue, int.MaxValue);
                            if (GUILayout.Button("X", GUILayout.Width(25)))
                            {
                                ItemToManage.ItemAttributes.RemoveAt(i);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                GUILayout.EndScrollView();
            }
            EditorUtility.SetDirty(ItemDatabase);
        }
        GUILayout.EndScrollView();
    }

    void CommonItemSettings(Item item)
    {
        item.Name = EditorGUILayout.TextField("Item name: ", item.Name);

        item.ItemQuality = (ItemQuality)EditorGUILayout.EnumPopup("Item quality: ", item.ItemQuality);

        item.Icon = (Sprite)EditorGUILayout.ObjectField("Icon: ", item.Icon, typeof(Sprite), false);

        item.ItemSound = (AudioClip)EditorGUILayout.ObjectField("Item sound: ", item.ItemSound, typeof(AudioClip), false);

        item.UseSound = (AudioClip)EditorGUILayout.ObjectField("Use sound: ", item.UseSound, typeof(AudioClip), false);

        if (item.DroppedItem == null)
        {
            item.DroppedItem = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Components/Inventory/Prefabs/DefaultDropItem.prefab", typeof(GameObject)) as GameObject;
        }
        item.DroppedItem = (GameObject)EditorGUILayout.ObjectField("Dropped item: ", item.DroppedItem, typeof(GameObject), false);

        item.CustomObject = (GameObject)EditorGUILayout.ObjectField("Custom object: ", item.CustomObject, typeof(GameObject), false);

        item.Cooldown = EditorGUILayout.IntField("Cooldown: ", item.Cooldown);

        item.Description = EditorGUILayout.TextField("Description: ", item.Description);
    }

    void WeaponItemSettings(Item item)
    {
        OffHandOnly = EditorGUILayout.Toggle("OffHand: ", OffHandOnly);
        if (OffHandOnly)
        {
            item.TwoHanded = false;
        }
        if (!OffHandOnly)
        {
            item.TwoHanded = EditorGUILayout.Toggle("Two handed", item.TwoHanded);
        }
        item.LevelReq = (byte)EditorGUILayout.IntField("Item level requirement: ", item.LevelReq);

        GUILayout.BeginHorizontal();

        item.Damage = (byte)EditorGUILayout.IntField("Damage: ", item.Damage);
        item.CriticalDamage = (byte)EditorGUILayout.IntField("CriticalDamage: ", item.CriticalDamage);

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        item.Range = EditorGUILayout.IntField("Range: ", item.Range);
        item.AttackSpeed = EditorGUILayout.FloatField("Attack speed: ", item.AttackSpeed);

        GUILayout.EndHorizontal();
    }

    void ArmorItemSettings(Item item)
    {
        EditorGUILayout.Space();

        item.LevelReq = (byte)EditorGUILayout.IntField("Item level requirement: ", item.LevelReq);

        GUILayout.BeginHorizontal();

        item.Armor = EditorGUILayout.FloatField("Armor: ", item.Armor);
        item.ColdResistance = EditorGUILayout.IntField("Cold resistance: ", item.ColdResistance);
        item.HeatResistance = EditorGUILayout.IntField("Heat resistance: ", item.HeatResistance);

        GUILayout.EndHorizontal();
    }


    void CreateItemDatabase()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("The database could not be found");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Create", GUILayout.Width(200)))
        {
            ItemDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(ItemDatabase, "Assets/Components/Inventory/Data/ItemDatabase.asset");
            AssetDatabase.SaveAssets();
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("The database will be installed in the following directory: Assets/Components/Inventory/Data/ItemDatabase.asset");
        GUILayout.EndHorizontal();
    }
}
