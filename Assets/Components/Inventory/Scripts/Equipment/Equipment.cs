using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Equipment : MonoBehaviour, IDropHandler {

    public InventoryManager InventoryManager;

    [HideInInspector]
    public EquipmentItemType EquipmentItemType;
    [HideInInspector]
    public ItemType ItemType;

    //public Text ArmorText;
    //public Text HeatResistText;
    //public Text ColdResistText;
    //public Text DamageText;
    [Space(20)]
    public Transform SlotContainer;

    public List<GameObject> Slots;

    //Player Player;
    // Use this for initialization
    void Awake()
    {
        //Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        AttachedItem.Equipment = GetComponent<Equipment>();

        /*for (int i = 0; i < SlotContainer.childCount; i++)
        {
            Slots.Add(SlotContainer.GetChild(i).gameObject);
        }*/
    }

    public void CreateEquipmentSlots()
    {
        string[] equipmentTypes = System.Enum.GetNames(typeof(EquipmentItemType));
        RemoveEquipmentSlots();
        for (int i = 0; i < equipmentTypes.Length; i++)
        {
            GameObject slot = (GameObject)Instantiate(InventoryManager.EquipmentSlotPrefab);
            slot.GetComponent<EquipmentSlot>().SlotType = SlotType.Equipment;
            slot.transform.SetParent(SlotContainer.transform);
            slot.name = (EquipmentItemType)i + " Slot";
            Slots.Add(slot);
            //Slots[i].gameObject.transform.GetComponent<Slot>().SlotNumber += i;
            }

    }
    /*for (int i = 0; i < height * width; i++)
    {
        GameObject slot = (GameObject)Instantiate(InventoryManager.EquipmentSlotPrefab);
        slot.transform.SetParent(SlotContainer.transform);
        slot.name = "Slot " + i;
        //Slots.Add(slot);
        //Slots[i].gameObject.transform.GetComponent<Slot>().SlotNumber += i;
    }*/

    /*public void AddEquipmentSlot()
    {

    }*/

    public void RemoveEquipmentSlots()
    {

        for (int i = 0; i < Slots.Count; i++)
        {
            DestroyImmediate(Slots[i].gameObject);
        }
        Slots = new List<GameObject>();
    }

    public void OnDrop(PointerEventData data)
    {
        InventoryManager.Inventory.OnDrop(data);
    }
}
