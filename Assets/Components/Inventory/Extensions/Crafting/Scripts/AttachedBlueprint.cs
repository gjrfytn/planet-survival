using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class AttachedBlueprint : MonoBehaviour, IPointerClickHandler {

    public Text ItemName;
    public Image QualityColor;
    public Blueprint Blueprint;
    InventoryManager InventoryManager;
    Crafting Crafting;
    ItemInfoPanel ItemInfoPanel;
    //InventoryTooltip Tooltip;

    void Awake()
    {
        InventoryManager = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>();
        Crafting = InventoryManager.Crafting;
    }

	// Use this for initialization
	void Start () {
        ItemInfoPanel = Crafting.ItemInfoPanel;
        //Tooltip = InventoryManager.Tooltip;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnPointerClick (PointerEventData data)
    {
        if(data.button == PointerEventData.InputButton.Left)
        {
            Crafting.Ingredients = new List<Ingredient>();
            Crafting.SelectedBlueprint = Blueprint;
            Crafting.UpdateData();
            ItemInfoPanel.ActivatePanel(Blueprint.Item);
            if (Crafting.IngredientsContainer.childCount != 0)
            {
                for (int i = 0; i < Crafting.IngredientsContainer.childCount; i++)
                {
                    Destroy(Crafting.IngredientsContainer.GetChild(i).gameObject);
                }
            }
            for (int i = 0; i < Blueprint.ItemIds.Count; i++)
            {
                GameObject ingredientPrefab = Instantiate(Crafting.IngredientPrefab);
                Ingredient ingredient = ingredientPrefab.GetComponent<Ingredient>();
                ingredientPrefab.transform.SetParent(Crafting.IngredientsContainer);
                ingredient.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                ingredientPrefab.GetComponent<Image>().sprite = Blueprint.ItemsForCraft[i].Icon;
                ingredient.Item = Blueprint.ItemsForCraft[i];
                Crafting.Ingredients.Add(ingredient);
                ingredient.RequiredAmountText.text = Blueprint.RequiredAmount[i].ToString() + " / " + Crafting.CraftingMaterialCounters[i];
            }
        }
    }
}
