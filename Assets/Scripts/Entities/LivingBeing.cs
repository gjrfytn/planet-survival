using UnityEngine;

public abstract class LivingBeing : Entity
{
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

    public bool Fighting { get; protected set; }

    [SerializeField]
    protected float MoveAnimTime;//TODO Вынести?
    [SerializeField, Range(0, 255)]
    byte MaxHealth_;
    public byte MaxHealth { get { return MaxHealth_; } private set { MaxHealth_ = value; } }
    [SerializeField]
    protected TempWeapon BaseWeapon;
    [SerializeField, Range(0, 255)]
    protected byte Speed;
    protected bool MakingTurn;
    [SerializeField]
    ushort Experience;

    [SerializeField, Range(0, 255)]
    byte ViewRange_;
    public byte ViewRange { get { return ViewRange_; } private set { ViewRange_ = value; } }
    [SerializeField, Range(0, 255)]
    byte Initiative_;
    public byte Initiative { get { return Initiative_; } private set { Initiative_ = value; } }

    [SerializeField]
    Container Corpse;

    protected override void Start()
    {
        base.Start();
        Debug.Assert(Corpse.GetComponent<Container>() != null);
        Health = MaxHealth; //TODO C# 6.0 инициализаторы свойств
    }

    public abstract void TakeDamage(byte damage, bool applyArmor, bool applyDefenceAction);

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
        EventManager.OnCreatureDeath();
    }

    public abstract void MakeTurn();
}
