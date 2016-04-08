using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ItemClass {

	public string ItemName;
	public string ID;
	public int BaseID;
	public int Height;
	public int Width;

	public int Level;

	//Условные цены
	public int SellPrice;
	public int BuyPrice;

	public ItemQuality ItemQuality;

	public AudioClip ItemSound;

	public bool Stackable;
	public int StackSize;
	public int MaxStackSize;
	public int HealAmount;
	public bool Consumable;
	public AudioClip UseSound;
	public string UseEffectScriptName;

	[SerializeField]
	public List<ItemAttribute> ItemAttributes = new List<ItemAttribute>(); 

	public string Tooltip;
	public string TooltipHeader;
	public string DescriptionText;

	public GameObject DroppedItem;

	public int StartSlot;
	public bool TwoHanded;


	public EquipmentSlotType ItemType;
	public WeaponType WeaponType;
	public ConsumableType ConsumableType;

	public int Cooldown;

	public string IconName;
	public Sprite Icon;

	public int Damage;
	public int CriticalDamage;
	public float AttackSpeed;
	public float LifeSteal;
	public int Distance;


	public int Armor; 
	public int BlockAmount;
	public int BlockChance;
}
