using UnityEngine;

/*public abstract class Action
{
    public TempItem[] RequiredItems;

    public Sprite Sprite;
}*/

public abstract class Action : MonoBehaviour
{
    public Sprite Sprite;
    //[SerializeField]
    //Sprite Sprite_;
    //public Sprite Sprite{get{return Sprite_;}private set{Sprite_=value;}}
    [Range(0, 1)]
    public float Chance;
    public float StaminaConsumption;
}
