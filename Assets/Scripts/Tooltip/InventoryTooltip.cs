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

    public void ActivateInventoryTooltip(Item item)
    {
        if (InventoryManager.EnableTooltip)
        {
            if (RectTransform.localPosition.x < 0)
            {
                if (RectTransform.localPosition.y < 0)
                {
                    RectTransform.pivot = new Vector2(0, 0);
                }
                else
                {
                    RectTransform.pivot = new Vector2(0, 1);
                }
            }
            else {
                if (RectTransform.localPosition.y < 0)
                {
                    RectTransform.pivot = new Vector2(1, 0);
                }
                else
                {
                    RectTransform.pivot = new Vector2(1, 1);
                }
            }
            TooltipPanel.SetActive(true);
            RectTransform.localPosition = new Vector2();
            HeaderText.text = item.Name;
            DescriptionText.text = item.Description;
            Image.sprite = item.Icon;
            QualityColor.color = InventoryManager.FindColor(item);
        }
    }


}
