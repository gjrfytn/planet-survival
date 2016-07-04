using UnityEngine;

[System.Serializable]
public sealed class AttackAction : Action
{
    public LivingBeing Target;
    public float DamageMultiplier;
    public float StaminaCostMultiplier;
    public UnityEngine.Sprite WeaponSprite;
}
