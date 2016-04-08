using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryController : MonoBehaviour {

	public GameObject InventoryPanel = null;
	public GameObject EquipmentPanel = null;
	//public GameObject CraftPanel = null;

	public KeyCode InventoryKeyCode;
	public KeyCode EquipmentKeyCode;
	//public KeyCode CraftKeyCode;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
		
	public void OpenInventory()
	{
		if(Input.GetKey(InventoryKeyCode) || Input.GetKey(EquipmentKeyCode) /*|| Input.GetKey(CraftKeyCode)*/)
		{
			InventoryPanel.SetActive(true);
			EquipmentPanel.SetActive(true);
			//CraftPanel.SetActive(true);
		}
	}

	public void CloseInventory()
	{
		if(Input.GetKey(InventoryKeyCode) || Input.GetKey(EquipmentKeyCode) /*|| Input.GetKeyDown(CraftKeyCode)*/)
		InventoryPanel.SetActive(false);
		EquipmentPanel.SetActive(false);
		//CraftPanel.SetActive(false);;
	}

}
