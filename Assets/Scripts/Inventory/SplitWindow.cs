using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SplitWindow : MonoBehaviour {

	public InputField InputField;
	public Slider Slider;

	private int SplitSize;
	private Inventory Inventory;

	// Use this for initialization
	public void Start () {
	
		Inventory = GameObject.FindGameObjectWithTag("Invnetory").GetComponent<Inventory>();
		SplitSize = 1;
		Slider.minValue = 1;
		Slider.maxValue = Inventory.ItemToSplit.Item.StackSize - 1;
		Slider.value = SplitSize;
		InputField.text = SplitSize.ToString();
		Slider.value = Slider.minValue;

	}
	
	// Update is called once per frame
	void Update () {
	
		if(Input.GetKeyDown(KeyCode.Return))
		{
			Accept();
		}

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Cancel();
		}
	}

	public void UpdateInputField()
	{
		if(!string.IsNullOrEmpty(InputField.text) && int.Parse(InputField.text) <= Inventory.ItemToSplit.Item.StackSize -1)
		{
			SplitSize = int.Parse(InputField.text);
		}
		else if(string.IsNullOrEmpty(InputField.text))
		{
			SplitSize = 0;
		}
		else
		{
			SplitSize = Inventory.ItemToSplit.Item.StackSize -1;
			InputField.text = SplitSize.ToString();
		}
	}

	public void UpdateSlider()
	{
		SplitSize = (int)Slider.value;
		InputField.text = SplitSize.ToString();
	}

	public void Accept()
	{
		if(Inventory.ItemToSplit.Item.ItemName != "")
		{
			ItemClass item = Inventory.DeepCopy(Inventory.ItemToSplit.Item);
			item.StackSize = SplitSize;
			Inventory.ItemToSplit.Item.StackSize -= SplitSize;
			Inventory.ItemToSplit.StackSizeText.text = Inventory.ItemToSplit.Item.StackSize.ToString();
			Inventory.DraggedItem = item;
			Inventory.Dragging = true;
			Inventory.DragItem.sprite = item.Icon;
			Inventory.DragItem.gameObject.SetActive(true);
			Inventory.DragItem.rectTransform.sizeDelta = new Vector2(item.Width * Inventory.SlotIconSize, item.Height * Inventory.SlotIconSize);
			this.gameObject.SetActive(false);
		}
	}
	public void Cancel()
	{
		this.gameObject.SetActive(false);
	}
}
