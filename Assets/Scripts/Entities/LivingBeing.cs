using UnityEngine;

public abstract class LivingBeing : Entity
{
    public float MoveAnimTime;//TODO Вынести?
    [Range(0, 255)]
    public byte MaxHealth;
    public TempWeapon BaseWeapon;
    [Range(0, 1)]
    public float BaseArmor;
    [Range(0, 255)]
    public byte Speed;
    public ushort Experience;

    public Container Corpse;

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
        Debug.Assert(Corpse.GetComponent<Container>() != null);
        Health = MaxHealth; //TODO C# 6.0 инициализаторы свойств
    }

    public void TakeDamage(byte damage, bool applyArmor)
    {
        //Debug.Assert(damage >= 0);
        float buf = Health - damage * (1 - BaseArmor);
        Health = (byte)(buf > 0 ? buf : 0);//TODO Если Health не float
        EventManager.OnCreatureHit(this, damage);
    }

    public void TakeHeal(byte heal)//C#6.0 EBD
    {
        //Debug.Assert(heal >= 0);
        Health = (byte)Mathf.Clamp(Health + heal, 0, MaxHealth);
        EventManager.OnCreatureHealed(this, heal);
    }

    public override void Destroy()
    {
        base.Destroy();
        Entity corpse = (Instantiate(Corpse, transform.position, Quaternion.identity) as Entity);
        corpse.Pos = Pos;
        EventManager.OnEntitySpawn(corpse);
    }
}
