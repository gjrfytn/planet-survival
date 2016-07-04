using UnityEngine;

[System.Serializable]
public class TempWeapon : TempItem
{
    public float Damage;
    public float StaminaCost;
    [Range(1, 255)]
    public byte Range;
    public TempItem Ammo;
    public bool TwoHanded;

    public Sprite NormalHitSprite;
    public Sprite PowerHitSprite;
    public Sprite RareHitSprite;
}
