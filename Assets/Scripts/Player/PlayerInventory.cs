using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerInventory : MonoBehaviour
{
    public GameObject inventory;
    public GameObject characterSystem;
    public GameObject craftSystem;
    public Player PlayerChars;
    //	private Inventory craftSystemInventory;
    //	private CraftSystem cS;
    private Inventory mainInventory;
    //	private Inventory characterSystemInventory;
    //	private Tooltip toolTip;

    private InputManager inputManagerDatabase;

    int normalSize = 3;

    public void OnEnable()
    {
        Inventory.ItemEquip += OnBackpack;
        Inventory.UnEquipItem += UnEquipBackpack;

        Inventory.ItemEquip += OnGearItem;
        Inventory.ItemConsumed += OnConsumeItem;
        Inventory.UnEquipItem += OnUnEquipItem;

        Inventory.ItemEquip += EquipWeapon;
        Inventory.UnEquipItem += UnEquipWeapon;
    }

    public void OnDisable()
    {
        Inventory.ItemEquip -= OnBackpack;
        Inventory.UnEquipItem -= UnEquipBackpack;

        Inventory.ItemEquip -= OnGearItem;
        Inventory.ItemConsumed -= OnConsumeItem;
        Inventory.UnEquipItem -= OnUnEquipItem;

        Inventory.UnEquipItem -= UnEquipWeapon;
        Inventory.ItemEquip -= EquipWeapon;
    }

    void EquipWeapon(Item item)
    {
        if (item.itemType == ItemType.Weapon)
        {

        }
    }

    void UnEquipWeapon(Item item)
    {
        if (item.itemType == ItemType.Weapon)
        {

        }
    }

    void OnBackpack(Item item)
    {
        if (item.itemType == ItemType.Backpack)
        {
            for (int i = 0; i < item.itemAttributes.Count; i++)
            {
                if (mainInventory == null)
                    mainInventory = inventory.GetComponent<Inventory>();
                mainInventory.sortItems();
                if (item.itemAttributes[i].attributeName == "Slots")
                    changeInventorySize(item.itemAttributes[i].attributeValue);
            }
        }
    }

    void UnEquipBackpack(Item item)
    {
        if (item.itemType == ItemType.Backpack)
            changeInventorySize(normalSize);
    }

    void changeInventorySize(int size)
    {
        dropTheRestItems(size);

        if (mainInventory == null)
            mainInventory = inventory.GetComponent<Inventory>();
        if (size == 3)
        {
            mainInventory.width = 3;
            mainInventory.height = 1;
        }
        else if (size == 6)
        {
            mainInventory.width = 3;
            mainInventory.height = 2;
        }
        else if (size == 12)
        {
            mainInventory.width = 4;
            mainInventory.height = 3;
        }
        else if (size == 16)
        {
            mainInventory.width = 4;
            mainInventory.height = 4;
        }
        else if (size == 24)
        {
            mainInventory.width = 6;
            mainInventory.height = 4;
        }
        mainInventory.updateSlotAmount(); //Возможно так лучше?
        mainInventory.adjustInventorySize();
    }

    void dropTheRestItems(int size)
    {
        if (size < mainInventory.ItemsInInventory.Count)
        {
            for (int i = size; i < mainInventory.ItemsInInventory.Count; i++)
            {
                GameObject dropItem = (GameObject)Instantiate(mainInventory.ItemsInInventory[i].itemModel);
                dropItem.AddComponent<PickUpItem>();
                dropItem.GetComponent<PickUpItem>().item = mainInventory.ItemsInInventory[i];
                dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
            }
        }
    }

    void Start()
    {

        if (inputManagerDatabase == null)
            inputManagerDatabase = (InputManager)Resources.Load("InputManager");

        //if (craftSystem != null)
        //cS = craftSystem.GetComponent<CraftSystem>();

        //if (GameObject.FindGameObjectWithTag("Tooltip") != null)
        //	toolTip = GameObject.FindGameObjectWithTag("Tooltip").GetComponent<Tooltip>();
        if (inventory != null)
            mainInventory = inventory.GetComponent<Inventory>();
        //if (characterSystem != null)
        //	characterSystemInventory = characterSystem.GetComponent<Inventory>();
        //if (craftSystem != null)
        //	craftSystemInventory = craftSystem.GetComponent<Inventory>();
    }



    public void OnConsumeItem(Item item)
    {
        PlayerChars = GetComponent<Player>();
        for (int i = 0; i < item.itemAttributes.Count; i++)
        {
            if (item.itemAttributes[i].attributeName == "Health")
                PlayerChars.TakeHeal(item.itemAttributes[i].attributeValue);
            if (item.itemAttributes[i].attributeName == "Mana")
            {
                if ((PlayerChars.CurrentEnergy + item.itemAttributes[i].attributeValue) > PlayerChars.MaxEnergy)
                    PlayerChars.CurrentEnergy = PlayerChars.MaxEnergy;
                else
                    PlayerChars.CurrentEnergy += item.itemAttributes[i].attributeValue;
            }
            if (item.itemAttributes[i].attributeName == "Armor")
            {
                //if ((PlayerChars.currentArmor + item.itemAttributes[i].attributeValue) > PlayerChars.maxArmor)
                //	PlayerChars.currentArmor = PlayerChars.maxArmor;
                //else
                PlayerChars.Armor += item.itemAttributes[i].attributeValue;
            }
            if (item.itemAttributes[i].attributeName == "Damage")
            {
                //if ((PlayerChars.currentDamage + item.itemAttributes[i].attributeValue) > PlayerChars.maxDamage)
                //	PlayerChars.currentDamage = PlayerChars.maxDamage;
                //else
                PlayerChars.Damage += item.itemAttributes[i].attributeValue;
            }
        }
    }

    public void OnGearItem(Item item)
    {
        for (int i = 0; i < item.itemAttributes.Count; i++)
        {
            if (item.itemAttributes[i].attributeName == "Health")
                PlayerChars.MaxHealth += item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Energy")
                PlayerChars.MaxEnergy += item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Armor")
                PlayerChars.Armor += item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Damage")
                PlayerChars.Damage += item.itemAttributes[i].attributeValue;
        }
    }

    public void OnUnEquipItem(Item item)
    {
        for (int i = 0; i < item.itemAttributes.Count; i++)
        {
            if (item.itemAttributes[i].attributeName == "Health")
                PlayerChars.MaxHealth -= item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Energy")
                PlayerChars.MaxEnergy -= item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Armor")
                PlayerChars.Armor -= item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Damage")
                PlayerChars.Damage -= item.itemAttributes[i].attributeValue;
        }
    }

    void Update()
    {
        /*if (Input.GetKeyDown(inputManagerDatabase.CharacterSystemKeyCode))
      {
          if (!characterSystem.activeSelf)
          {
              characterSystemInventory.openInventory();
          }
          else
          {
              if (toolTip != null)
                  toolTip.deactivateTooltip();
              characterSystemInventory.closeInventory();
          }
      }*/

        /*if (Input.GetKeyDown(inputManagerDatabase.InventoryKeyCode))
        {
            if (!inventory.activeSelf)
            {
                mainInventory.openInventory();
                characterSystemInventory.openInventory();
            }
            else
            {
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                mainInventory.closeInventory();
                characterSystemInventory.closeInventory();
            }
        }
		
        if (Input.GetKeyDown(inputManagerDatabase.CraftSystemKeyCode))
        {
            if (!craftSystem.activeSelf)
                craftSystemInventory.openInventory();
            else
            {
                if (cS != null)
                    cS.backToInventory();
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                craftSystemInventory.closeInventory();
            }
        }*/
    }
}
