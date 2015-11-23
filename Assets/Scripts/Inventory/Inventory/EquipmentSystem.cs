using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class EquipmentSystem : MonoBehaviour
{
    [SerializeField]
    public int slotsInTotal;
    [SerializeField]
    public ItemType[] itemTypeOfSlots = new ItemType[999];

    void Start()
    {
        ConsumeItem.eS = GetComponent<EquipmentSystem>();
    }

    public void getSlotsInTotal()
    {
        Inventory inv = GetComponent<Inventory>();
        slotsInTotal = inv.width * inv.height;
    }
#if UNITY_EDITOR
    [MenuItem("Inventory System/Create/Equipment")]  
    public static void menuItemCreateInventory() 
    {
		GameObject Canvas = GameObject.FindGameObjectWithTag("Canvas");
		if (Canvas == null)
        {
            GameObject inventory = new GameObject();
            inventory.name = "Inventories";
			Canvas = Instantiate(Resources.Load("Prefabs/Canvas - Inventory")) as GameObject;
            Canvas.transform.SetParent(inventory.transform, true);
            Instantiate(Resources.Load("Prefabs/EventSystem") as GameObject);  
        }
        else
            DestroyImmediate(GameObject.FindGameObjectWithTag("DraggingItem"));

		GameObject draggingItem = Instantiate(Resources.Load("Prefabs/DraggingItem")) as GameObject;
		draggingItem.transform.SetParent(Canvas.transform, true);
		GameObject panel = Instantiate(Resources.Load("Prefabs/Panel - EquipmentSystem")) as GameObject;
		panel.transform.SetParent(Canvas.transform, true);
		panel.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
		panel.AddComponent<InventoryDesign>();
		panel.AddComponent<EquipmentSystem>();
		Inventory inv = panel.AddComponent<Inventory>();
		inv.getPrefabs();
    }
#endif

}

