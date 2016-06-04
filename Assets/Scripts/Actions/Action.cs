using UnityEngine;

public abstract class Action
{
    public Sprite Sprite;
}

[System.Serializable]
public sealed class BattleAction : Action
{
    public float Damage;
    public float Accuracy;
    public float StaminaCost;
}

[System.Serializable]
public sealed class TimedAction : Action
{
    public ushort Duration;
    public float WaterConsumption;
    public float FoodConsumption;
    public float EnergyConsumption;

    //На будущее
    /*public readonly ushort Duration;
    public readonly float WaterConsumption;
    public readonly float FoodConsumption;
    public readonly float EnergyConsumption;

    public readonly List<EffectApplier> StartEffects;
    public readonly List<EffectApplier> EndEffects;

    public Action(ushort duration,float waterConsumption,float foodConsumption,float energyConsumption,List<EffectApplier> startEffects,List<EffectApplier> endEffects)
    {
        Duration=duration;
        WaterConsumption=waterConsumption;
        FoodConsumption=foodConsumption;
        EnergyConsumption=energyConsumption;
        StartEffects=startEffects;
        EndEffects=endEffects;
    }*/
}
