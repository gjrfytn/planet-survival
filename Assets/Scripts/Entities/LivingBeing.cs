using UnityEngine;

public abstract class LivingBeing : Entity
{
    public float MoveAnimTime;
    [Range(0, 255)]
    public byte MaxHealth;
    [Range(0, 255)]
    public byte BaseDamage;
    public const float DamageSpread = 0.1f;
    [Range(0, 1)]
    public float BaseAccuracy;
    [Range(0, 1)]
    public float BaseArmor;
    public ushort Experience;

    protected byte Health_;
    public virtual byte Health
    {
        get
        {
            return Health_;
        }
        protected set
        {
            Health_ = value;
            if (Health_ == 0/*TODO если float, то <=*/)
                Destroy();
        }
    }

    protected virtual void Start()
    {
        Health = MaxHealth; //TODO C# 6.0 инициализаторы свойств
    }

    public void TakeDamage(byte damage, bool applyArmor)
    {
        //Debug.Assert(damage >= 0);
        EventManager.OnCreatureHit(this, damage);
        float buf = Health - damage * (1 - BaseArmor);
        Health = (byte)(buf > 0 ? buf : 0);//TODO Если Health не float
    }

    public void TakeHeal(byte heal)//C#6.0 EBD
    {
        //Debug.Assert(heal >= 0);
        Health = (byte)Mathf.Clamp(Health + heal, 0, MaxHealth);
    }
}
