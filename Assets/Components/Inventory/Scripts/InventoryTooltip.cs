using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class InventoryTooltip : MonoBehaviour
{

    public Canvas Canvas;

    public GameObject TooltipPanel;

    [HideInInspector]
    public RectTransform CanvasRectTransform;
    [HideInInspector]
    public RectTransform RectTransform;

    public Text HeaderText;
    public Text DescriptionText;
    public Text AdditionalText;

    public Image Image = null;

    public GameObject ScrollContent = null;

    public Image QualityColor;

    private Item Item;

    private InventoryManager InventoryManager;


    // Use this for initialization
    void Start()
    {
        DeactivateTooltip();
        CanvasRectTransform = Canvas.GetComponent<RectTransform>();
        RectTransform = TooltipPanel.GetComponent<RectTransform>();
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
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

    public void DeactivateTooltip()
    {
        TooltipPanel.SetActive(false);
    }
}
