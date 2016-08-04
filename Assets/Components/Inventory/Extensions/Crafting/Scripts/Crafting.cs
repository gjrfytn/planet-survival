using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Crafting : MonoBehaviour {

    [HideInInspector]
    public InventoryManager InventoryManager;
    [HideInInspector]
    public Inventory Inventory;
    public BlueprintDatabase BlueprintDatabase;
    public Blueprint SelectedBlueprint;
    public List<Ingredient> Ingredients = new List<Ingredient>();
    public Sprite[] CraftingTabs;

    public ItemInfoPanel ItemInfoPanel;

    public Transform BlueprintsContent;
    public Transform IngredientsContent;
    public GameObject BlueprintPrefab;
    public GameObject IngredientPrefab;
    public Button AcceptButton;

    private bool Craft;
    private List<int> MaterialAmounts;

    public List<int> CraftingMaterialCounters = new List<int>();

    void Awake()
    {
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
    }

	// Use this for initialization
	void Start () {

        AddBlueprints();
        AcceptButton.interactable = false;
	
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void AddItem()
    {
        if (Inventory.CheckIfItemAlreadyExist(SelectedBlueprint.Item.Id, 1)) //Blueprint StackSize
        {
            InventoryManager.UpdateStacks(Inventory.Slots);
        }
        else
        {
            Inventory.AddItem(SelectedBlueprint.Item.Id, 1);
        }
    }

    public void AddBlueprints()
    {
        for (uint i = 0; i < BlueprintDatabase.Count; i++)
        {
            GameObject blueprintPrefab = Instantiate(BlueprintPrefab);
            AttachedBlueprint attachedBlueprint = blueprintPrefab.GetComponent<AttachedBlueprint>();
            attachedBlueprint.Blueprint = BlueprintDatabase[i];
            attachedBlueprint.ItemName.text = BlueprintDatabase[i].Item.Name;
            attachedBlueprint.QualityColor.color = InventoryManager.FindColor(BlueprintDatabase[i].Item);
            blueprintPrefab.name = BlueprintDatabase[i].Item.Name;
            blueprintPrefab.transform.SetParent(BlueprintsContent);
        }
    }

    public void UpdateData()
    {
        InventoryManager.Inventory.UpdateItemList();
        CraftingMaterialCounters = new List<int>();
        for(int i = 0; i < SelectedBlueprint.ItemsForCraft.Count; i++)
        {
            int count = 0;
            for(int j = 0; j < Inventory.ItemsInInventory.Count; j++)
            {
                if (Inventory.ItemsInInventory[j].Item.Name == SelectedBlueprint.ItemsForCraft[i].Name /*&& j == Inventory.ItemsInInventory[j].Item.ItemStartNumber*/)
                {
                    count += Inventory.ItemsInInventory[j].GetComponent<AttachedItem>().StackSize;
                }
            }
            CraftingMaterialCounters.Add(count);
        }
        int highestItemAvaliable = 0;
        for (int i = 0; i < CraftingMaterialCounters.Count; i++)
        {
            if(CraftingMaterialCounters[i] != 0 && CraftingMaterialCounters[i] / SelectedBlueprint.RequiredAmount[i] < highestItemAvaliable || (highestItemAvaliable == 0 && i == 0))
            {
                highestItemAvaliable = Mathf.FloorToInt(CraftingMaterialCounters[i] / SelectedBlueprint.RequiredAmount[i]);
            }
            bool avaliable = CraftingMaterialCounters[i] >= SelectedBlueprint.RequiredAmount[i];
            if(!avaliable)
            {
                AcceptButton.interactable = false;
                break;
            }
            else
            {
                AcceptButton.interactable = true;
            }
        }

        /*if(highestItemAvaliable == 0)
        {
            AcceptButton.interactable = false;
        }
        else
        {
            AcceptButton.interactable = true;
        }*/
    }

    public void RemoveMaterials()
    {
        for (int i = 0; i < SelectedBlueprint.ItemsForCraft.Count; i++)
        {
            int stackToRemove = SelectedBlueprint.RequiredAmount[i];
            for (int j = 0; j < Inventory.ItemsInInventory.Count; j++)
            {
                if (Inventory.ItemsInInventory[j].Item.Name == SelectedBlueprint.ItemsForCraft[i].Name)
                {
                    Inventory.UpdateItemList();
                    Inventory.ItemsInInventory[j].GetComponent<AttachedItem>().StackSize -= stackToRemove;
                    if (Inventory.ItemsInInventory[j].GetComponent<AttachedItem>().StackSize == 0)
                    {
                        Inventory.RemoveItemAndGameObject(Inventory.ItemsInInventory[j]);
                    }
                    else if (Inventory.ItemsInInventory[j].GetComponent<AttachedItem>().StackSize < 0)
                    {
                        Inventory.RemoveItemAndGameObject(Inventory.ItemsInInventory[j]);
                        stackToRemove = Mathf.Abs(Inventory.ItemsInInventory[j].GetComponent<AttachedItem>().StackSize);
                    }
                    else
                    {
                        Inventory.ItemsInInventory[j].GetComponent<AttachedItem>().UpdateStackSize();
                        break;
                    }
                }
            }
            UpdateData();
            Ingredients[i].RequiredAmountText.text = SelectedBlueprint.RequiredAmount[i].ToString() + " / " + CraftingMaterialCounters[i];
        }
    }

    public void Accept()
    {
        bool EnoughSpace = Inventory.ItemsInInventory.Count < Inventory.Height * Inventory.Width;
        if (EnoughSpace)
        {
            RemoveMaterials();
            AddItem();
        }
        else
        {
            Debug.Log("Недостаточно места");
            return;
        }
    }
}
