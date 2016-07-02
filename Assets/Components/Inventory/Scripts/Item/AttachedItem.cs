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

    [HideInInspector]
    public Text StackSize;

    public InventoryManager InventoryManager;
    public Inventory Inventory;
    public static Equipment Equipment;
    public Hotbar Hotbar;
    public InventoryTooltip Tooltip;

    public Transform LastSlot;
    public Transform CurrentSlot;
    public Transform DraggingSlot;

    public static GameObject DraggingItem;
    public GameObject Duplicate;

    private CanvasGroup CanvasGroup;
    private RectTransform RectTransform;
    private RectTransform DraggingSlotRectTransform;

    // Use this for initialization
    void Start () {
        InventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        Inventory = InventoryManager.Inventory;
        Equipment = InventoryManager.Equipment;
        Hotbar = InventoryManager.Hotbar;
        Tooltip = InventoryManager.Tooltip;

        CanvasGroup = GetComponent<CanvasGroup>();
        RectTransform = GetComponent<RectTransform>();
        DraggingSlotRectTransform = InventoryManager.DraggingSlot.GetComponent<RectTransform>();
        DraggingSlot = InventoryManager.DraggingSlot.transform;
    }
	
	// Update is called once per frame
	void Update () {
	
	}



    public void CreateDuplicate(GameObject Item)
    {
        Item item = Item.GetComponent<AttachedItem>().Item;
        GameObject duplicate = Inventory.AddItem(item.Id, item.StackSize);
        Item.GetComponent<AttachedItem>().Duplicate = duplicate;
        duplicate.GetComponent<AttachedItem>().Duplicate = Item;
    }

    public void UpdateStackSize()
    {
        StackSize = transform.GetComponentInChildren<Text>();
        if (Item.MaxStackSize > 1)
        {
            StackSize.text = Item.StackSize.ToString();
            StackSize.enabled = true;
            if (Duplicate != null)
            {
                Duplicate.GetComponent<AttachedItem>().StackSize = Duplicate.transform.GetComponentInChildren<Text>();
                Duplicate.GetComponent<AttachedItem>().StackSize.text = Item.StackSize.ToString();
                Duplicate.GetComponent<AttachedItem>().StackSize.enabled = true;
            }
        }
        else
        {
            StackSize.enabled = false;
            if (Duplicate != null)
            {
                Duplicate.GetComponent<AttachedItem>().StackSize.enabled = false;
            }
        }
    }


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

        if (data.button == PointerEventData.InputButton.Right)
        {
            if (Item.UseSound != null)
            {
                InventoryManager.GetComponent<AudioSource>().PlayOneShot(Item.UseSound);
            }
            //ActionButtons.SetActive(true);
            //ActionButtons.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position;
            if (Item.IsEquipment)
            {
                #region IsEquipment
                for (int i = 0; i < Equipment.Slots.Count; i++)
                {
                    if (Equipment.Slots[i].GetComponent<EquipmentSlot>().EquipmentType.Equals(Item.ItemType) || Item.ItemType == ItemType.Socket)
                    {
                        if (Equipment.Slots[i].transform.childCount == 0)
                        {
                            if (GetComponentInParent<EquipmentSlot>() != null)
                            {

                            }
                            else
                            {
                                Inventory.EquipItem(Item);
                            }
                            gameObject.transform.SetParent(Equipment.Slots[i].gameObject.transform);
                            gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;

                            Inventory.UpdateItemList();
                            if (Duplicate != null)
                            {
                                Destroy(Duplicate);
                            }
                            break;

                        }
                        if (Equipment.Slots[i].transform.childCount != 0)
                        {
                            GameObject equippedItem = Equipment.Slots[i].transform.GetChild(0).gameObject;
                            Item itemFromSlot = equippedItem.GetComponent<AttachedItem>().Item;
                            if (Item.ItemType == ItemType.Backpack)
                            {
                                //
                            }
                            else
                            {
                                Inventory.EquipItem(Item);
                                Inventory.UnequipItem(equippedItem.GetComponent<AttachedItem>().Item);
                            }
                            if (this == null)
                            {
                                GameObject dropItem = (GameObject)Instantiate(itemFromSlot.DroppedItem);
                                dropItem.AddComponent<PickUpItem>();
                                dropItem.GetComponent<PickUpItem>().Item = itemFromSlot;
                                dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
                                Inventory.UpdateItemList();
                            }
                            else
                            {
                                //Inventory.EquipItem(Item);
                                //Inventory.UnequipItem(equippedItem.GetComponent<AttachedItem>().Item);
                                equippedItem.transform.SetParent(transform.parent);
                                equippedItem.GetComponent<RectTransform>().localPosition = Vector3.zero;
                                if (gameObject.GetComponentInParent<Slot>().SlotType == SlotType.Hotbar)
                                {
                                    CreateDuplicate(equippedItem);
                                }
                                if (gameObject.GetComponentInParent<Slot>().SlotType == SlotType.Equipment)
                                {
                                    // Нужно реализовать это условие по другому
                                    if (Inventory.ItemsInInventory.Count < (Inventory.Width * Inventory.Height))
                                    {
                                        if (Tooltip != null)
                                        {
                                            Tooltip.DeactivateTooltip();
                                        }
                                        Inventory.UnequipItem(equippedItem.GetComponent<AttachedItem>().Item);

                                        Inventory.AddItem(Item.Id, Item.StackSize);
                                        Inventory.UpdateItemList();
                                        InventoryManager.Stackable(Inventory.Slots);
                                        Destroy(gameObject);
                                    }
                                }
                                gameObject.transform.SetParent(Equipment.Slots[i].gameObject.transform);
                                gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                            }

                            if (Duplicate != null)
                            {
                                Destroy(Duplicate);
                            }

                            Inventory.UpdateItemList();
                            break;
                        }

                    }
                }
                #endregion
            }
            else if(Item.IsConsumable)
            {
                Item duplicateItem = null;

                    if (Duplicate != null)
                    {
                        duplicateItem = Duplicate.GetComponent<AttachedItem>().Item;
                    }
                    Inventory.UseItem(Item);
                    Item.StackSize--;
                    UpdateStackSize();
                    if (duplicateItem != null)
                    {
                        Duplicate.GetComponent<AttachedItem>().Item.StackSize--;
                        UpdateStackSize();
                        if (Item.StackSize <= 0)
                        {
                            if (Tooltip != null)
                            {
                                Tooltip.DeactivateTooltip();
                            }
                            Inventory.RemoveItem(Item);
                            Destroy(Duplicate);
                        }
                    }
                    if (Item.StackSize <= 0)
                    {
                        if (Tooltip != null)
                        {
                            Tooltip.DeactivateTooltip();
                        }
                        Destroy(gameObject);
                    }
            }
            else
            {
                //Item duplicateItem = null;

                if (Item.ItemType == ItemType.Book)
                {
                    //
                }
                if(Item.ItemType == ItemType.AudioPlayer)
                {
                    GameObject audioPlayer = Instantiate(Item.CustomObject);
                    audioPlayer.name = "AudioPlayer";
                    audioPlayer.transform.SetParent(InventoryManager.transform);
                    audioPlayer.GetComponent<RectTransform>().localPosition = Vector3.zero;
                }
            }
            if (Tooltip != null)
            {
                Tooltip.DeactivateTooltip();
            }
        }
        else if (data.button == PointerEventData.InputButton.Left) //Поменять
        {
            //ActionButtons.SetActive(false);
        }

    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            CanvasGroup.blocksRaycasts = false;
            DraggingItem = gameObject;
            LastSlot = transform.parent.gameObject.transform;

            if (transform.parent.GetComponent<Slot>().SlotType == SlotType.Equipment)
            {
                Inventory.UnequipItem(Item);
            }
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
            //DraggingItem = null;
            if (gameObject.transform.parent == DraggingSlot)
            {
                GameObject dropItem = (GameObject)Instantiate(Item.DroppedItem);
                if (dropItem.GetComponent<SpriteRenderer>().sprite == null)
                {
                    dropItem.GetComponent<SpriteRenderer>().sprite = Item.Icon;
                }
                dropItem.AddComponent<PickUpItem>();
                dropItem.GetComponent<PickUpItem>().Item = Item;
                dropItem.AddComponent<BoxCollider2D>();
                dropItem.GetComponent<BoxCollider2D>().isTrigger = true;
                //dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
                Inventory.UpdateItemList();
                Destroy(gameObject);
                Debug.Log("Предмет выброшен");
            }
            DraggingItem = null;
            Inventory.UpdateItemList();
        }
    }
}
