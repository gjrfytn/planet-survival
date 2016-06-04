using UnityEngine;

public abstract class Action
{
    public TempItem[] RequiredItems;

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
    public float StaminaConsumption;
}
