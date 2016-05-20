using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {

	[HideInInspector]
	public ItemClass Item;
	public bool ContainsItem;
	public int ItemStartNumber;
	public Inventory Inventory;
	public Text StackSizeText;
	public Image ItemImage;
	public Image ItemFrame;

	// Use this for initialization
	void Start () {
	
		Inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();

	}
		
	public void OnPointerDown(PointerEventData data)
	{
		if(data.button == PointerEventData.InputButton.Left)
		{
			Inventory.OnSlotClick(gameObject, 0);
		}
		if(data.button == PointerEventData.InputButton.Right)
		{
			/*if(!Inventory.Identifying)
			{
				Inventory.OnSlotClick(gameObject, 1);
			}*/
		}
	}
	public void OnPointerEnter(PointerEventData data)
	{
		if(Item.ItemName != "")
		{
			Inventory.OnMouseEnter(gameObject);
		}
		Inventory.HoveredSlot = this;
		if(Inventory.Dragging)
		{
			Inventory.DragItemBackground.gameObject.SetActive(true);
		}
	}
	public void OnPointerExit(PointerEventData data)
	{
		if(Item.ItemName != "")
		{
			Inventory.OnMouseExit(gameObject);
		}
		Inventory.HoveredSlot = null;
		if(Inventory.Dragging)
		{
			Inventory.DragItemBackground.gameObject.SetActive(false);
		}
	}
}
