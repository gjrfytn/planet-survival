using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;


public class Inventory_Manager : EditorWindow
{
    [MenuItem("Game Modules/Inventory Module/Inventory Manager")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(Inventory_Manager));

		Object itemDatabase = Resources.Load("Inventory/ItemDatabase");
        if (itemDatabase == null)
            inventoryItemList = CreateItemDatabase.createItemDatabase();
        else
			inventoryItemList = (ItemDataBaseList)Resources.Load("Inventory/ItemDatabase");

		Object attributeDatabase = Resources.Load("Inventory/AttributeDatabase");
        if (attributeDatabase == null)
            itemAttributeList = CreateAttributeDatabase.createItemAttributeDatabase();
        else
			itemAttributeList = (ItemAttributeList)Resources.Load("Inventory/AttributeDatabase");

		Object inputManager = Resources.Load("Inventory/InputManager");
        if (inputManager == null)
            inputManagerDatabase = CreateInputManager.createInputManager();
        else
			inputManagerDatabase = (InputManager)Resources.Load("Inventory/InputManager");


    }


    bool showInputManager;
    bool showItemDataBase;
    bool showBluePrintDataBase;

    //Itemdatabase
    static ItemDataBaseList inventoryItemList = null;
    static ItemAttributeList itemAttributeList = null;
    static InputManager inputManagerDatabase = null;
    List<bool> manageItem = new List<bool>();

    Vector2 scrollPosition;

    static KeyCode test;

    bool showItemAttributes;
    string addAttributeName = "";
    int attributeAmount = 1;
    int[] attributeName;
    int[] attributeValue;

    int[] attributeNamesManage = new int[100];
    int[] attributeValueManage = new int[100];
    int attributeAmountManage;

    bool showItem;

    public int toolbarInt = 0;
    public string[] toolbarStrings = new string[] { "Create Items", "Manage Items" };

    //Blueprintdatabase
    static BlueprintDatabase bluePrintDatabase = null;
    List<bool> manageItem1 = new List<bool>();
    int amountOfFinalItem;
    //    float timeToCraft;
    int finalItemID;
    int amountofingredients;
    int[] ingredients;
    int[] amount;
    ItemDataBaseList itemdatabase;
    Vector2 scrollPosition1;

    public int toolbarInt1 = 0;
    public string[] toolbarStrings1 = new string[] { "Create Blueprints", "Manage Blueprints" };

    void OnGUI()
    {
        Header();

        if (GUILayout.Button("Input Manager"))
        {
            showInputManager = !showInputManager;
            showItemDataBase = false;
            showBluePrintDataBase = false;
        }

        if (showInputManager)
            InputManager1();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Itemdatabase"))
        {
            showInputManager = false;
            showItemDataBase = !showItemDataBase;
            showBluePrintDataBase = false;
        }

        if (GUILayout.Button("Blueprintdatabase"))
        {
            showInputManager = false;
            showItemDataBase = false;
            showBluePrintDataBase = !showBluePrintDataBase;
        }
        EditorGUILayout.EndHorizontal();

        if (showItemDataBase)
            ItemDataBase();

        if (showBluePrintDataBase)
            BluePrintDataBase();

    }



    void InputManager1()
    {
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label("┌─Inputs", EditorStyles.boldLabel);

        EditorUtility.SetDirty(inputManagerDatabase);

        inputManagerDatabase.InventoryKeyCode = (KeyCode)EditorGUILayout.EnumPopup("Inventory", (KeyCode)inputManagerDatabase.InventoryKeyCode);
        inputManagerDatabase.StorageKeyCode = (KeyCode)EditorGUILayout.EnumPopup("Storage", (KeyCode)inputManagerDatabase.StorageKeyCode);
        inputManagerDatabase.CharacterSystemKeyCode = (KeyCode)EditorGUILayout.EnumPopup("Charactersystem", (KeyCode)inputManagerDatabase.CharacterSystemKeyCode);
        inputManagerDatabase.CraftSystemKeyCode = (KeyCode)EditorGUILayout.EnumPopup("Craftsystem", (KeyCode)inputManagerDatabase.CraftSystemKeyCode);
        inputManagerDatabase.SplitItem = (KeyCode)EditorGUILayout.EnumPopup("Split", (KeyCode)inputManagerDatabase.SplitItem);


        EditorUtility.SetDirty(inputManagerDatabase);

        EditorGUILayout.EndVertical();
    }

    void ItemDataBase()
    {
        EditorGUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings, GUILayout.Width(position.width - 18));                                                    //creating a toolbar(tabs) to navigate what you wanna do
        GUILayout.EndHorizontal();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Space(10);

        if (toolbarInt == 0)
        {
            GUI.color = Color.green;
            if (GUILayout.Button("Add Item", GUILayout.Width(position.width - 23)))
            {
                addItem();
                showItem = true;
            }

            if (showItem)
            {
                GUI.color = Color.white;

                GUILayout.BeginVertical("Box", GUILayout.Width(position.width - 23));
                try
                {
                    inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemName = EditorGUILayout.TextField("Item Name", inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemName, GUILayout.Width(position.width - 30));
                    inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemID = inventoryItemList.itemList.Count - 1; 

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Item Description");
                    GUILayout.Space(47);
                    inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemDesc = EditorGUILayout.TextArea(inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemDesc, GUILayout.Width(position.width - 180), GUILayout.Height(70));
                    GUILayout.EndHorizontal();
                    inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemIcon = (Sprite)EditorGUILayout.ObjectField("Item Icon", inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemIcon, typeof(Sprite), false, GUILayout.Width(position.width - 33));
                    inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemModel = (GameObject)EditorGUILayout.ObjectField("Item Model", inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemModel, typeof(GameObject), false, GUILayout.Width(position.width - 33));

                    inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemType, GUILayout.Width(position.width - 33));
                    inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].maxStack = EditorGUILayout.IntField("Max Stack", inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].maxStack, GUILayout.Width(position.width - 33));
                    inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].rarity = EditorGUILayout.IntSlider("Rarity", inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].rarity, 0, 100);
                    GUILayout.BeginVertical("Box", GUILayout.Width(position.width - 33));
                    showItemAttributes = EditorGUILayout.Foldout(showItemAttributes, "Item attributes");
                    if (showItemAttributes)
                    {
                        GUILayout.BeginHorizontal();
                        addAttributeName = EditorGUILayout.TextField("Name", addAttributeName);
                        GUI.color = Color.green;
                        if (GUILayout.Button("Add"))
                            addAttribute();
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);
                        GUI.color = Color.white;
                        EditorGUI.BeginChangeCheck();
                        attributeAmount = EditorGUILayout.IntSlider("Amount", attributeAmount, 0, 50);
                        if (EditorGUI.EndChangeCheck())
                        {
                            attributeName = new int[attributeAmount];
                            attributeValue = new int[attributeAmount];
                        }

                        string[] attributes = new string[itemAttributeList.itemAttributeList.Count];
                        for (int i = 1; i < attributes.Length; i++)
                        {
                            attributes[i] = itemAttributeList.itemAttributeList[i].attributeName;
                        }


                        for (int k = 0; k < attributeAmount; k++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            attributeName[k] = EditorGUILayout.Popup("Attribute " + (k + 1), attributeName[k], attributes, EditorStyles.popup);
                            attributeValue[k] = EditorGUILayout.IntField("Value", attributeValue[k]);
                            EditorGUILayout.EndHorizontal();
                        }
                        if (GUILayout.Button("Save"))
                        {
                            List<ItemAttribute> iA = new List<ItemAttribute>();
                            for (int i = 0; i < attributeAmount; i++)
                            {
                                iA.Add(new ItemAttribute(attributes[attributeName[i]], attributeValue[i]));
                            }
                            inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].itemAttributes = iA;
                            showItem = false;

                        }

                    }
                    GUILayout.EndVertical();
                    inventoryItemList.itemList[inventoryItemList.itemList.Count - 1].indexItemInList = 999;



                }
                catch { }
                GUILayout.EndVertical();
            }

        }

        if (toolbarInt == 1)
        {
            if (inventoryItemList == null)
				inventoryItemList = (ItemDataBaseList)Resources.Load("Inventory/ItemDatabase");
            if (inventoryItemList.itemList.Count == 1)
            {
                GUILayout.Label("There is no Item in the Database!");
            }
            else
            {
                GUILayout.BeginVertical();
                for (int i = 1; i < inventoryItemList.itemList.Count; i++)
                {
                    try
                    {
                        manageItem.Add(false);
                        GUILayout.BeginVertical("Box");
                        manageItem[i] = EditorGUILayout.Foldout(manageItem[i], "" + inventoryItemList.itemList[i].itemName);
                        if (manageItem[i])
                        {

                            EditorUtility.SetDirty(inventoryItemList);                                                                                                
                            GUI.color = Color.red;
                            if (GUILayout.Button("Delete Item"))
                            {
                                inventoryItemList.itemList.RemoveAt(i);
                                EditorUtility.SetDirty(inventoryItemList);
                            }

                            GUI.color = Color.white;
                            inventoryItemList.itemList[i].itemName = EditorGUILayout.TextField("Item Name", inventoryItemList.itemList[i].itemName, GUILayout.Width(position.width - 45));
                            inventoryItemList.itemList[i].itemID = i; 
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Item ID");
                            GUILayout.Label("" + i);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Item Description");
                            GUILayout.Space(47);
                            inventoryItemList.itemList[i].itemDesc = EditorGUILayout.TextArea(inventoryItemList.itemList[i].itemDesc, GUILayout.Width(position.width - 195), GUILayout.Height(70));
                            GUILayout.EndHorizontal();
                            inventoryItemList.itemList[i].itemIcon = (Sprite)EditorGUILayout.ObjectField("Item Icon", inventoryItemList.itemList[i].itemIcon, typeof(Sprite), false, GUILayout.Width(position.width - 45));
                            inventoryItemList.itemList[i].itemModel = (GameObject)EditorGUILayout.ObjectField("Item Model", inventoryItemList.itemList[i].itemModel, typeof(GameObject), false, GUILayout.Width(position.width - 45));
                            inventoryItemList.itemList[i].itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", inventoryItemList.itemList[i].itemType, GUILayout.Width(position.width - 45));
                            inventoryItemList.itemList[i].maxStack = EditorGUILayout.IntField("Max Stack", inventoryItemList.itemList[i].maxStack, GUILayout.Width(position.width - 45));
                            inventoryItemList.itemList[i].rarity = EditorGUILayout.IntSlider("Rarity", inventoryItemList.itemList[i].rarity, 0, 100);
                            GUILayout.BeginVertical("Box", GUILayout.Width(position.width - 45));
                            showItemAttributes = EditorGUILayout.Foldout(showItemAttributes, "Item attributes");
                            if (showItemAttributes)
                            {

                                string[] attributes = new string[itemAttributeList.itemAttributeList.Count];
                                for (int t = 1; t < attributes.Length; t++)
                                {
                                    attributes[t] = itemAttributeList.itemAttributeList[t].attributeName;
                                }


                                if (inventoryItemList.itemList[i].itemAttributes.Count != 0)
                                {
                                    for (int t = 0; t < inventoryItemList.itemList[i].itemAttributes.Count; t++)
                                    {
                                        for (int z = 1; z < attributes.Length; z++)
                                        {
                                            if (inventoryItemList.itemList[i].itemAttributes[t].attributeName == attributes[z])
                                            {
                                                attributeNamesManage[t] = z;
                                                attributeValueManage[t] = inventoryItemList.itemList[i].itemAttributes[t].attributeValue;
                                                break;
                                            }
                                        }
                                    }
                                }

                                for (int z = 0; z < inventoryItemList.itemList[i].itemAttributes.Count; z++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUI.color = Color.red;
                                    if (GUILayout.Button("-"))
                                        inventoryItemList.itemList[i].itemAttributes.RemoveAt(z);
                                    GUI.color = Color.white;
                                    attributeNamesManage[z] = EditorGUILayout.Popup(attributeNamesManage[z], attributes, EditorStyles.popup);
                                    inventoryItemList.itemList[i].itemAttributes[z].attributeValue = EditorGUILayout.IntField("Value", inventoryItemList.itemList[i].itemAttributes[z].attributeValue);
                                    EditorGUILayout.EndHorizontal();
                                }
                                GUI.color = Color.green;
                                if (GUILayout.Button("+"))
                                    inventoryItemList.itemList[i].itemAttributes.Add(new ItemAttribute());




                                GUI.color = Color.white;
                                if (GUILayout.Button("Save"))
                                {
                                    List<ItemAttribute> iA = new List<ItemAttribute>();
                                    for (int k = 0; k < inventoryItemList.itemList[i].itemAttributes.Count; k++)
                                    {
                                        iA.Add(new ItemAttribute(attributes[attributeNamesManage[k]], attributeValueManage[k]));
                                    }
                                    inventoryItemList.itemList[i].itemAttributes = iA;

                                    GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
                                    for (int z = 0; z < items.Length; z++)
                                    {
                                        ItemOnObject item = items[z].GetComponent<ItemOnObject>();
                                        if (item.item.itemID == inventoryItemList.itemList[i].itemID)
                                        {
                                            int value = item.item.itemValue;
                                            item.item = inventoryItemList.itemList[i];
                                            item.item.itemValue = value;
                                        }
                                    }

                                    manageItem[i] = false;
                                }



                            }
                            GUILayout.EndVertical();

                            EditorUtility.SetDirty(inventoryItemList);
                        }
                        GUILayout.EndVertical();
                    }
                    catch { }

                }
                GUILayout.EndVertical();

            }



        }
        if (inventoryItemList == null)
			inventoryItemList = (ItemDataBaseList)Resources.Load("Inventory/ItemDatabase");

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

    }

    void BluePrintDataBase()
    {
        EditorGUILayout.BeginVertical("Box");
        if (inventoryItemList == null)
			inventoryItemList = (ItemDataBaseList)Resources.Load("Inventory/ItemDatabase");
        if (bluePrintDatabase == null)
			bluePrintDatabase = (BlueprintDatabase)Resources.Load("Inventory/BlueprintDatabase");

        GUILayout.BeginHorizontal();
        toolbarInt1 = GUILayout.Toolbar(toolbarInt1, toolbarStrings1, GUILayout.Width(position.width - 20));
        GUILayout.EndHorizontal();
        scrollPosition1 = EditorGUILayout.BeginScrollView(scrollPosition1);
        GUILayout.Space(10);

        if (toolbarInt1 == 0)
        {
            GUI.color = Color.white;
            try
            {
                GUILayout.BeginVertical("Box");
                string[] items = new string[inventoryItemList.itemList.Count];
                for (int i = 1; i < items.Length; i++)
                {
                    items[i] = inventoryItemList.itemList[i].itemName;
                }
                EditorGUILayout.BeginHorizontal();
                finalItemID = EditorGUILayout.Popup("Final Item", finalItemID, items, EditorStyles.popup);
                amountOfFinalItem = EditorGUILayout.IntField("Value", amountOfFinalItem);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                amountofingredients = EditorGUILayout.IntSlider("Ingredients", amountofingredients, 1, 50, GUILayout.Width(position.width - 38));
                if (EditorGUI.EndChangeCheck())
                {
                    ingredients = new int[amountofingredients];
                    amount = new int[amountofingredients];
                }
                for (int i = 0; i < amountofingredients; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    ingredients[i] = EditorGUILayout.Popup("Ingredient " + i, ingredients[i], items, EditorStyles.popup, GUILayout.Width((position.width / 2) - 20));
                    amount[i] = EditorGUILayout.IntField("Value", amount[i], GUILayout.Width((position.width / 2) - 20));

                    EditorGUILayout.EndHorizontal();
                }
                GUI.color = Color.green;
                if (GUILayout.Button("Add Blueprint", GUILayout.Width(position.width - 35), GUILayout.Height(50)))
                    addBlueprint();

                GUILayout.EndVertical();

            }
            catch { }

        }

        if (toolbarInt1 == 1)
        {

            if (bluePrintDatabase == null)
            {
				bluePrintDatabase = (BlueprintDatabase)Resources.Load("Inventory/BlueprintDatabase");
                if (bluePrintDatabase == null)
                {
                    bluePrintDatabase = CreateBlueprintDatabase.createBlueprintDatabase();
					bluePrintDatabase = (BlueprintDatabase)Resources.Load("Inventory/BlueprintDatabase");
                }
            }

            if (bluePrintDatabase.blueprints.Count == 1)
            {
                GUILayout.Label("There is no Blueprint in the Database!");
            }
            else
            {
                GUILayout.BeginVertical();
                for (int i = 1; i < bluePrintDatabase.blueprints.Count; i++)
                {
                    try
                    {
                        manageItem1.Add(false);
                        GUILayout.BeginVertical("Box", GUILayout.Width(position.width - 23));
                        manageItem1[i] = EditorGUILayout.Foldout(manageItem1[i], "" + bluePrintDatabase.blueprints[i].finalItem.itemName);
                        if (manageItem1[i])
                        {
                            EditorGUI.indentLevel++;
                            EditorUtility.SetDirty(bluePrintDatabase);                                                                                               
                            GUI.color = Color.red;
                            if (GUILayout.Button("Delete Blueprint", GUILayout.Width(position.width - 38)))
                            {
                                bluePrintDatabase.blueprints.RemoveAt(i);
                                EditorUtility.SetDirty(bluePrintDatabase);
                            }

                            GUI.color = Color.white;
                            EditorUtility.SetDirty(bluePrintDatabase);
                            bluePrintDatabase.blueprints[i].amountOfFinalItem = EditorGUILayout.IntField("Amount of final items", bluePrintDatabase.blueprints[i].amountOfFinalItem, GUILayout.Width(position.width - 35));
                            //bluePrintDatabase.blueprints[i].timeToCraft = EditorGUILayout.FloatField("Time to craft", bluePrintDatabase.blueprints[i].timeToCraft);
                            EditorUtility.SetDirty(bluePrintDatabase);
                            string[] items = new string[inventoryItemList.itemList.Count];
                            for (int z = 1; z < items.Length; z++)
                            {
                                items[z] = inventoryItemList.itemList[z].itemName;
                            }
                            GUILayout.Label("Ingredients");
                            for (int k = 0; k < bluePrintDatabase.blueprints[i].ingredients.Count; k++)
                            {
                                GUILayout.BeginHorizontal();
                                GUI.color = Color.red;
                                if (GUILayout.Button("-"))
                                    bluePrintDatabase.blueprints[i].ingredients.RemoveAt(k);
                                GUI.color = Color.white;
                                bluePrintDatabase.blueprints[i].ingredients[k] = EditorGUILayout.Popup("Ingredient " + (k + 1), bluePrintDatabase.blueprints[i].ingredients[k], items, EditorStyles.popup);
                                bluePrintDatabase.blueprints[i].amount[k] = EditorGUILayout.IntField("Value", bluePrintDatabase.blueprints[i].amount[k]);

                                GUILayout.EndHorizontal();
                            }

                            GUI.color = Color.green;
                            if (GUILayout.Button("+"))
                            {
                                bluePrintDatabase.blueprints[i].ingredients.Add(0);
                                bluePrintDatabase.blueprints[i].amount.Add(0);
                            }
                            GUI.color = Color.white;
                            EditorGUI.indentLevel--;
                            EditorUtility.SetDirty(bluePrintDatabase);
                        }
                        GUILayout.EndVertical();
                    }
                    catch { }
                }
                GUILayout.EndVertical();
            }

        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    void Header()
    {
        GUILayout.BeginHorizontal();


        GUILayout.BeginVertical();
        GUILayout.Space(10);

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();


        EditorGUI.BeginChangeCheck();
        if (inputManagerDatabase == null)
			inputManagerDatabase = (InputManager)Resources.Load("Inventory/InputManager");
    }

    GameObject itemPrefab;
    GameObject hotbarPrefab;
	

    void addItem()
    {
        EditorUtility.SetDirty(inventoryItemList);
        Item newItem = new Item();
        newItem.itemName = "New Item";
        inventoryItemList.itemList.Add(newItem);
        EditorUtility.SetDirty(inventoryItemList);
    }

    void addAttribute()
    {
        EditorUtility.SetDirty(itemAttributeList);
        ItemAttribute newAttribute = new ItemAttribute();
        newAttribute.attributeName = addAttributeName;
        itemAttributeList.itemAttributeList.Add(newAttribute);
        addAttributeName = "";
        EditorUtility.SetDirty(itemAttributeList);
    }

    void addBlueprint()
    {
        EditorUtility.SetDirty(bluePrintDatabase);
        Blueprint newBlueprint = new Blueprint(ingredients.ToList<int>(), amountOfFinalItem, amount.ToList<int>(), inventoryItemList.getItemByID(finalItemID));      
        bluePrintDatabase.blueprints.Add(newBlueprint);       
        EditorUtility.SetDirty(bluePrintDatabase);
    }

}