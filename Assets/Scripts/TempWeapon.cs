using UnityEngine;

[System.Serializable]
public class TempWeapon : TempItem
{
	public float NormalHitDamage;
	public float PowerHitDamage;
	public float RareHitDamage;
	public float NormalHitStaminaCost;
	public float PowerHitStaminaCost;
	public float RareHitStaminaCost;
	[Range(1,255)]
	public byte Range;
	public TempItem Ammo;
	public bool TwoHanded;

	public Sprite NormalHitSprite;
	public Sprite PowerHitSprite;
	public Sprite RareHitSprite;
}