using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemEditor : EditorWindow {

    Item Item = new Item();

    //ItemType ItemTypeToCreate;
    EquipmentItemType EquipmentItemTypeToCreate; //Предметы которые можно одеть на персонажа
    OtherItemType OtherItemTypeToCreate; //Любые предметы которые нельзя надеть на персонажа

    Texture2D Logo = null;

    bool OffHandOnly;
    bool IsOther;
    bool EnableAttributes;
    bool IsReturnsItem;
    int AttributeAmount;
    string ItemToSearch = string.Empty;

    List<Item> ItemBuffer; // TODO

    static ItemDatabase ItemDatabase;

    //bool CreateItem, ManageItem, CreateCraftingItem, ManageCraftingItem;

    Item ItemToManage;

    private enum SelectAction
    {
        CreateItem,
        ManageItem,
        CreateCraftingItem,
        ManageCraftingItem,
    }

    SerializedObject SerObj;

    private Vector2 ScrollPosition;
    private Vector2 ScrollManageItem;

    private SelectAction SelectedAction;


    [MenuItem("Game manager/Inventory/Item editor")]
    static void Init()
    {
        ItemDatabase = (ItemDatabase)Resources.Load("Inventory/ItemDatabase", typeof(ItemDatabase)) as ItemDatabase;
        GetWindow(typeof(ItemEditor));
    }

    void OnEnable()
    {
        SerObj = new SerializedObject(this);
        Logo = (Texture2D)Resources.Load("Inventory/Textures/Logo", typeof(Texture2D));
    }

    void OnInspectorUpdate()
    {
        ItemDatabase = (ItemDatabase)Resources.Load("Inventory/ItemDatabase", typeof(ItemDatabase)) as ItemDatabase;
    }

    void OnGUI()
    {
        SerObj.Update();

        GUILayout.Label(Logo);

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
        if (GUILayout.Button("Create items"))
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

        GUILayout.BeginHorizontal();



        if (SelectedAction == SelectAction.CreateCraftingItem)
        {
            GUI.color = Color.green;
        }
        else
        {
            GUI.color = Color.white;
        }
        if (GUILayout.Button("Create crafted items"))
        {
            SelectedAction = SelectAction.CreateCraftingItem;
            //CraftItem = new CraftedItem();
            //CraftItem.MaterialsId = new List<int>();
        }

        if (SelectedAction == SelectAction.ManageCraftingItem)
        {
            GUI.color = Color.green;
        }
        else
        {
            GUI.color = Color.white;
        }
        if (GUILayout.Button("Edit crafting items"))
        {
            SelectedAction = SelectAction.ManageCraftingItem;
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
        else if (SelectedAction == SelectAction.CreateCraftingItem)
        {
            //CreateCraftingItem();
        }
        else if (SelectedAction == SelectAction.ManageCraftingItem)
        {
           // ManageCraftingItem();
        }

        SerObj.ApplyModifiedProperties();

        if(GUI.changed)
        {
            EditorUtility.SetDirty(ItemDatabase);
            PrefabUtility.SetPropertyModifications(PrefabUtility.GetPrefabObject(ItemDatabase), PrefabUtility.GetPropertyModifications(ItemDatabase));
        }
    }



    void CreateItems()
    {
        ScrollManageItem = GUILayout.BeginScrollView(ScrollManageItem);
        Item.Name = EditorGUILayout.TextField("Item name: ", Item.Name);

        /*EditorGUILayout.BeginHorizontal();
        Item.Width = (byte)EditorGUILayout.IntSlider(new GUIContent("Width: "), Item.Width, 0, 255);
        Item.Height = (byte)EditorGUILayout.IntSlider(new GUIContent("Height: "), Item.Height, 0, 255);
        EditorGUILayout.EndHorizontal();*/

        Item.ItemQuality = (ItemQuality)EditorGUILayout.EnumPopup("Item quality: ", Item.ItemQuality);

        Item.Icon = (Sprite)EditorGUILayout.ObjectField("Icon: ", Item.Icon, typeof(Sprite), false);

        Item.ItemSound = (AudioClip)EditorGUILayout.ObjectField("Item sound: ", Item.ItemSound, typeof(AudioClip), false);

        Item.UseSound = (AudioClip)EditorGUILayout.ObjectField("Use sound: ", Item.UseSound, typeof(AudioClip), false);

        if(Item.DroppedItem == null)
        {
            Item.DroppedItem = (GameObject)Resources.Load("Inventory/DefaultDropItem");
        }
        Item.DroppedItem = (GameObject)EditorGUILayout.ObjectField("Dropped item: ", Item.DroppedItem, typeof(GameObject), false);

        Item.CustomObject = (GameObject)EditorGUILayout.ObjectField("Custom object: ", Item.CustomObject, typeof(GameObject), false);

        Item.Cooldown = EditorGUILayout.IntField("Cooldown: ", Item.Cooldown);

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

                OffHandOnly = EditorGUILayout.Toggle("OffHand: ", OffHandOnly);
                if (OffHandOnly)
                {
                    Item.TwoHanded = false;
                }
                if (!OffHandOnly)
                {
                    Item.TwoHanded = EditorGUILayout.Toggle("Two handed", Item.TwoHanded);
                }
                Item.LevelReq = (byte)EditorGUILayout.IntField("Item level requirement: ", Item.LevelReq);

                GUILayout.BeginHorizontal();

                Item.Damage = (byte)EditorGUILayout.IntField("Damage: ", Item.Damage);
                Item.CriticalDamage = (byte)EditorGUILayout.IntField("CriticalDamage: ", Item.CriticalDamage);

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                Item.Range = EditorGUILayout.IntField("Range: ", Item.Range);
                Item.AttackSpeed = EditorGUILayout.FloatField("Attack speed: ", Item.AttackSpeed);

                GUILayout.EndHorizontal();
            }
            else if (EquipmentItemTypeToCreate != EquipmentItemType.Weapon)
            {
                EditorGUILayout.Space();

                Item.LevelReq = (byte)EditorGUILayout.IntField("Item level requirement: ", Item.LevelReq);

                GUILayout.BeginHorizontal();

                Item.Armor = EditorGUILayout.FloatField("Armor: ", Item.Armor);
                Item.ColdResistance = EditorGUILayout.IntField("Cold resistance: ", Item.ColdResistance);
                Item.HeatResistance = EditorGUILayout.IntField("Heat resistance: ", Item.HeatResistance);

                GUILayout.EndHorizontal();

            }
        }
        else if(IsOther)
        {

            OtherItemTypeToCreate = (OtherItemType)EditorGUILayout.EnumPopup("Item type: ", OtherItemTypeToCreate);

            string[] itemTypes = System.Enum.GetNames(typeof(ItemType));

            for (int i = 0; i < itemTypes.Length; i++)
            {
                if (itemTypes[i] == OtherItemTypeToCreate.ToString())
                {
                    Item.ItemType = (ItemType)System.Enum.Parse(typeof(ItemType), itemTypes[i]);
                    break;
                }
            }


            if (Item.ItemType == ItemType.Potion)
            {
                EditorGUILayout.LabelField("Note: potions are consumables by default", GUILayout.Height(40));
                Item.IsConsumable = true;
                Item.HealthRestore = EditorGUILayout.IntField("Health restore", Item.HealthRestore);
                Item.EnergyRestore = EditorGUILayout.IntField("Energy restore", Item.EnergyRestore);
            }
            else
            {
                Item.HealthRestore = 0;
                Item.EnergyRestore = 0;
            }

            if (Item.ItemType != ItemType.Potion)
            {
                Item.IsConsumable = EditorGUILayout.Toggle(new GUIContent("Consumable: "), Item.IsConsumable);
            }

            Item.IsStackable = EditorGUILayout.Toggle("Stackable: ", Item.IsStackable);

            if (Item.IsStackable)
            {
                Item.MaxStackSize = EditorGUILayout.IntField("Max stack size: ", Item.MaxStackSize);
            }

            EnableAttributes = EditorGUILayout.Toggle("Enable attributes: ", EnableAttributes);
            if (EnableAttributes)
            {
                AttributeAmount = EditorGUILayout.IntSlider("Amount: ", AttributeAmount, 0, 50);

            }
            IsReturnsItem = EditorGUILayout.Toggle("Return item? ", IsReturnsItem);

            if (IsReturnsItem)
            {
                string[] items = new string[ItemDatabase.Items.Count];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = ItemDatabase.Items[i].Name;
                }
                Item.ReturnItemId = EditorGUILayout.Popup("Returned item: ", Item.ReturnItemId, items, EditorStyles.popup);
                Item.ReturnItemStackSize = EditorGUILayout.IntField("Stack size: ", Item.ReturnItemStackSize);
            }
            else
            {
                Item.ReturnItemId = -1;
            }

        }


        Item.Description = EditorGUILayout.TextField("Description: ", Item.Description);

        if (GUILayout.Button("Create item", GUILayout.Height(40)))
        {
                Item.Id = ItemDatabase.Items.Count;
                ItemDatabase.AddItem(Item);
                Item = new Item();
                UpdateDatabase();
        }
        GUILayout.EndScrollView();
    }

    void ManageItems()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        ItemToSearch = GUILayout.TextField(ItemToSearch, EditorStyles.toolbarTextField, GUILayout.Width(400));
        GUILayout.Label("Search in the item database");
        GUILayout.EndHorizontal();
        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
        for (int i = 0; i < ItemDatabase.Items.Count; i++)
        {
            if (ItemDatabase.Items[i] != ItemToManage)
            {
                if (ItemDatabase.Items[i].Name.ToLower().Contains(ItemToSearch.ToLower()))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(">", "label", GUILayout.Width(10)))
                    {
                        ItemToManage = ItemDatabase.Items[i];
                        string[] itemTypes = System.Enum.GetNames(typeof(ItemType));

                        //string[] equipmentItemTypes = System.Enum.GetNames(typeof(ItemType));

                        for (int k = 0; k < itemTypes.Length; k++)
                        {
                            if (itemTypes[k] == EquipmentItemTypeToCreate.ToString())
                            {
                                Item.ItemType = (ItemType)System.Enum.Parse(typeof(ItemType), itemTypes[k]);
                                break;
                            }
                        }
                    }

                    if (GUILayout.Button(ItemDatabase.Items[i].Name))
                    {
                        ItemToManage = ItemDatabase.Items[i];
                    }
                    GUI.color = Color.red;

                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        if (EditorUtility.DisplayDialog("Remove item", "Are you sure?", "Remove", "Cancel"))
                        {
                            ItemDatabase.Items.Remove(ItemDatabase.Items[i]);
                            UpdateDatabase();
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUI.color = Color.white;
                }
            }
            else
            {
                GUI.color = Color.green;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("v", "label", GUILayout.Width(10)))
                {
                    ItemToManage = null;
                    return;
                }
                if (GUILayout.Button(ItemDatabase.Items[i].Name))
                {
                    ItemToManage = null;
                    return;
                }
                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Remove item", "Are you sure?", "Remove", "Cancel"))
                    {
                        ItemDatabase.Items.Remove(ItemDatabase.Items[i]);
                        UpdateDatabase();
                    }
                }
                GUILayout.EndHorizontal();

                GUI.color = Color.white;

                ScrollManageItem = GUILayout.BeginScrollView(ScrollManageItem);

                //ItemToManage.Id = EditorGUILayout.IntField("Item ID: ", ItemToManage.Id); //Не рекомендуется редактировать это значение

                ItemToManage.Name = EditorGUILayout.TextField("Item name: ", ItemToManage.Name);
                ItemToManage.ItemType = (ItemType)EditorGUILayout.EnumPopup("Item type: ", ItemToManage.ItemType);
                ItemToManage.ItemActionType = (ItemActionType)EditorGUILayout.EnumPopup("Item action type: ", ItemToManage.ItemActionType);
                /*EditorGUILayout.BeginHorizontal();
                ItemToManage.Width = (byte)EditorGUILayout.IntSlider("Item width: ", ItemToManage.Width, 0, 255);
                ItemToManage.Height = (byte)EditorGUILayout.IntSlider("Item height: ", ItemToManage.Height, 0, 255);
                EditorGUILayout.EndHorizontal();*/
                ItemToManage.ItemQuality = (ItemQuality)EditorGUILayout.EnumPopup("Item quality: ", ItemToManage.ItemQuality);
                ItemToManage.Icon = (Sprite)EditorGUILayout.ObjectField("Icon: ", ItemToManage.Icon, typeof(Sprite), false);
                ItemToManage.ItemSound = (AudioClip)EditorGUILayout.ObjectField("Item sound: ", ItemToManage.ItemSound, typeof(AudioClip), false);
                ItemToManage.DroppedItem = (GameObject)EditorGUILayout.ObjectField("Model: ", ItemToManage.DroppedItem, typeof(GameObject), false);
                ItemToManage.CustomObject = (GameObject)EditorGUILayout.ObjectField("Custom object: ", ItemToManage.CustomObject, typeof(GameObject), false);

                if (ItemToManage.IsEquipment)
                {
                    if (ItemToManage.ItemType == ItemType.Weapon)
                    {
                        OffHandOnly = EditorGUILayout.Toggle("OffHand: ", OffHandOnly);
                        if (OffHandOnly)
                        {
                            ItemToManage.TwoHanded = false;
                        }
                        if (!OffHandOnly)
                        {
                            ItemToManage.TwoHanded = EditorGUILayout.Toggle("Two handed", ItemToManage.TwoHanded);
                        }
                        ItemToManage.LevelReq = (byte)EditorGUILayout.IntField("Item level requirement: ", ItemToManage.LevelReq);

                        GUILayout.BeginHorizontal();

                        ItemToManage.Damage = (byte)EditorGUILayout.IntField("Damage: ", ItemToManage.Damage);
                        ItemToManage.CriticalDamage = (byte)EditorGUILayout.IntField("CriticalDamage: ", ItemToManage.CriticalDamage);

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        ItemToManage.Range = EditorGUILayout.IntField("Range: ", ItemToManage.Range);
                        ItemToManage.AttackSpeed = EditorGUILayout.FloatField("Attack speed: ", ItemToManage.AttackSpeed);

                        GUILayout.EndHorizontal();
                    }
                    else if (ItemToManage.ItemType != ItemType.Weapon)
                    {
                        ItemToManage.LevelReq = (byte)EditorGUILayout.IntField("Item level requirement: ", ItemToManage.LevelReq);

                        GUILayout.BeginHorizontal();
                        ItemToManage.Armor = EditorGUILayout.FloatField("Armor: ", ItemToManage.Armor);
                        ItemToManage.ColdResistance = EditorGUILayout.IntField("Cold resistance: ", ItemToManage.ColdResistance);
                        ItemToManage.HeatResistance = EditorGUILayout.IntField("Heat resistance: ", ItemToManage.HeatResistance);
                        GUILayout.EndHorizontal();
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
                        AttributeAmount = EditorGUILayout.IntSlider("Amount: ", AttributeAmount, 0, 50);

                    }
                }
                ItemToManage.Description = EditorGUILayout.TextField("Description: ", ItemToManage.Description);
                GUILayout.EndScrollView();
            }
        }
        GUILayout.EndScrollView();
    }


    void UpdateDatabase()
    {
        for (int i = 0; i < ItemDatabase.Items.Count; i++)
        {
            ItemDatabase.Items[i].Id = i;
        }
    }
   /* void UpdateCraftDatabase()
    {
        for (int i = 0; i < ItemDatabase.CraftItems.Count; i++)
        {
            ItemDatabase.CraftItems[i].Id = i.ToString();
        }
    }*/

}
