using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class Inventory : MonoBehaviour {

	public List<int> ItemsIDs;

	public float SlotIconSize = 78f;
	public int InventoryHeight;
	public int InventoryWidth;

	public List<EquipmentSlot> EquipmentSlots;

	public bool DragSwap;
	public bool SnapItemWhenDragging;
	public bool AutoFindEquipmentSlot;
	public bool RightClickUnequipItems;

	public Color SnapCanFitColor;
	public Color SnapCannotFitColor;

	public Color JunkColor;
	public Color NormalColor;
	public Color UnusualColor;
	public Color RareColor;
	public Color LegendaryColor;

	public Image DragItem;
	public Image DragItemBackground;
	public Text DragStackText;

	public ItemDatabase Database;
	public Tooltip Tooltip;

	public Transform InventorySlots;

	public GameObject Canvas;
	public GameObject SlotPrefab;
	public GameObject SplitWindow;

	[HideInInspector]
	public Player Player;
	[HideInInspector]
	public List<InventorySlot> Items;
	[HideInInspector]
	public int DragStartIndex;
	[HideInInspector]
	public bool Dragging;
	[HideInInspector]
	public bool ShowInventory;
	[HideInInspector]
	public ItemClass DraggedItem;
	[HideInInspector]
	public InventorySlot HoveredSlot;
	[HideInInspector]
	public InventorySlot ItemToSplit;

	void Awake() {
		Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

		for(int i = 0; i < InventoryWidth * InventoryHeight; i++)
		{
			GameObject slot = Instantiate(SlotPrefab) as GameObject;
			slot.transform.SetParent(InventorySlots);
			slot.name = "Slot" + i;
			slot.transform.localScale = Vector3.one;

			InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
			inventorySlot.Item = new ItemClass();
			inventorySlot.Item.ItemName = "";
			inventorySlot.ItemStartNumber = i;
			Items.Add(inventorySlot);
		}

		GridLayoutGroup grid = GetComponentInChildren<GridLayoutGroup>();
		grid.constraintCount = InventoryWidth;
		grid.cellSize = new Vector2(SlotIconSize,SlotIconSize);

		for(int i = 0; i < EquipmentSlots.Count; i++)
		{
			EquipmentSlots[i].Item = new ItemClass();
			EquipmentSlots[i].Item.ItemName = "";
		}
			
	}
	// Use this for initialization
	void Start () {
	
		for(int i = 0; i < Items.Count; i++)
		{
			Items[i].GetComponent<CanvasGroup>().interactable = false;
		}

	}
	
	// Update is called once per frame
	void Update () 
	{

		if(Dragging)
		{
			if(SnapItemWhenDragging)
			{
				if(HoveredSlot)
				{
					DragItem.rectTransform.position = HoveredSlot.transform.position;
					DragItemBackground.rectTransform.position = HoveredSlot.transform.position;
					DragItemBackground.rectTransform.sizeDelta = new Vector2(DraggedItem.Width * SlotIconSize, DraggedItem.Height * SlotIconSize);
					if(CheckItemFit(DraggedItem, HoveredSlot, false))
					{
						DragItemBackground.color = SnapCanFitColor;
					}
					else
					{
						DragItemBackground.color = SnapCannotFitColor;
					}
				}
				else
				{
					DragItem.rectTransform.position = Input.mousePosition;
					DragItemBackground.rectTransform.position = Input.mousePosition;
					DragItemBackground.color = Color.white;
				}
			}
			else
			{
				DragItem.rectTransform.position = new Vector3(Input.mousePosition.x + DragItem.rectTransform.sizeDelta.x * DragItem.rectTransform.lossyScale.x * 0.5f, Input.mousePosition.y - DragItem.rectTransform.sizeDelta.x * DragItem.rectTransform.lossyScale.y * 0.5f, -20);
			}
		}

		if(DraggedItem.Stackable)
		{
			DragStackText.gameObject.SetActive(true);
			DragStackText.text = DraggedItem.StackSize.ToString();
		}
	
	}// End Update

	public bool AddItem(ItemClass item)
	{
		for(int i = 0; i < Items.Count; i++)
		{
			if(Items[i].Item.ItemName == "")
			{
				if(!CheckItemFit(item, Items[i], false))
				{
					continue;
				}

				int counter = 0;

				for(int j = 0; j < item.Height; j++ )
				{
					for(int k = 0; k < item.Width; k++)
					{
						if(Items[i + InventoryWidth * j + k].Item.ItemName != "")
						{
							counter++;
						}
					}
				}

				if(counter == 0)
				{
					for(int l = 0; l < item.Height; l++)
					{
						for(int m = 0; m < item.Width; m++)
						{
							Items[i + InventoryWidth * l + m].Item = DeepCopy(item);
							Items[i + InventoryWidth * l + m].ItemStartNumber = i;
							Items[i + InventoryWidth * l + m].GetComponent<Image>().color = Color.clear;
							Items[i + InventoryWidth * l + m].StackSizeText.gameObject.SetActive(false);

							if(Items.IndexOf(Items[i + InventoryWidth * l + m]) == i)
							{
								SetSlotImageSprite(Items[i + InventoryWidth * l + m], item.Icon);
								Items[i + InventoryWidth * l + m].ItemFrame.gameObject.SetActive(true);
								Items[i + InventoryWidth * l + m].ItemFrame.GetComponent<CanvasGroup>().interactable = true;
								Items[i + InventoryWidth * l + m].ItemFrame.GetComponent<CanvasGroup>().blocksRaycasts = true;
								Items[i + InventoryWidth * l + m].GetComponent<CanvasGroup>().blocksRaycasts = true;
								Items[i + InventoryWidth * l + m].ItemFrame.rectTransform.sizeDelta = new Vector2(item.Width * SlotIconSize, item.Height * SlotIconSize);
							
								if(item.Stackable)
								{
									Items[i + InventoryWidth * l + m].StackSizeText.gameObject.SetActive(true);
									Items[i + InventoryWidth * l + m].StackSizeText.text = item.StackSize.ToString();
								}
							}
						}
					}
					return true;
				}

			}
		}
		return false;
	}// End AddItem

	public void AddItemButton()
	{
		List<string> itemNames = new List<string>();

		for(int i = 0; i < Database.Items.Count; i++)
		{
			itemNames.Add(Database.Items[i].ItemName);
			Debug.Log("" + Database.Items[i].ItemName);
		}

		/*for(int i = 0; i < ItemsIDs.Count; i++)
		{

			ItemClass item = Database.FindItem(ItemsIDs[i]);
			ItemsIDs.Insert(ItemsIDs.Count, 0);
		}*/


	}

	public bool AddItemAtSlot(ItemClass item, InventorySlot slot)
	{

		int i = Items.IndexOf(slot);
		for(int j = 0; j < item.Height; j++)
		{
			for(int k = 0; k < item.Width; k++)
			{
				if(!CheckItemFit(item, slot, true))
				{
					return false;
				}

				if(Items[i + InventoryWidth * j + k].Item.ItemName != "")
				{
					if(DragSwap)
					{
						int counter = 0;
						InventorySlot foundSlot = null;
						int itemStartNumber = Mathf.RoundToInt(Mathf.Infinity);
						for(int l = 0; l < item.Height; l++)
						{
							for(int m = 0; m < item.Width; m++)
							{
								if(Items[slot.ItemStartNumber + InventoryWidth * l + m].Item.ItemName != "" && itemStartNumber != Items[slot.ItemStartNumber + InventoryWidth * l + m].ItemStartNumber)
								{

									itemStartNumber = Items[slot.ItemStartNumber + InventoryWidth * l + m].ItemStartNumber;
									counter++;
									foundSlot = Items[slot.ItemStartNumber + InventoryWidth* l + m];

								}
							}
						}

						if(counter == 1)
						{

							ItemClass tempItem = DeepCopy(DraggedItem);
							DragItemFromSlot(foundSlot);
							AddItemAtSlot(tempItem, slot);
							transform.root.GetComponent<AudioSource>().PlayOneShot(Items[i + InventoryWidth * j + k].Item.ItemSound);
							return false;
						}
						else
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
				if(j == item.Height - 1 && k == item.Width - 1)
				{
					for(int l = 0; l < item.Height; l ++)
					{
						for(int m = 0; m < item.Width; m++)
						{
							Items[i + InventoryWidth * l + m].Item = DeepCopy(item);
							Items[i + InventoryWidth * l + m].ItemStartNumber = i;
							Items[i + InventoryWidth * l + m].GetComponent<Image>().color = Color.clear;
							Items[i + InventoryWidth * l + m].StackSizeText.gameObject.SetActive(false);
							if(Items.IndexOf(Items[i + InventoryWidth * l + m]) == i)
							{
								SetSlotImageSprite(Items[i + InventoryWidth * l + m], item.Icon);
								Items[i + InventoryWidth * l + m].ItemFrame.gameObject.SetActive(true);
								Items[i + InventoryWidth * l + m].ItemFrame.GetComponent<CanvasGroup>().interactable = true;
								Items[i + InventoryWidth * l + m].ItemFrame.GetComponent<CanvasGroup>().blocksRaycasts = true;
								Items[i + InventoryWidth * l + m].GetComponent<CanvasGroup>().blocksRaycasts = true;
								Items[i + InventoryWidth * l + m].ItemFrame.rectTransform.sizeDelta = new Vector2(item.Width * SlotIconSize, item.Height * SlotIconSize);
								if(item.Stackable)
								{
									Items[i + InventoryWidth * l + m].StackSizeText.gameObject.SetActive(true);
									Items[i + InventoryWidth * l + m].StackSizeText.text = item.StackSize.ToString();

								}
							}
						}
					}

					return true;

				}
			}
		}

		return false;

	}

	public bool AddStackableItem(ItemClass item)
	{
      for(int i = 0; i < Items.Count; i++)
        {
            if(Items[i].Item.ItemName != "" && Items[i].Item.ItemName == item.ItemName && Items[i].Item.StackSize != Items[i].Item.MaxStackSize)
            {
                int count = Items[i].Item.StackSize + item.StackSize;

                if(count <= Items[i].Item.MaxStackSize)
                {
                    Items[i].Item.StackSize = count;
                    Items[i].StackSizeText.text = Items[i].Item.StackSize.ToString();
                    return true;
                }

                else if(count > Items[i].Item.MaxStackSize)
                {
                    ItemClass temp = DeepCopy(item);
                    temp.StackSize = count - Items[i].Item.MaxStackSize;
                    if(AddItem(temp))
                    {
                        Items[i].Item.StackSize = Items[i].Item.MaxStackSize;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if(i == Items.Count - 1)
            {
                AddItem(item);
                return true;
            }
        }
        return false;
	}

	public void RemoveItemFromSlot(InventorySlot slot)
	{
		ItemClass item = DeepCopy(slot.Item);

		for(int i = 0; i < item.Height; i++)
		{
			for(int j = 0; j < item.Width; j++)
			{
				Items[slot.ItemStartNumber + InventoryWidth * i + j].Item = new ItemClass();
				Items[slot.ItemStartNumber + InventoryWidth * i + j].Item.ItemName = "";
				Items[slot.ItemStartNumber + InventoryWidth * i + j].ItemImage.color = Color.white;
				Items[slot.ItemStartNumber + InventoryWidth * i + j].GetComponent<Image>().color = Color.white;
				Items[slot.ItemStartNumber + InventoryWidth * i + j].ItemImage.gameObject.SetActive(false);
				Items[slot.ItemStartNumber + InventoryWidth * i + j].ItemFrame.gameObject.SetActive(false);
				Items[slot.ItemStartNumber + InventoryWidth * i + j].StackSizeText.gameObject.SetActive(false);
				Items[slot.ItemStartNumber + InventoryWidth * i + j].GetComponent<CanvasGroup>().blocksRaycasts = true;
			}
		}
		ResetItemStartNumbers();
	}


	public void DragSwapEquippedItem(EquipmentSlot slot) 
	{
		ItemClass item = DeepCopy(slot.Item);
		slot.Item = DeepCopy(DraggedItem);
		SetSlotImageSprite(slot, slot.Item.Icon);
		DraggedItem = item;
		DragItem.sprite = item.Icon;
		DragItem.rectTransform.sizeDelta = new Vector2(item.Width * SlotIconSize, item.Height * SlotIconSize);

		if(slot.Item.ItemType == EquipmentSlotType.OffHand) 
		{
			for(int i = 0; i < EquipmentSlots.Count; i++) 
			{
				if(EquipmentSlots[i].EquipmentSlotType == EquipmentSlotType.Weapon) 
				{
					if(EquipmentSlots[i].Item.TwoHanded) 
					{
						AddItem(EquipmentSlots[i].Item);
						RemoveEquippedItem(EquipmentSlots[i]);
					}
				}
			}
		}
	}


	public void DragEquippedItem(EquipmentSlot slot) 
	{
		DraggedItem = DeepCopy(slot.Item);
		DragItem.sprite = slot.Item.Icon;
		DragItem.rectTransform.sizeDelta = new Vector2(slot.Item.Width * SlotIconSize, slot.Item.Height * SlotIconSize);
		DragItem.gameObject.SetActive(true);
		Dragging = true;
		slot.transform.FindChild("ItemBackground").gameObject.SetActive(true);
	}

	public void RemoveEquippedItem (EquipmentSlot slot)
	{
		slot.Item = new ItemClass ();
		slot.Item.ItemName = "";
		slot.ItemIcon.gameObject.SetActive (false);
		Tooltip.HideTooltip();
		slot.transform.FindChild ("ItemBackground").gameObject.SetActive (true);
	}

	public bool SwapItems(InventorySlot slot)
	{
		ItemClass item = DeepCopy(Items[slot.ItemStartNumber].Item);
		RemoveItemFromSlot(slot);
		AddItemAtSlot(DraggedItem, slot);
		Dragging = true;
		DraggedItem = item;
		DragItem.sprite = item.Icon;
		DragItem.rectTransform.sizeDelta = new Vector2(item.Width * SlotIconSize, item.Height * SlotIconSize);
		DragItem.gameObject.SetActive(true);
		return true;
	}

	public bool DragItemFromSlot(InventorySlot slot)
	{

		ItemClass item = DeepCopy(slot.Item);
		DragStartIndex = slot.ItemStartNumber;
		RemoveItemFromSlot(slot);
		Dragging = true;
		DraggedItem = item;
		DragItem.sprite = item.Icon;
		DragItem.rectTransform.sizeDelta = new Vector2(item.Width * SlotIconSize, item.Height * SlotIconSize);
		DragItem.gameObject.SetActive(true);
		return true;

	}

	public void ReturnDraggedItem()
	{

		AddItemAtSlot(DraggedItem, Items[DragStartIndex]);

		StopDragging();

	}


	public void DropDraggedItem()
	{
		GameObject obj = Instantiate(DraggedItem.DroppedItem, Player.transform.position + Player.transform.forward, Quaternion.identity) as GameObject;
		ItemClassController ic = obj.AddComponent<ItemClassController>();
		ic.Item = DeepCopy(DraggedItem);
		GameObject itemCanvasObj = Instantiate(Canvas) as GameObject;
		itemCanvasObj.transform.SetParent(obj.transform);
		itemCanvasObj.transform.localPosition = new Vector3(0, 1, 0);
		StopDragging();
	}

	public void StopDragging()
	{
		Dragging = false;
		DraggedItem = new ItemClass();
		DraggedItem.ItemName = "";
		DragItem.gameObject.SetActive(false);
		DragItemBackground.gameObject.SetActive(false);
	}

	public void SetSlotImageSprite(InventorySlot slot, Sprite sprite) 
	{
		slot.ItemImage.sprite = slot.Item.Icon;
		slot.ItemImage.rectTransform.sizeDelta = new Vector2(slot.Item.Width * SlotIconSize, slot.Item.Height * SlotIconSize);
		slot.ItemImage.gameObject.SetActive(true);
	}

	public void SetSlotImageSprite(EquipmentSlot slot, Sprite sprite) {
		slot.ItemIcon.sprite = slot.Item.Icon;
		slot.ItemIcon.rectTransform.sizeDelta = new Vector2(slot.Item.Width * SlotIconSize * slot.IconScaleFactor, slot.Item.Height * SlotIconSize * slot.IconScaleFactor);
		slot.ItemIcon.gameObject.SetActive(true);
	}




	public void OnSlotClick(GameObject obj, int mouseIndex) 
	{
		InventorySlot slot = obj.GetComponent<InventorySlot>();
		if(mouseIndex == 0) 
		{
			if(slot.Item.ItemName != "") 
			{
				if(Dragging) 
				{
					
					if(DraggedItem.ItemName == slot.Item.ItemName && slot.Item.Stackable) 
					{
						slot.Item.StackSize += DraggedItem.StackSize;
						transform.root.GetComponent<AudioSource>().PlayOneShot(slot.Item.ItemSound);
						if(slot.Item.StackSize > slot.Item.MaxStackSize) 
						{
							int tempStack = slot.Item.MaxStackSize - slot.Item.StackSize;
							slot.Item.StackSize = slot.Item.MaxStackSize;
							DraggedItem.StackSize = Mathf.Abs(tempStack);
							slot.StackSizeText.text = slot.Item.StackSize.ToString();
							return;
						}
						else 
						{
							slot.StackSizeText.text = slot.Item.StackSize.ToString();
							StopDragging();
							return;
						}
					}

					int counter = 0;
					InventorySlot foundSlot = null;
					int itemStartNumber = Mathf.RoundToInt(Mathf.Infinity);
					for(int i = 0; i < DraggedItem.Height; i++) 
					{
						for(int j = 0; j < DraggedItem.Width; j++) 
						{
							if(Items[slot.ItemStartNumber + InventoryWidth * i + j].Item.ItemName != "" && itemStartNumber != Items[slot.ItemStartNumber + InventoryWidth * i + j].ItemStartNumber) {
								itemStartNumber = Items[slot.ItemStartNumber + InventoryWidth * i + j].ItemStartNumber;
								counter++;
								foundSlot = Items[slot.ItemStartNumber + InventoryWidth * i + j];
							}
						}
					}
					if(counter == 1) {
						if(SwapItems(foundSlot)) {
							transform.root.GetComponent<AudioSource>().PlayOneShot(slot.Item.ItemSound);
							OnMouseEnter(slot.gameObject);
						}
					}
				}
				else {
					if(DragItemFromSlot(slot)) {
						RemoveItemFromSlot(slot);
						Tooltip.HideTooltip();
					}
				}
			}
			else {
				if(Dragging) {
					if(AddItemAtSlot(DraggedItem, slot)) {
						transform.root.GetComponent<AudioSource>().PlayOneShot(slot.Item.ItemSound);
						StopDragging();
						OnMouseEnter(slot.gameObject);
					}
				}
				else {
					//Может вставить звук для пустого слота?
					//transform.root.audio.PlayOneShot(emptySound);
				}
			}
		}
		else if(mouseIndex == 1) {
			if(slot.Item.ItemName != "") {
				if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					if(slot.Item.StackSize <= 1) {
						return;
					}
					ItemToSplit = slot;
					SplitWindow.SetActive(true);
					SplitWindow.GetComponent<SplitWindow>().Start();
				}
			}
			else {
				//transform.root.audio.PlayOneShot(emptySound);
			}
		}
	}





	public void OnMouseEnter(GameObject obj) 
	{
		ItemClass item = new ItemClass();

		if(obj.GetComponent<InventorySlot>())
		{
			item = obj.GetComponent<InventorySlot>().Item;
			for(int i = 0; i < Items.Count; i++) //TODO
			{
				if(!Tooltip.ShowTooltip)
				{
					StartCoroutine(Tooltip.Show_Tooltip(true, item, SlotType.Inventory, obj.GetComponent<InventorySlot>().ItemStartNumber, obj.GetComponent<RectTransform>()));
				}
				else if(obj.GetComponent<EquipmentSlot>()) {
					item = obj.GetComponent<EquipmentSlot>().Item;
				}

				if(item.ItemName == "" || Dragging) {
					return;
				}

				if(obj.GetComponent<EquipmentSlot>()) {
					StartCoroutine(Tooltip.Show_Tooltip(true, item, SlotType.Equipment, 0, obj.GetComponent<RectTransform>()));
				}
			}
		}
	}

	public void OnMouseExit(GameObject obj)
	{
		Tooltip.HideTooltip();
	}


	public bool EquipItem(InventorySlot slot) 
	{
		if(slot.Item.ItemType == EquipmentSlotType.OffHand) 
		{
			for(int i = 0; i < EquipmentSlots.Count; i++) 
			{
				if(EquipmentSlots[i].EquipmentSlotType == EquipmentSlotType.Weapon) 
				{
					if(EquipmentSlots[i].Item.TwoHanded) 
					{
						AddItem(EquipmentSlots[i].Item);
						RemoveEquippedItem(EquipmentSlots[i]);
					}
				}
			}
		}
		if(slot.Item.ItemType == EquipmentSlotType.Weapon && slot.Item.TwoHanded) 
		{
			for(int i = 0; i < EquipmentSlots.Count; i++) 
			{
				if(EquipmentSlots[i].EquipmentSlotType == EquipmentSlotType.OffHand) 
				{
					if(EquipmentSlots[i].Item.ItemType == EquipmentSlotType.OffHand) 
					{
						AddItem(EquipmentSlots[i].Item);
						RemoveEquippedItem(EquipmentSlots[i]);
					}
				}
			}
		}
		for(int i = 0; i < EquipmentSlots.Count; i++) 
		{
			if(EquipmentSlots[i].EquipmentSlotType == slot.Item.ItemType) 
			{
				if(EquipmentSlots[i].Item.ItemName == "") 
				{
					EquipmentSlots[i].Item = DeepCopy(slot.Item);
					SetSlotImageSprite(EquipmentSlots[i], EquipmentSlots[i].Item.Icon);
					EquipmentSlots[i].transform.FindChild("ItemBackground").gameObject.SetActive(false);
					return true;
				}
				else 
				{
					ItemClass item = DeepCopy(EquipmentSlots[i].Item);
					ItemClass tempSlotItem = DeepCopy(slot.Item);
					if(CheckItemFit(item,Items[slot.ItemStartNumber], true)) 
					{
						int startNumber = slot.ItemStartNumber;
						RemoveItemFromSlot(Items[startNumber]);
						AddItemAtSlot(item,Items[startNumber]);
						EquipmentSlots[i].Item = tempSlotItem;
						SetSlotImageSprite(EquipmentSlots[i], EquipmentSlots[i].Item.Icon);
						transform.root.GetComponent<AudioSource>().PlayOneShot(item.ItemSound);
						EquipmentSlots[i].transform.FindChild("ItemBackground").gameObject.SetActive(false);
						StartCoroutine(Tooltip.Show_Tooltip(true, slot.Item, SlotType.Inventory, slot.ItemStartNumber, slot.GetComponent<RectTransform>()));
						return false;
					}
					else 
					{
						RemoveItemFromSlot(slot);
						EquipmentSlots[i].Item = tempSlotItem;
						AddItem(item);
						SetSlotImageSprite(EquipmentSlots[i], EquipmentSlots[i].Item.Icon);
						transform.root.GetComponent<AudioSource>().PlayOneShot(item.ItemSound);
						OnMouseEnter(slot.gameObject);
						EquipmentSlots[i].transform.FindChild("ItemBackground").gameObject.SetActive(false);
						StartCoroutine(Tooltip.Show_Tooltip(true, slot.Item, SlotType.Inventory, slot.ItemStartNumber, slot.GetComponent<RectTransform>()));
						return false;
					}
				}
			}
		}
		return false;
	}

	public void EquipItemAtSlot(EquipmentSlot slot, ItemClass item) {
		slot.Item = DeepCopy(item);

		SetSlotImageSprite(slot, item.Icon);
		
		slot.transform.FindChild("ItemBackground").gameObject.SetActive(false);

		if(slot.Item.ItemType == EquipmentSlotType.OffHand) {
			for(int i = 0; i < EquipmentSlots.Count; i++) {
				if(EquipmentSlots[i].EquipmentSlotType == EquipmentSlotType.Weapon) {
					if(EquipmentSlots[i].Item.TwoHanded) {
						AddItem(EquipmentSlots[i].Item);
						RemoveEquippedItem(EquipmentSlots[i]);
					}
				}
			}
		}
		if(slot.Item.ItemType == EquipmentSlotType.Weapon && slot.Item.TwoHanded) {
			for(int i = 0; i < EquipmentSlots.Count; i++) {
				if(EquipmentSlots[i].EquipmentSlotType == EquipmentSlotType.OffHand) {
					if(EquipmentSlots[i].Item.ItemType == EquipmentSlotType.OffHand) {
						AddItem(EquipmentSlots[i].Item);
						RemoveEquippedItem(EquipmentSlots[i]);
					}
				}
			}
		}
	}

	public void EquipItemAtSlot(EquipmentSlot slot) {
		slot.Item = DeepCopy(DraggedItem);
		SetSlotImageSprite(slot, DraggedItem.Icon);
		slot.transform.FindChild("ItemBackground").gameObject.SetActive(false);
		StopDragging();
		StartCoroutine(Tooltip.Show_Tooltip(true, slot.Item, SlotType.Equipment,0, slot.GetComponent<RectTransform>()));

		if(slot.Item.ItemType == EquipmentSlotType.OffHand) {
			for(int i = 0; i < EquipmentSlots.Count; i++) {
				if(EquipmentSlots[i].EquipmentSlotType == EquipmentSlotType.Weapon) {
					if(EquipmentSlots[i].Item.TwoHanded) {
						AddItem(EquipmentSlots[i].Item);
						RemoveEquippedItem(EquipmentSlots[i]);
					}
				}
			}
		}
		if(slot.Item.ItemType == EquipmentSlotType.Weapon && slot.Item.TwoHanded) {
			for(int i = 0; i < EquipmentSlots.Count; i++) {
				if(EquipmentSlots[i].EquipmentSlotType == EquipmentSlotType.OffHand) {
					if(EquipmentSlots[i].Item.ItemType == EquipmentSlotType.OffHand) {
						AddItem(EquipmentSlots[i].Item);
						RemoveEquippedItem(EquipmentSlots[i]);
					}
				}
			}
		}
	}

	public bool CheckItemFit(ItemClass item, InventorySlot slot, bool skipLastCheck) //TODO
	{
		for(int i = 0; i < item.Height; i++)
		{
            for(int j = 0; j < item.Width; j++)
            {
                if(slot.ItemStartNumber + InventoryWidth * i + j >= Items.Count)
                {
                    return false;
                }
                for(int k = 0; k < item.Height; k++)
                {
                    if(slot.ItemStartNumber + InventoryWidth *k + j != slot.ItemStartNumber + InventoryWidth *k)
                    {
                        if(((slot.ItemStartNumber + InventoryWidth * i + j) % InventoryWidth == 0) && item.Width != 1)
                        {
                            return false;
                        }
                    }
                }
                if(!skipLastCheck)
                {
                    if(Items[slot.ItemStartNumber + InventoryWidth * i +j].ItemStartNumber != slot.ItemStartNumber + InventoryWidth * i + j)
                    {
                        return false;
                    }
                }
                else
                {
                    List<int> counter = new List<int>();
                    for(int l = 0; l < item.Height; l++)
                    {
                        for(int m = 0; m < item.Width; m++)
                        {
                            if((slot.ItemStartNumber + InventoryWidth * (item.Height -1) + (item.Width -1)) < Items.Count - 1 && Items[slot.ItemStartNumber + InventoryWidth * l + m].ItemStartNumber != slot.ItemStartNumber && Items[slot.ItemStartNumber + InventoryWidth * l + m].Item.ItemName != "" && !counter.Contains(Items[slot.ItemStartNumber + InventoryWidth * l + m].ItemStartNumber))
                            {
                                counter.Add(Items[slot.ItemStartNumber + InventoryWidth *l + m].ItemStartNumber);
                            }
                        }
                    }
                    if(counter.Count > 1)
                    {
                        return false;
                    }
                    else if(counter.Count == 1)
                    {
                        return true;
                    }
                }
            }
		}
        return true;
	}

	public void ResetItemStartNumbers()
	{
		for(int i = 0; i < Items.Count; i++)
		{
			if(Items[i].Item.ItemName == "")
			{
				Items[i].ItemStartNumber = i;
			}
		}
	}

	public void OnEquipmentSlotClick(GameObject obj, int mouseIndex)
	{

		EquipmentSlot slot = obj.GetComponent<EquipmentSlot> ();

		if (mouseIndex == 0) 
		{
			if(Dragging)
			{
				if(DraggedItem.ItemType == slot.EquipmentSlotType) {
					if(slot.Item.ItemName == "") {
						transform.root.GetComponent<AudioSource>().PlayOneShot(DraggedItem.ItemSound);
						EquipItemAtSlot(slot);
					}
					else {
						transform.root.GetComponent<AudioSource>().PlayOneShot(DraggedItem.ItemSound);
						DragSwapEquippedItem(slot);
					}
				}
				else if(AutoFindEquipmentSlot)
				{
					for(int i = 0; i < EquipmentSlots.Count; i++) {
						if(Player.CanDualWield && DraggedItem.ItemType == EquipmentSlotType.Weapon) {
							if(EquipmentSlots[i].EquipmentSlotType == DraggedItem.ItemType) {
								if(EquipmentSlots[i].Item.ItemName == "") {
									transform.root.GetComponent<AudioSource>().PlayOneShot(DraggedItem.ItemSound);
									EquipItemAtSlot(EquipmentSlots[i], DraggedItem);
									StopDragging();
								}
								else {
									for(int j = 0; j < EquipmentSlots.Count; j++) {
										if(EquipmentSlots[j].EquipmentSlotType == EquipmentSlotType.OffHand) {
											if((!DraggedItem.TwoHanded && EquipmentSlots[j].Item.ItemName == "") || (DraggedItem.TwoHanded && EquipmentSlots[j].Item.ItemName == "")) {
												transform.root.GetComponent<AudioSource>().PlayOneShot(DraggedItem.ItemSound);
												EquipItemAtSlot(EquipmentSlots[j], DraggedItem);
												StopDragging();
											}
											else if((DraggedItem.TwoHanded && EquipmentSlots[j].Item.ItemName != "") || (DraggedItem.ItemType == EquipmentSlotType.Weapon && Player.CanDualWield && EquipmentSlots[j].Item.ItemName != "")) {
												transform.root.GetComponent<AudioSource>().PlayOneShot(DraggedItem.ItemSound);
												DragSwapEquippedItem(EquipmentSlots[j]);
												return;
											}
											else {
												transform.root.GetComponent<AudioSource>().PlayOneShot(DraggedItem.ItemSound);
												DragSwapEquippedItem(EquipmentSlots[i]);
											}
										}
									}
								}
							}
						}
						else {
							if(EquipmentSlots[i].EquipmentSlotType == DraggedItem.ItemType) {
								transform.root.GetComponent<AudioSource>().PlayOneShot(DraggedItem.ItemSound);
								if(EquipmentSlots[i].Item.ItemName == "") {
									EquipItemAtSlot(EquipmentSlots[i], DraggedItem);
									StopDragging();
								}
								else {
									transform.root.GetComponent<AudioSource>().PlayOneShot(DraggedItem.ItemSound);
									DragSwapEquippedItem(EquipmentSlots[i]);
								}
							}
						}
					}
				}
			}
		}

	}

	public Color FindColor(ItemClass item)
	{

		if(item.ItemQuality == ItemQuality.Junk)
		{
			return JunkColor;
		}
		if(item.ItemQuality == ItemQuality.Normal)
		{
			return NormalColor;
		}
		if(item.ItemQuality == ItemQuality.Unusual)
		{
			return UnusualColor;
		}
		if(item.ItemQuality == ItemQuality.Rare)
		{
			return RareColor;
		}
		if(item.ItemQuality == ItemQuality.Legendary)
		{
			return LegendaryColor;
		}
		return Color.clear;
	}

	//Создает копию предмета
	public static ItemClass DeepCopy(ItemClass obj)
	{
        if(obj == null)
        {
            throw new ArgumentNullException("Object Cannot be null");
        }
		GameObject oj = obj.DroppedItem;
		Sprite tempTex = obj.Icon;
		obj.Icon = null;
		AudioClip clip = obj.ItemSound;
		AudioClip useClip = obj.UseSound;
		obj.ItemSound = null;
		ItemClass i = (ItemClass)Process(obj);
		i.DroppedItem = oj;
		i.Icon = tempTex;
		obj.ItemSound = clip;
		obj.UseSound = useClip;
		i.ItemSound = clip;
		i.UseSound = useClip;
		DestroyImmediate(GameObject.Find("New Game Object"));
		return i;
	}

	static object Process(object obj) {
		if(obj == null)
		{
			return null;
		}
		Type type = obj.GetType();
		if(type.IsValueType || type == typeof(string)) 
		{
			return obj;
		}

		else if(type.IsArray) 
		{
			Type elementType = Type.GetType(type.FullName.Replace("[]", string.Empty));
			var array = obj as Array;
			Array copied = Array.CreateInstance(elementType, array.Length);
			for(int i = 0; i < array.Length; i++) 
			{
				copied.SetValue(Process(array.GetValue(i)),i);
			}
			return Convert.ChangeType(copied, obj.GetType());
		}
		else if(type.IsClass) {
			object toret = Activator.CreateInstance(obj.GetType());
			FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach(FieldInfo field in fields) 
			{
				object fieldValue = field.GetValue(obj);
				if(fieldValue == null)
				{
					continue;
				}
				field.SetValue(toret, Process(fieldValue));
			}
			return toret;
		}
		else
		{
			throw new ArgumentException("Unknown type");
		}
	}
}
