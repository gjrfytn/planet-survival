using UnityEngine;

public abstract class Action : MonoBehaviour
{
    public ItemActionType[] RequiredItemActionTypes;
    public Sprite Sprite;
    public Sprite AdditionalSprite;
    [Range(0, 1)]
    public float Chance;
    public float StaminaConsumption;
}
