using UnityEngine;

public abstract class Action : MonoBehaviour
{
    public float StaminaConsumption;
    public Sprite AdditionalSprite;

    [SerializeField]
    ItemActionType[] RequiredItemActionTypes_;
    public ItemActionType[] RequiredItemActionTypes { get { return RequiredItemActionTypes_; } }
    [SerializeField]
    Sprite Sprite_;
    public Sprite Sprite { get { return Sprite_; } set { Sprite_ = value; } } //TODO Временно
    [SerializeField, Range(0, 1)]
    float Chance_;
    public float Chance { get { return Chance_; } }
}
