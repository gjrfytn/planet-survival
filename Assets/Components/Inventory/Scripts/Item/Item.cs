using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Item
{
    public string Name;

    bool IdSet = false;
    uint Id_;
    public uint Id
    {
        get
        {
            return Id_;
        }
        set
        {
            if (IdSet)
                throw new System.InvalidOperationException("Id has already been set.");
            else
            {
                Id_ = value;
                IdSet = true;
            }
        }
    }

    public byte Height;
    public byte Width;


    public byte Level;
    public byte LevelReq;

    public ItemQuality ItemQuality;

    public AudioClip ItemSound;

    public bool IsStackable;
    public int StackSize;
    public int MaxStackSize;
    public AudioClip UseSound;

    [Multiline]
    public string Description;

    public GameObject DroppedItem;
    public GameObject CustomObject;

    public int StartSlot;
    public bool IsEquipment;
    public bool IsConsumable;
    public bool TwoHanded;

    public ItemType ItemType;

	public ItemActionType ItemActionType;

    public int Cooldown; //Задержка перед использованием

    public Sprite Icon;

    //Weapon
    public int Damage;
    public int CriticalDamage;
    public float AttackSpeed;
    //public float LifeSteal;
    public int Range;

    [Tooltip("Указывается эффект из скрипта")]
    public string UseEffectScriptName;

    //Armor
    public float Armor;
    public int BlockAmount;
    public float BlockChance;
    public int ColdResistance;
    public int HeatResistance;

    //Consumable
    public int HealthRestore;
    public int EnergyRestore;

    public int ReturnItemId;
    public int ReturnItemStackSize;
    [SerializeField]
    public List<ItemAttribute> ItemAttributes = new List<ItemAttribute>();

    [Space(20)]
    public int ItemStartNumber;



    public Item ItemCopy()
    {
        return (Item)this.MemberwiseClone();
    }


}

