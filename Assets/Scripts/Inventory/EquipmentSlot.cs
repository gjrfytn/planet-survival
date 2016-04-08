using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

	[HideInInspector]
	public ItemClass Item;
	public EquipmentSlotType EquipmentSlotType;
	[HideInInspector]
	public Inventory Inventory;
	public float IconScaleFactor;
	[HideInInspector]
	public Image ItemIcon;

	// Use this for initialization
	void Start () {
	
		Inventory = GameObject.FindGameObjectWithTag ("Inventory").GetComponent<Inventory> ();
		ItemIcon = transform.FindChild("ItemIcon").GetComponent<Image>();

	}

	public void OnPointerClick(PointerEventData data)
	{

		if (data.button == PointerEventData.InputButton.Left) 
		{
			Inventory.OnEquipmentSlotClick(gameObject, 0);
		}
		if(data.button == PointerEventData.InputButton.Right)
		{
			Inventory.OnEquipmentSlotClick(gameObject, 1);
		}

	}

	public void OnPointerEnter(PointerEventData data)
	{
		Inventory.OnMouseEnter (gameObject);
	}

	public void OnPointerExit(PointerEventData data)
	{
		Inventory.OnMouseExit (gameObject);
	}



}
