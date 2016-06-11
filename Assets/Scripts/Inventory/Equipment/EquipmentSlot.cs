using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class EquipmentSlot : Slot, IDropHandler {

    public ItemType EquipmentType;
	
	// Update is called once per frame
	void Update () {
	
	}

    /*public new void OnDrop(PointerEventData data)
    {
        GameObject DraggingItem = DragItem.DraggingItem;
        base.OnDrop(data);
        if(DraggingItem.GetComponent<AttachedItem>().Item.IsEquipment && EquipmentType.Equals(DraggingItem.GetComponent<AttachedItem>().Item.ItemType))
        {
            Inventory.EquipItem(transform.GetComponentInChildren<AttachedItem>().Item);
        }
        else
        {
            return;
        }
    }*/
}
