using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SplitItem : MonoBehaviour {

    InventoryManager InventoryManager;
    Inventory Inventory;

    //private Inventory Inventory;
    private int SplitSize;

    public InputField InputField;
    public Slider Slider;
    public Button AcceptButton;
    public Button CancelButton;
    public GameObject Slot;

    private void Awake()
    {
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
    }


    public void Split(AttachedItem itemFromSlot)
    {
        Slider.onValueChanged.RemoveAllListeners();
        InputField.onValueChanged.RemoveAllListeners();
        gameObject.SetActive(true);
        GameObject tempItemGameObject = Instantiate(itemFromSlot.gameObject);
        AttachedItem tempItem = tempItemGameObject.GetComponent<AttachedItem>();

        tempItemGameObject.transform.SetParent(Slot.transform, false);
        //tempItemGameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        //tempItemGameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        tempItemGameObject.name = itemFromSlot.Item.Name;
        tempItemGameObject.transform.localPosition = Vector2.zero;
        Slider.value = 1;
        Slider.minValue = 1;
        Slider.maxValue = itemFromSlot.StackSize - 1;
        tempItem.StackSize = (int)Slider.minValue;
        tempItem.CanvasGroup.interactable = false;
        tempItem.CanvasGroup.blocksRaycasts = false;
        itemFromSlot.StackSize = (int)Slider.maxValue;
        InventoryManager.UpdateItemStack(tempItem);
        InventoryManager.UpdateItemStack(itemFromSlot);

        Slider.onValueChanged.AddListener(delegate { ValueToSplit(itemFromSlot, tempItem); });
        //SplitPanel.InputField.onValueChanged.AddListener(delegate { ValueToSplit(tempItem); });
        AcceptButton.onClick.RemoveAllListeners();
        AcceptButton.onClick.AddListener(delegate { Accept(itemFromSlot, tempItem); });
        CancelButton.onClick.RemoveAllListeners();
        CancelButton.onClick.AddListener(delegate {Cancel(itemFromSlot, tempItem); });
    }

    void ValueToSplit(AttachedItem itemFromSlot, AttachedItem tempItem)
    {

        tempItem.StackSize = (int)Slider.value;

        if (itemFromSlot != null)
        {
            itemFromSlot.StackSize = Mathf.Abs(tempItem.StackSize - (int)Slider.maxValue - 1);
            InventoryManager.UpdateItemStack(itemFromSlot);
        }
        //SplitPanel.InputField.text = SplitPanel.Slider.value.ToString();
        /*if (int.Parse(SplitPanel.InputField.text) >= SplitPanel.Slider.minValue && int.Parse(SplitPanel.InputField.text) <= SplitPanel.Slider.maxValue)
        {
            SplitPanel.Slider.value = float.Parse(SplitPanel.InputField.text);
        }*/
        InventoryManager.UpdateItemStack(tempItem);
    }

    public void Accept(AttachedItem itemFromSlot, AttachedItem tempItem)
    {
        tempItem.CanvasGroup.interactable = true;
        tempItem.CanvasGroup.blocksRaycasts = true;
        Inventory.CloneItemIntoSlot(tempItem);
        Inventory.UpdateItemList();
        Deactivate(itemFromSlot, tempItem);
    }

    public void Cancel(AttachedItem itemFromSlot, AttachedItem tempItem)
    {
        itemFromSlot.StackSize = (int)Slider.maxValue + 1;
        Deactivate(itemFromSlot, tempItem);
    }

    public void Deactivate(AttachedItem itemFromSlot, AttachedItem tempItem)
    {
        Destroy(tempItem.gameObject);
        InventoryManager.UpdateItemStack(itemFromSlot);
        if (InventoryManager.ItemInfoPanel != null)
        {
            InventoryManager.ItemInfoPanel.ActivatePanel(itemFromSlot);
        }
        itemFromSlot = null;
        //Reset slider values
        gameObject.SetActive(false);
    }
}
