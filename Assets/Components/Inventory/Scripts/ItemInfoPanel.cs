using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemInfoPanel : MonoBehaviour {

    InventoryManager InventoryManager;
    public GameObject ItemPanel;

    public Text HeaderText;
    public Text DescriptionText;
    public Image Icon;
    public Image QualityColor;
    public GameObject AdditionalInformation;

    [Header("Buttons")]
    public bool EnableActionButtons;
    public Button ConsumeButton;
    public Button EquipButton;
    public Button SplitButton;
    public Button DropButton;


    // Use this for initialization
    void Start () {

        InventoryManager = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>();
        DeactivatePanel();
    }

    public void ActivatePanel(AttachedItem attachedItem)
    {
        ItemPanel.SetActive(true);
        HeaderText.text = attachedItem.Item.Name;
        HeaderText.color = InventoryManager.FindColor(attachedItem.Item);
        DescriptionText.text = attachedItem.Item.Description;
        Icon.sprite = attachedItem.Item.Icon;
        QualityColor.color = InventoryManager.FindColor(attachedItem.Item);

        // Не лучший способ использования лямбда выражений
        if (EnableActionButtons)
        {
            if (attachedItem.Item.IsConsumable)
            {
                ConsumeButton.onClick.RemoveAllListeners();
                ConsumeButton.onClick.AddListener(() => InventoryManager.Consume(attachedItem));
                ConsumeButton.gameObject.SetActive(true);
            }
            else
            {
                ConsumeButton.gameObject.SetActive(false);
            }
            if (attachedItem.Item.IsEquipment)
            {
                EquipButton.onClick.RemoveAllListeners();
                EquipButton.onClick.AddListener(() => InventoryManager.Equip(attachedItem));
                EquipButton.gameObject.SetActive(true);
            }
            else
            {
                EquipButton.gameObject.SetActive(false);
            }
            if (attachedItem.StackSize > 1)
            {
                SplitButton.onClick.RemoveAllListeners();
                SplitButton.onClick.AddListener(() => InventoryManager.Split(attachedItem));
                SplitButton.gameObject.SetActive(true);
            }
            else
            {
                SplitButton.gameObject.SetActive(false);
            }

            DropButton.onClick.RemoveAllListeners();
            DropButton.onClick.AddListener(() => InventoryEvents.DropItem(attachedItem));
        }
    }

    public void ActivatePanel(Item item)
    {
        ItemPanel.SetActive(true);
        HeaderText.text = item.Name;
        HeaderText.color = InventoryManager.FindColor(item);
        DescriptionText.text = item.Description;
        Icon.sprite = item.Icon;
        QualityColor.color = InventoryManager.FindColor(item);
    }

    public void DeactivatePanel()
    {
        ItemPanel.SetActive(false);
    }
}
