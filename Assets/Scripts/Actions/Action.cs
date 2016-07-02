using UnityEngine;

/*public abstract class Action
{
    public TempItem[] RequiredItems;

    public Sprite Sprite;
}*/

public abstract class Action
{
    public Sprite Sprite;
    //[SerializeField]
    //Sprite Sprite_;
    //public Sprite Sprite{get{return Sprite_;}private set{Sprite_=value;}}
    [SerializeField, Range(0, 1)]
    protected float Chance;
    [SerializeField]
    public float StaminaConsumption;
}
