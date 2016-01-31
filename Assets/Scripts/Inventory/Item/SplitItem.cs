using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SplitItem : MonoBehaviour, IPointerDownHandler
{ 

    private bool pressingButtonToSplit;   
    public Inventory inv;        
    static InputManager inputManagerDatabase = null;

    void Update()
    {
        if (Input.GetKeyDown(inputManagerDatabase.SplitItem))         
            pressingButtonToSplit = true;                 
        if (Input.GetKeyUp(inputManagerDatabase.SplitItem))
            pressingButtonToSplit = false;        

    }

    void Start()
    {
		inputManagerDatabase = (InputManager)Resources.Load("Inventory/InputManager");
    }

    public void OnPointerDown(PointerEventData data)     
    {
        inv = transform.parent.parent.parent.GetComponent<Inventory>();
        if (transform.parent.parent.parent.GetComponent<Hotbar>() == null && data.button == PointerEventData.InputButton.Left && pressingButtonToSplit && inv.stackable && (inv.ItemsInInventory.Count < (inv.height * inv.width))) 
        {
            ItemOnObject itemOnObject = GetComponent<ItemOnObject>();               

            if (itemOnObject.item.itemValue > 1)      
            {
                int splitPart = itemOnObject.item.itemValue;     
                itemOnObject.item.itemValue = (int)itemOnObject.item.itemValue / 2;   
                splitPart = splitPart - itemOnObject.item.itemValue;     

                inv.addItemToInventory(itemOnObject.item.itemID, splitPart);    
                inv.stackableSettings();

                if (GetComponent<ConsumeItem>().duplication != null)
                {
                    GameObject dup = GetComponent<ConsumeItem>().duplication;
                    dup.GetComponent<ItemOnObject>().item.itemValue = itemOnObject.item.itemValue;
                    dup.GetComponent<SplitItem>().inv.stackableSettings();
                }
                inv.updateItemList();

            }
        }
    }
}
