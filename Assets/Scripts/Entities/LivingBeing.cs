using UnityEngine;

public abstract class LivingBeing : Entity
{
    [SerializeField]
    protected float MoveAnimTime;//TODO Вынести?
    [SerializeField, Range(0, 255)]
    byte MaxHealth_;
    public byte MaxHealth { get { return MaxHealth_; } private set { MaxHealth_ = value; } }
    [SerializeField]
    protected TempWeapon BaseWeapon;
    [SerializeField, Range(0, 1)]
    float BaseArmor;
    [SerializeField, Range(0, 255)]
    protected byte Speed;
    [SerializeField]
    ushort Experience;

    [SerializeField]
    Container Corpse;

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

    protected override void Start()
    {
        base.Start();
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
