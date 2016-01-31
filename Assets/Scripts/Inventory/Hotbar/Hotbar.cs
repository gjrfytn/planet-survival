using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;

public class Hotbar : MonoBehaviour
{

    [SerializeField]
    public KeyCode[] keyCodesForSlots = new KeyCode[999];
    [SerializeField]
    public int slotsInTotal;

#if UNITY_EDITOR
	[MenuItem("Game Modules/Inventory Module/Create/Hotbar")] 
    public static void menuItemCreateInventory()  
    {
		GameObject Canvas = GameObject.FindGameObjectWithTag("Canvas");
		if (Canvas == null)
        {
            GameObject inventory = new GameObject();
            inventory.name = "Inventories";
			Canvas = Instantiate(Resources.Load("Prefabs/Inventory/Canvas - Inventory")) as GameObject;
            Canvas.transform.SetParent(inventory.transform, true);
			Instantiate(Resources.Load("Prefabs/Inventory/EventSystem"));
        }
        else
            DestroyImmediate(GameObject.FindGameObjectWithTag("DraggingItem"));

		GameObject panel = Instantiate(Resources.Load("Prefabs/Inventory/Panel - Hotbar")) as GameObject;
		panel.transform.SetParent(Canvas.transform, true);
		panel.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
		panel.AddComponent<Hotbar>();
		panel.AddComponent<InventoryDesign>();
		GameObject draggingItem = Instantiate(Resources.Load("Prefabs/Inventory/DraggingItem")) as GameObject;
		draggingItem.transform.SetParent(Canvas.transform, true);
		Inventory inv = panel.AddComponent<Inventory>();
		inv.getPrefabs();
    }
#endif

    void Update()
    {
        for (int i = 0; i < slotsInTotal; i++)
        {
            if (Input.GetKeyDown(keyCodesForSlots[i]))
            {
                if (transform.GetChild(1).GetChild(i).childCount != 0 && transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ItemOnObject>().item.itemType != ItemType.UFPS_Ammo)
                {
                    if (transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ConsumeItem>().duplication != null && transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ItemOnObject>().item.maxStack == 1)
                    {
                        Destroy(transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ConsumeItem>().duplication);
                    }
                    transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ConsumeItem>().consumeIt();
                }
            }
        }
    }

    public int getSlotsInTotal()
    {
        Inventory inv = GetComponent<Inventory>();
        return slotsInTotal = inv.width * inv.height;
    }
}
