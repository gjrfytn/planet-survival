using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//[RequireComponent(typeof(Tooltip))]
public class InventoryTooltip : Tooltip
{
    public Image QualityColor;

    public Item Item;

    [HideInInspector]
    public InventoryManager InventoryManager;


    // Use this for initialization
    void Start()
    {
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        DeactivateTooltip();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivateInventoryTooltip()
    {
        if (InventoryManager.EnableTooltip)
        {
            TooltipPanel.SetActive(true);
            RectTransform.localPosition = new Vector2();
            HeaderText.text = Item.Name;
            DescriptionText.text = Item.Description;
            Image.sprite = Item.Icon;
            QualityColor.color = InventoryManager.FindColor(Item);
        }
    }


}
