using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour {

    public ItemDatabase ItemDatabase;

    [Header("Inventory components")]
    public Inventory Inventory;
    public Equipment Equipment;
    public Crafting Crafting;
    public Hotbar Hotbar;

    [Header("Panels & buttons")]
    public bool EnableTooltip;
    public InventoryTooltip Tooltip;
    //public GameObject BackpackPanel;
    public bool EnableInfoPanel;
    public ItemInfoPanel ItemInfoPanel; // На будущее. Замена тултипов для сенсорных экранов
    public GameObject SplitPanel; 
    public GameObject DropPanel;
    public GameObject ItemActionButtons; // TODO Вслпывающие кнопки при нажатии правой кнопки мыши на предмете

    [Header("Prefabs")]
    public GameObject SlotPrefab;
    public GameObject EquipmentSlotPrefab;
    public GameObject ItemPrefab;
    [Header("Other")] // Неудачное название
    public GameObject DraggingSlot;

    [Header("Quality colors")]
    public Color JunkColor;
    public Color NormalColor;
    public Color UnusualColor;
    public Color RareColor;
    public Color LegendaryColor;

    [HideInInspector]
    public Item item;




	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void InfoPanel(Item item)
    {

    }

    public Color FindColor(Item item)
    {

        if (item.ItemQuality == ItemQuality.Junk)
        {
            return JunkColor;
        }
        if (item.ItemQuality == ItemQuality.Normal)
        {
            return NormalColor;
        }
        if (item.ItemQuality == ItemQuality.Unusual)
        {
            return UnusualColor;
        }
        if (item.ItemQuality == ItemQuality.Rare)
        {
            return RareColor;
        }
        if (item.ItemQuality == ItemQuality.Legendary)
        {
            return LegendaryColor;
        }
        return Color.clear;
    }

    public void Stackable(List<GameObject> slots)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].gameObject.transform.childCount > 0)
            {
                //AttachedItem item = slots[i].gameObject.transform.GetChild(0).GetComponent<AttachedItem>();
                AttachedItem item = slots[i].gameObject.transform.GetChild(0).GetComponent<AttachedItem>();
                if (item.Item.MaxStackSize > 1)
                {
                    //Подразумевается, что буде использоваться только 1 объект с присоединенным текстом
                    item.transform.GetComponentInChildren<Text>().text = item.Item.StackSize.ToString();
                    //StackSizeText.text = item.Item.StackSize.ToString();
                    item.transform.GetComponentInChildren<Text>().enabled = true; //IsStackable;
                }
                else
                {
                    item.transform.GetComponentInChildren<Text>().enabled = false;
                }
            }
        }
    }

    public void DropItem()
    {

    }

    public void SplitItem()
    {

    }

}
