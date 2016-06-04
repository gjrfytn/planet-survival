using UnityEngine;

[System.Serializable]
public class TempItem
{

}

[System.Serializable]
public class TempWeapon : TempItem
{
    public Sprite NormalHitSprite;
    public Sprite PowerHitSprite;
    public Sprite RareHitSprite;
    public float NormalHitDamage;
    public float PowerHitDamage;
    public float RareHitDamage;
    public float NormalHitStaminaCost;
    public float PowerHitStaminaCost;
    public float RareHitStaminaCost;
}
