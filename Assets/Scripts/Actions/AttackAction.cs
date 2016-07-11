using UnityEngine;

[System.Serializable]
public sealed class AttackAction : Action
{
    public LivingBeing Target;

    [SerializeField]
    float DamageMultiplier_;
    public float DamageMultiplier { get { return DamageMultiplier_; } }
    [SerializeField]
    float StaminaCostMultiplier_;
    public float StaminaCostMultiplier { get { return StaminaCostMultiplier_; } }
}
