using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IDragHandler, IPointerDownHandler, IEndDragHandler
{
    private Vector2 pointerOffset;
    private RectTransform rectTransform;
    private RectTransform rectTransformSlot;
    private CanvasGroup canvasGroup;
    private GameObject oldSlot;
    private Inventory inventory;
    private Transform draggedItemBox;

    public delegate void ItemDelegate();
    public static event ItemDelegate updateInventoryList;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransformSlot = GameObject.FindGameObjectWithTag("DraggingItem").GetComponent<RectTransform>();
        inventory = transform.parent.parent.parent.GetComponent<Inventory>();
        draggedItemBox = GameObject.FindGameObjectWithTag("DraggingItem").transform;
    }


    public void OnDrag(PointerEventData data)
    {
        if (rectTransform == null)
            return;

        if (data.button == PointerEventData.InputButton.Left && transform.parent.GetComponent<CraftResultSlot>() == null)
        {
            rectTransform.SetAsLastSibling();
            transform.SetParent(draggedItemBox);
            Vector2 localPointerPosition;
            canvasGroup.blocksRaycasts = false;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransformSlot, Input.mousePosition, data.pressEventCamera, out localPointerPosition))
            {
                rectTransform.localPosition = localPointerPosition - pointerOffset;
                if (transform.GetComponent<ConsumeItem>().duplication != null)
                    Destroy(transform.GetComponent<ConsumeItem>().duplication);
            }
        }

        inventory.OnUpdateItemList();
    }



    public void OnPointerDown(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out pointerOffset);
            oldSlot = transform.parent.gameObject;
        }
        if (updateInventoryList != null)
            updateInventoryList();
    }

    public void createDuplication(GameObject Item)
    {
        Item item = Item.GetComponent<ItemOnObject>().item;
        GameObject duplication = GameObject.FindGameObjectWithTag("MainInventory").GetComponent<Inventory>().addItemToInventory(item.itemID, item.itemValue);
        duplication.transform.parent.parent.parent.GetComponent<Inventory>().stackableSettings();
        Item.GetComponent<ConsumeItem>().duplication = duplication;
        duplication.GetComponent<ConsumeItem>().duplication = Item;
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            canvasGroup.blocksRaycasts = true;
            Transform newSlot = null;
            if (data.pointerEnter != null)
                newSlot = data.pointerEnter.transform;

            if (newSlot != null)
            {

                GameObject firstItemGameObject = this.gameObject;
                GameObject secondItemGameObject = newSlot.parent.gameObject;
                RectTransform firstItemRectTransform = this.gameObject.GetComponent<RectTransform>();
                RectTransform secondItemRectTransform = newSlot.parent.GetComponent<RectTransform>();
                Item firstItem = rectTransform.GetComponent<ItemOnObject>().item;
                Item secondItem = new Item();
                if (newSlot.parent.GetComponent<ItemOnObject>() != null)
                    secondItem = newSlot.parent.GetComponent<ItemOnObject>().item;


                bool sameItem = firstItem.itemName == secondItem.itemName;
                bool sameItemRerferenced = firstItem.Equals(secondItem);
                bool secondItemStack = false;
                bool firstItemStack = false;
                if (sameItem)
                {
                    firstItemStack = firstItem.itemValue < firstItem.maxStack;
                    secondItemStack = secondItem.itemValue < secondItem.maxStack;
                }

                GameObject Inventory = secondItemRectTransform.parent.gameObject;
                if (Inventory.tag == "Slot")
                    Inventory = secondItemRectTransform.parent.parent.parent.gameObject;

                if (Inventory.tag.Equals("Slot"))
                    Inventory = Inventory.transform.parent.parent.gameObject;

       
                if (Inventory.GetComponent<Hotbar>() == null && Inventory.GetComponent<EquipmentSystem>() == null && Inventory.GetComponent<CraftSystem>() == null)
                {
   
                    if (newSlot.transform.parent.tag == "ResultSlot" || newSlot.transform.tag == "ResultSlot" || newSlot.transform.parent.parent.tag == "ResultSlot")
                    {
                        firstItemGameObject.transform.SetParent(oldSlot.transform);
                        firstItemRectTransform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        int newSlotChildCount = newSlot.transform.parent.childCount;
                        bool isOnSlot = newSlot.transform.parent.GetChild(0).tag == "ItemIcon";
         
                        if (newSlotChildCount != 0 && isOnSlot)
                        {
                       
                            bool fitsIntoStack = false;
                            if (sameItem)
                                fitsIntoStack = (firstItem.itemValue + secondItem.itemValue) <= firstItem.maxStack;
             

                            if (inventory.stackable && sameItem && firstItemStack && secondItemStack)
                            {

                                if (fitsIntoStack && !sameItemRerferenced)
                                {
                                    secondItem.itemValue = firstItem.itemValue + secondItem.itemValue;
                                    secondItemGameObject.transform.SetParent(newSlot.parent.parent);
                                    Destroy(firstItemGameObject);
                                    secondItemRectTransform.localPosition = Vector3.zero;
                                    if (secondItemGameObject.GetComponent<ConsumeItem>().duplication != null)
                                    {
                                        GameObject dup = secondItemGameObject.GetComponent<ConsumeItem>().duplication;
                                        dup.GetComponent<ItemOnObject>().item.itemValue = secondItem.itemValue;
                                        dup.GetComponent<SplitItem>().inv.stackableSettings();
                                        dup.transform.parent.parent.parent.GetComponent<Inventory>().updateItemList();
                                    }
                                }

                                else
                                {
          
                                    int rest = (firstItem.itemValue + secondItem.itemValue) % firstItem.maxStack;

       
                                    if (!fitsIntoStack && rest > 0)
                                    {
                                        firstItem.itemValue = firstItem.maxStack;
                                        secondItem.itemValue = rest;

                                        firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                        secondItemGameObject.transform.SetParent(oldSlot.transform);

                                        firstItemRectTransform.localPosition = Vector3.zero;
                                        secondItemRectTransform.localPosition = Vector3.zero;
                                    }
                                }

                            }
           
                            else
                            {
              
                                int rest = 0;
                                if (sameItem)
                                    rest = (firstItem.itemValue + secondItem.itemValue) % firstItem.maxStack;

             
                                if (!fitsIntoStack && rest > 0)
                                {
                                    secondItem.itemValue = firstItem.maxStack;
                                    firstItem.itemValue = rest;

                                    firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                    secondItemGameObject.transform.SetParent(oldSlot.transform);

                                    firstItemRectTransform.localPosition = Vector3.zero;
                                    secondItemRectTransform.localPosition = Vector3.zero;
                                }
   
                                else if (!fitsIntoStack && rest == 0)
                                {
          
                                    if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null && firstItem.itemType == secondItem.itemType)
                                    {
                                        newSlot.transform.parent.parent.parent.parent.GetComponent<Inventory>().UnEquipItem1(firstItem);
                                        oldSlot.transform.parent.parent.GetComponent<Inventory>().EquiptItem(secondItem);

                                        firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                        secondItemGameObject.transform.SetParent(oldSlot.transform);
                                        secondItemRectTransform.localPosition = Vector3.zero;
                                        firstItemRectTransform.localPosition = Vector3.zero;

                                        if (secondItemGameObject.GetComponent<ConsumeItem>().duplication != null)
                                            Destroy(secondItemGameObject.GetComponent<ConsumeItem>().duplication);

                                    }
                                  
                                    else if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null && firstItem.itemType != secondItem.itemType)
                                    {
                                        firstItemGameObject.transform.SetParent(oldSlot.transform);
                                        firstItemRectTransform.localPosition = Vector3.zero;
                                    }
    
                                    else if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() == null)
                                    {
                                        firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                        secondItemGameObject.transform.SetParent(oldSlot.transform);
                                        secondItemRectTransform.localPosition = Vector3.zero;
                                        firstItemRectTransform.localPosition = Vector3.zero;
                                    }
                                }

                            }

                        }


                        else
                        {
                            if (newSlot.tag != "Slot" && newSlot.tag != "ItemIcon")
                            {
                                firstItemGameObject.transform.SetParent(oldSlot.transform);
                                firstItemRectTransform.localPosition = Vector3.zero;
                            }
                            else
                            {                                
                                firstItemGameObject.transform.SetParent(newSlot.transform);
                                firstItemRectTransform.localPosition = Vector3.zero;

                                if (newSlot.transform.parent.parent.GetComponent<EquipmentSystem>() == null && oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null)
                                    oldSlot.transform.parent.parent.GetComponent<Inventory>().UnEquipItem1(firstItem);
                            }
                        }
                    }
                }



          
                if (Inventory.GetComponent<Hotbar>() != null)
                {
                    int newSlotChildCount = newSlot.transform.parent.childCount;
                    bool isOnSlot = newSlot.transform.parent.GetChild(0).tag == "ItemIcon";
        
                    if (newSlotChildCount != 0 && isOnSlot)
                    {
         
                        bool fitsIntoStack = false;
                        if (sameItem)
                            fitsIntoStack = (firstItem.itemValue + secondItem.itemValue) <= firstItem.maxStack;
  

                        if (inventory.stackable && sameItem && firstItemStack && secondItemStack)
                        {

                            if (fitsIntoStack && !sameItemRerferenced)
                            {
                                secondItem.itemValue = firstItem.itemValue + secondItem.itemValue;
                                secondItemGameObject.transform.SetParent(newSlot.parent.parent);
                                Destroy(firstItemGameObject);
                                secondItemRectTransform.localPosition = Vector3.zero;
                                if (secondItemGameObject.GetComponent<ConsumeItem>().duplication != null)
                                {
                                    GameObject dup = secondItemGameObject.GetComponent<ConsumeItem>().duplication;
                                    dup.GetComponent<ItemOnObject>().item.itemValue = secondItem.itemValue;
                                    Inventory.GetComponent<Inventory>().stackableSettings();
                                    dup.transform.parent.parent.parent.GetComponent<Inventory>().updateItemList();
                                }
                            }

                            else
                            {
                
                                int rest = (firstItem.itemValue + secondItem.itemValue) % firstItem.maxStack;

                     
                                if (!fitsIntoStack && rest > 0)
                                {
                                    firstItem.itemValue = firstItem.maxStack;
                                    secondItem.itemValue = rest;

                                    firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                    secondItemGameObject.transform.SetParent(oldSlot.transform);

                                    firstItemRectTransform.localPosition = Vector3.zero;
                                    secondItemRectTransform.localPosition = Vector3.zero;

                                    createDuplication(this.gameObject);
                                    secondItemGameObject.GetComponent<ConsumeItem>().duplication.GetComponent<ItemOnObject>().item = secondItem;
                                    secondItemGameObject.GetComponent<SplitItem>().inv.stackableSettings();

                                }
                            }

                        }
                   
                        else
                        {
          
                            int rest = 0;
                            if (sameItem)
                                rest = (firstItem.itemValue + secondItem.itemValue) % firstItem.maxStack;

                            bool fromEquip = oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null;

                   
                            if (!fitsIntoStack && rest > 0)
                            {
                                secondItem.itemValue = firstItem.maxStack;
                                firstItem.itemValue = rest;

                                createDuplication(this.gameObject);

                                firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                secondItemGameObject.transform.SetParent(oldSlot.transform);

                                firstItemRectTransform.localPosition = Vector3.zero;
                                secondItemRectTransform.localPosition = Vector3.zero;

                            }
         
                            else if (!fitsIntoStack && rest == 0)
                            {
                                if (!fromEquip)
                                {
                                    firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                    secondItemGameObject.transform.SetParent(oldSlot.transform);
                                    secondItemRectTransform.localPosition = Vector3.zero;
                                    firstItemRectTransform.localPosition = Vector3.zero;

                                    if (oldSlot.transform.parent.parent.gameObject.Equals(GameObject.FindGameObjectWithTag("MainInventory")))
                                    {
                                        Destroy(secondItemGameObject.GetComponent<ConsumeItem>().duplication);
                                        createDuplication(firstItemGameObject);
                                    }
                                    else
                                    {
                                        createDuplication(firstItemGameObject);
                                    }
                                }
                                else
                                {
                                    firstItemGameObject.transform.SetParent(oldSlot.transform);
                                    firstItemRectTransform.localPosition = Vector3.zero;
                                }
                            }

                        }
                    }
                
                    else
                    {
                        if (newSlot.tag != "Slot" && newSlot.tag != "ItemIcon")
                        {
                            firstItemGameObject.transform.SetParent(oldSlot.transform);
                            firstItemRectTransform.localPosition = Vector3.zero;
                        }
                        else
                        {                            
                            firstItemGameObject.transform.SetParent(newSlot.transform);
                            firstItemRectTransform.localPosition = Vector3.zero;

                            if (newSlot.transform.parent.parent.GetComponent<EquipmentSystem>() == null && oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null)
                                oldSlot.transform.parent.parent.GetComponent<Inventory>().UnEquipItem1(firstItem);
                            createDuplication(firstItemGameObject);
                        }
                    }

                }


         
                if (Inventory.GetComponent<EquipmentSystem>() != null)
                {
                    ItemType[] itemTypeOfSlots = GameObject.FindGameObjectWithTag("EquipmentSystem").GetComponent<EquipmentSystem>().itemTypeOfSlots;
                    int newSlotChildCount = newSlot.transform.parent.childCount;
                    bool isOnSlot = newSlot.transform.parent.GetChild(0).tag == "ItemIcon";
                    bool sameItemType = firstItem.itemType == secondItem.itemType;
                    bool fromHot = oldSlot.transform.parent.parent.GetComponent<Hotbar>() != null;

               
                    if (newSlotChildCount != 0 && isOnSlot)
                    {
        
                        if (sameItemType && !sameItemRerferenced) //
                        {
                            Transform temp1 = secondItemGameObject.transform.parent.parent.parent;
                            Transform temp2 = oldSlot.transform.parent.parent;                            

                            firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                            secondItemGameObject.transform.SetParent(oldSlot.transform);
                            secondItemRectTransform.localPosition = Vector3.zero;
                            firstItemRectTransform.localPosition = Vector3.zero;

                            if (!temp1.Equals(temp2))
                            {
                                if (firstItem.itemType == ItemType.UFPS_Weapon)
                                {
                                    Inventory.GetComponent<Inventory>().UnEquipItem1(secondItem);
                                    Inventory.GetComponent<Inventory>().EquiptItem(firstItem);
                                }
                                else
                                {
                                    Inventory.GetComponent<Inventory>().EquiptItem(firstItem);
                                    if (secondItem.itemType != ItemType.Backpack)
                                        Inventory.GetComponent<Inventory>().UnEquipItem1(secondItem);
                                }
                            }

                            if (fromHot)
                                createDuplication(secondItemGameObject);

                        }
         
                        else
                        {
                            firstItemGameObject.transform.SetParent(oldSlot.transform);
                            firstItemRectTransform.localPosition = Vector3.zero;

                            if (fromHot)
                                createDuplication(firstItemGameObject);
                        }

                    }
         
                    else
                    {
                        for (int i = 0; i < newSlot.parent.childCount; i++)
                        {
                            if (newSlot.Equals(newSlot.parent.GetChild(i)))
                            {
          
                                if (itemTypeOfSlots[i] == transform.GetComponent<ItemOnObject>().item.itemType)
                                {
                                    transform.SetParent(newSlot);
                                    rectTransform.localPosition = Vector3.zero;

                                    if (!oldSlot.transform.parent.parent.Equals(newSlot.transform.parent.parent))
                                        Inventory.GetComponent<Inventory>().EquiptItem(firstItem);

                                }
             
                                else
                                {
                                    transform.SetParent(oldSlot.transform);
                                    rectTransform.localPosition = Vector3.zero;
                                    if (fromHot)
                                        createDuplication(firstItemGameObject);
                                }
                            }
                        }
                    }

                }

                if (Inventory.GetComponent<CraftSystem>() != null)
                {
                    CraftSystem cS = Inventory.GetComponent<CraftSystem>();
                    int newSlotChildCount = newSlot.transform.parent.childCount;


                    bool isOnSlot = newSlot.transform.parent.GetChild(0).tag == "ItemIcon";
     
                    if (newSlotChildCount != 0 && isOnSlot)
                    {
       
                        bool fitsIntoStack = false;
                        if (sameItem)
                            fitsIntoStack = (firstItem.itemValue + secondItem.itemValue) <= firstItem.maxStack;
            

                        if (inventory.stackable && sameItem && firstItemStack && secondItemStack)
                        {
                
                            if (fitsIntoStack && !sameItemRerferenced)
                            {
                                secondItem.itemValue = firstItem.itemValue + secondItem.itemValue;
                                secondItemGameObject.transform.SetParent(newSlot.parent.parent);
                                Destroy(firstItemGameObject);
                                secondItemRectTransform.localPosition = Vector3.zero;


                                if (secondItemGameObject.GetComponent<ConsumeItem>().duplication != null)
                                {
                                    GameObject dup = secondItemGameObject.GetComponent<ConsumeItem>().duplication;
                                    dup.GetComponent<ItemOnObject>().item.itemValue = secondItem.itemValue;
                                    dup.GetComponent<SplitItem>().inv.stackableSettings();
                                    dup.transform.parent.parent.parent.GetComponent<Inventory>().updateItemList();
                                }
                                cS.ListWithItem();
                            }

                            else
                            {
             
                                int rest = (firstItem.itemValue + secondItem.itemValue) % firstItem.maxStack;

                   
                                if (!fitsIntoStack && rest > 0)
                                {
                                    firstItem.itemValue = firstItem.maxStack;
                                    secondItem.itemValue = rest;

                                    firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                    secondItemGameObject.transform.SetParent(oldSlot.transform);

                                    firstItemRectTransform.localPosition = Vector3.zero;
                                    secondItemRectTransform.localPosition = Vector3.zero;
                                    cS.ListWithItem();


                                }
                            }

                        }
           
                        else
                        {
               
                            int rest = 0;
                            if (sameItem)
                                rest = (firstItem.itemValue + secondItem.itemValue) % firstItem.maxStack;

                
                            if (!fitsIntoStack && rest > 0)
                            {
                                secondItem.itemValue = firstItem.maxStack;
                                firstItem.itemValue = rest;

                                firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                secondItemGameObject.transform.SetParent(oldSlot.transform);

                                firstItemRectTransform.localPosition = Vector3.zero;
                                secondItemRectTransform.localPosition = Vector3.zero;
                                cS.ListWithItem();

                            }
            
                            else if (!fitsIntoStack && rest == 0)
                            {
          
                                if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null && firstItem.itemType == secondItem.itemType)
                                {                                  

                                    firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                    secondItemGameObject.transform.SetParent(oldSlot.transform);
                                    secondItemRectTransform.localPosition = Vector3.zero;
                                    firstItemRectTransform.localPosition = Vector3.zero;

                                    oldSlot.transform.parent.parent.GetComponent<Inventory>().EquiptItem(secondItem);
                                    newSlot.transform.parent.parent.parent.parent.GetComponent<Inventory>().UnEquipItem1(firstItem);
                                }
                                   
                                else if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null && firstItem.itemType != secondItem.itemType)
                                {
                                    firstItemGameObject.transform.SetParent(oldSlot.transform);
                                    firstItemRectTransform.localPosition = Vector3.zero;
                                }
             
                                else if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() == null)
                                {
                                    firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                    secondItemGameObject.transform.SetParent(oldSlot.transform);
                                    secondItemRectTransform.localPosition = Vector3.zero;
                                    firstItemRectTransform.localPosition = Vector3.zero;
                                }
                            }

                        }
                    }
                    else
                    {
                        if (newSlot.tag != "Slot" && newSlot.tag != "ItemIcon")
                        {
                            firstItemGameObject.transform.SetParent(oldSlot.transform);
                            firstItemRectTransform.localPosition = Vector3.zero;
                        }
                        else
                        {                            
                            firstItemGameObject.transform.SetParent(newSlot.transform);
                            firstItemRectTransform.localPosition = Vector3.zero;

                            if (newSlot.transform.parent.parent.GetComponent<EquipmentSystem>() == null && oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null)
                                oldSlot.transform.parent.parent.GetComponent<Inventory>().UnEquipItem1(firstItem);
                        }
                    }

                }


            }

            else
            {
                GameObject dropItem = (GameObject)Instantiate(GetComponent<ItemOnObject>().item.itemModel);
                dropItem.AddComponent<PickUpItem>();
                dropItem.GetComponent<PickUpItem>().item = this.gameObject.GetComponent<ItemOnObject>().item;               
                dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
                inventory.OnUpdateItemList();
                if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null)
                    inventory.GetComponent<Inventory>().UnEquipItem1(dropItem.GetComponent<PickUpItem>().item);
                Destroy(this.gameObject);

            }
        }
        inventory.OnUpdateItemList();
    }

}
