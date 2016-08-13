using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class AttachedItem : MonoBehaviour, 
    IPointerEnterHandler, 
    IPointerExitHandler, 
    IPointerClickHandler,  
    IBeginDragHandler, 
    IDragHandler, 
    IEndDragHandler
{

    public Item Item;

    public int StackSize;

    [HideInInspector]
    public Text StackSizeText;
    [HideInInspector]
    public static Equipment Equipment;

    [HideInInspector]
    public ItemInfoPanel ItemInfoPanel;
    [HideInInspector]
    public InventoryTooltip Tooltip;

    public Transform LastSlot;
    public Transform CurrentSlot;
    public Transform DraggingSlot;

    public static GameObject DraggingItem;

    public GameObject Duplicate;

    [HideInInspector]
    public CanvasGroup CanvasGroup;

    private InventoryManager InventoryManager;
    private Inventory Inventory;

    private RectTransform RectTransform;
    private RectTransform DraggingSlotRectTransform;


    private void Start ()
    {
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        Equipment = InventoryManager.Equipment;
        Tooltip = InventoryManager.Tooltip;
        ItemInfoPanel = InventoryManager.ItemInfoPanel;

        CanvasGroup = GetComponent<CanvasGroup>();
        RectTransform = GetComponent<RectTransform>();
        DraggingSlotRectTransform = InventoryManager.DraggingSlot.GetComponent<RectTransform>();
        DraggingSlot = InventoryManager.DraggingSlot.transform;
    }

    public void CreateDuplicate(GameObject Item)
    {
        Item item = Item.GetComponent<AttachedItem>().Item;
		GameObject duplicate = Inventory.AddItem(/*item.Id*/InventoryManager.ItemDatabase.GetID(item), StackSize);
        Item.GetComponent<AttachedItem>().Duplicate = duplicate;
        duplicate.GetComponent<AttachedItem>().Duplicate = Item;
    }

    /*public void UpdateItemStackSize(AttachedItem attachedItem) //Old
    {
        StackSizeText = transform.GetComponentInChildren<Text>();
        if (Item.MaxStackSize > 1)
        {
            StackSizeText.text = StackSize.ToString();
            StackSizeText.enabled = true;
            if (Duplicate != null)
            {
                Duplicate.GetComponent<AttachedItem>().StackSizeText = Duplicate.GetComponentInChildren<Text>();
                Duplicate.GetComponent<AttachedItem>().StackSizeText.text = StackSize.ToString();
                Duplicate.GetComponent<AttachedItem>().StackSizeText.enabled = true;
            }
        }
        else
        {
            StackSizeText.enabled = false;
            if (Duplicate != null)
            {
                Duplicate.GetComponent<AttachedItem>().StackSizeText.enabled = false;
            }
        }
        if (StackSize <= 0)
        {
            if (Duplicate != null)
            {
                Destroy(Duplicate);
            }
            Destroy(gameObject);
        }
    }*/


    public void OnPointerEnter(PointerEventData data)
    {

        Item = GetComponentInChildren<AttachedItem>().Item;
        Tooltip.ActivateInventoryTooltip(Item);

        Vector3[] slotCorners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(slotCorners);

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Tooltip.Canvas.GetComponent<RectTransform>(), slotCorners[3], data.pressEventCamera, out localPointerPosition))
        {
            Tooltip.RectTransform.localPosition = localPointerPosition;
        }



    }

    public void OnPointerExit(PointerEventData data)
    {
        Tooltip.DeactivateTooltip();
    }

    public void OnPointerClick(PointerEventData data)
    {

        if (data.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.SelectedItem = this;
            InventoryEvents.ItemLeftClick(this);
            if (ItemInfoPanel != null)
            {
                ItemInfoPanel.ActivatePanel(this);
            }
            if(InventoryManager.SplitPanel != null)
            {
                InventoryManager.SplitPanel.gameObject.SetActive(false);
            }
        }
        else if (data.button == PointerEventData.InputButton.Right)
        {
            if (Item.IsConsumable)
            {
                InventoryEvents.ConsumeItem(this);
            }
            else if(Item.IsEquipment)
            {
                InventoryEvents.EquipItem(this);
            }
        }

    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            CanvasGroup.blocksRaycasts = false;
            DraggingItem = gameObject;
            LastSlot = transform.parent.gameObject.transform;

            /*if (transform.parent.GetComponent<Slot>() != null && transform.parent.GetComponent<Slot>().SlotType == SlotType.Equipment)
            {
                InventoryEvents.UnequipItem(this);
            }*/
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            if (RectTransform == null)
            {
                return;
            }
            if (data.button == PointerEventData.InputButton.Left)
            {
                RectTransform.SetAsLastSibling();
                DraggingItem.transform.SetParent(DraggingSlot);
                Vector2 localPointerPosition;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(DraggingSlotRectTransform, Input.mousePosition, data.pressEventCamera, out localPointerPosition))
                {
                    RectTransform.localPosition = localPointerPosition;
                    if (Duplicate != null)
                    {
                        Destroy(Duplicate);
                    }
                }
                Inventory.UpdateItemList();
            }
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {

            CanvasGroup.blocksRaycasts = true;
            if (gameObject.transform.parent == DraggingSlot)
            {
                InventoryEvents.DropItem(this);
            }
            DraggingItem = null;
            Inventory.UpdateItemList();
        }
    }

}
