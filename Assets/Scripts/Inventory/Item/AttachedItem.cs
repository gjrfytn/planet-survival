using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class AttachedItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public Item Item;

    public InventoryManager InventoryManager;
    public Inventory Inventory;
    public static Equipment Equipment;
    public Hotbar Hotbar;
    public InventoryTooltip Tooltip;

    public Transform LastSlot;
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
        DraggingSlotRectTransform = InventoryManager.DraggingItem.GetComponent<RectTransform>();
        DraggingSlot = InventoryManager.DraggingItem.transform;
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


    public void OnPointerEnter(PointerEventData data)
    {

        Item = GetComponentInChildren<AttachedItem>().Item;
        Tooltip.Item = Item;
        Tooltip.ActivateInventoryTooltip();

        Vector3[] slotCorners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(slotCorners);

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Tooltip.CanvasRectTransform, slotCorners[3], data.pressEventCamera, out localPointerPosition))
        {
            Tooltip.RectTransform.localPosition = localPointerPosition;
        }



    }

    public void OnPointerExit(PointerEventData data)
    {
        Tooltip.DeactivateTooltip();
    }

    public void OnPointerDown(PointerEventData data)
    {

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
                    if (Equipment.Slots[i].GetComponent<EquipmentSlot>().EquipmentType.Equals(Item.ItemType))
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
                                equippedItem.transform.SetParent(this.transform.parent);
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
            else
            {
                Item duplicateItem = null;

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
                if (Item.ItemType == ItemType.AudioRecord)
                {
                    //
                }
                if (Item.ItemType == ItemType.Consumable)
                {
                    if (Duplicate != null)
                    {
                        duplicateItem = Duplicate.GetComponent<AttachedItem>().Item;
                    }
                    Inventory.UseItem(Item);
                    Item.StackSize--;
                    InventoryManager.Stackable(Hotbar.Slots);
                    if (duplicateItem != null)
                    {
                        Duplicate.GetComponent<AttachedItem>().Item.StackSize--;
                        InventoryManager.Stackable(Inventory.Slots);
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




                /*Item duplicateItem = null;
                if (Duplicate != null)
                {
                    duplicateItem = Duplicate.GetComponent<AttachedItem>().Item;
                }
                Inventory.UseItem(Item);
                Item.StackSize--;
                Inventory.Stackable();
                if (duplicateItem != null)
                {
                    Duplicate.GetComponent<AttachedItem>().Item.StackSize--;
                    Inventory.Stackable();
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
                    //Inventory.RemoveItem(Item);
                }*/
            }
        }
        else if (data.button == PointerEventData.InputButton.Left) //Поменять
        {
            //ActionButtons.SetActive(false);
        }

    }

    public void OnBeginDrag(PointerEventData data)
    {
        CanvasGroup.blocksRaycasts = false;
        DraggingItem = gameObject;
        LastSlot = transform.parent.gameObject.transform;

        if (transform.parent.GetComponent<Slot>().SlotType == SlotType.Equipment)
        {
            Inventory.UnequipItem(Item);
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
                    RectTransform.localPosition = localPointerPosition /*- PointerOffset*/;
                    if (Duplicate != null)
                    {
                        Destroy(Duplicate);
                    }
                }
            }
            Inventory.UpdateItemList();
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        CanvasGroup.blocksRaycasts = true;
        DraggingItem = null;
        if (gameObject.transform.parent == DraggingSlot)
        {
            GameObject dropItem = (GameObject)Instantiate(Item.DroppedItem);
            dropItem.AddComponent<PickUpItem>();
            dropItem.GetComponent<PickUpItem>().Item = Item;
            dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
            Inventory.UpdateItemList();
            Destroy(gameObject);
        }
        Inventory.UpdateItemList();
    }

}
