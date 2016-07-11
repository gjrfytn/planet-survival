using UnityEngine;

[System.Serializable]
public sealed class TimedAction : Action
{
    [System.Serializable]
    public class ProducedItem
    {
        [SerializeField]
        uint ItemID_;
        public uint ItemID { get { return ItemID_; } }
        [SerializeField]
        ushort Quantity_;
        public ushort Quantity { get { return Quantity_; } }
    }

    public ushort Duration;
    public float WaterConsumption;
    public float FoodConsumption;

    [SerializeField]
    ProducedItem[] ProducedItems_;
    public ProducedItem[] ProducedItems { get { return ProducedItems_; } }


    void Awake()
    {
        if (ProducedItems_ != null) //TODO ?
        {
            for (byte i = 0; i < ProducedItems_.Length; ++i)
                for (byte j = 0; j < ProducedItems_.Length; ++j)
                    if (ProducedItems_[i].ItemID == ProducedItems_[j].ItemID && i != j)
                        throw new System.Exception("Duplicatated IDs in ProducedItems_.");
        }
        else
            ProducedItems_ = new ProducedItem[0];
    }
}
