using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Item
{

    public string Name;

    public int Id;

    public byte Height;
    public byte Width;


    public byte Level;
    public byte LevelReq;

    public ItemQuality ItemQuality;

    public AudioClip ItemSound;

    public bool Stackable;
    public int StackSize;
    public int MaxStackSize;
    public int HealAmount;
    public bool Consumable;
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

    public int Cooldown;

    public Sprite Icon;

    public int Damage;
    public int CriticalDamage;
    public float AttackSpeed;
    public float LifeSteal;
    public int Range;

    [Tooltip("Указывается эффект из скрипта")]
    public string UseEffectScriptName;


    public int Armor;
    public int BlockAmount;
    public int BlockChance;

    [SerializeField]
    public List<ItemAttribute> ItemAttributes = new List<ItemAttribute>();

    [Space(20)]
    public int ItemStartNumber;



    public Item ItemCopy()
    {
        return (Item)this.MemberwiseClone();
    }


}

