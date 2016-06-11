using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : LivingBeing {

    public GlobalPos GlobalPos;
    public override LocalPos Pos
    {
        get
        {
            if (GlobalPos.X < 0 || GlobalPos.Y < 0 || GlobalPos.X > ushort.MaxValue || GlobalPos.Y > ushort.MaxValue)
                throw new System.InvalidOperationException("Cannot access \"Pos\" while Player is on global map.");
            return (LocalPos)GlobalPos;
        }
        set
        {
            GlobalPos = value;
        }
    }

    [Header("Основные характеристики")]
    [SerializeField, Range(0, 255)]
    byte MaxWater_;
    public byte MaxWater { get { return MaxWater_; } private set { MaxWater_ = value; } }
    [SerializeField, Range(0, 255)]
    byte MaxFood_;
    public byte MaxFood { get { return MaxFood_; } private set { MaxFood_ = value; } }
    [SerializeField, Range(0, 255)]
    byte MaxStamina_;
    public byte MaxStamina { get { return MaxStamina_; } private set { MaxStamina_ = value; } }
    [SerializeField, Range(0, 255)]
    byte MaxMental_;
    public byte MaxMental { get { return MaxMental_; } private set { MaxMental_ = value; } }
    [SerializeField, Range(0, 255)]
    byte MentalRegen_;
    public byte MentalRegen { get { return MentalRegen_; } private set { MentalRegen_ = value; } }

    //TODO Ждём C# 6.0 инициализаторы свойств:
    public override byte Health
    {
        protected set
        {
            Health_ = value;
            if (Health_ == 0/*TODO если float, то <=*/)
            {
                Debug.Log("Персонаж погиб.");
                //UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            }
        }
    }

    float Water_;
    public float Water
    {
        get
        {
            return Water_;
        }
        private set
        {
            Water_ = value;
            if (Water_ <= 0)
                Debug.Log("Персонаж умер от жажды.");
        }
    }

    float Food_;
    public float Food
    {
        get
        {
            return Food_;
        }
        private set
        {
            Food_ = value;
            if (Food_ <= 0)
                Debug.Log("Персонаж умер от голода.");
        }
    }

    float Stamina_;
    public float Stamina
    {
        get
        {
            return Stamina_;
        }
        private set
        {
            Stamina_ = value;
            if (Stamina_ <= 0)
                Debug.Log("Персонаж умер от усталости.");
        }
    }

    byte Mental_;
    public byte Mental
    {
        get
        {
            return Mental_;
        }
        private set
        {
            Mental_ = value;
            if (Mental_ == 0/*TODO если float, то <=*/)
                Debug.Log("Персонаж сошёл с ума.");
        }
    }

    [SerializeField]
    float WaterConsumption;
    [SerializeField]
    float FoodConsumption;
    [SerializeField]
    float StaminaConsumption;

    [System.Serializable]
    class MentalPenalty
    {
        [SerializeField]
        float TopPercent_;
        public float TopPercent { get { return TopPercent_; } private set { TopPercent_ = value; } }
        [SerializeField]
        float Water_;
        public float Water { get { return Water_; } private set { Water_ = value; } }
        [SerializeField]
        float Food_;
        public float Food { get { return Food_; } private set { Food_ = value; } }
        [SerializeField]
        float Stamina_;
        public float Stamina { get { return Stamina_; } private set { Stamina_ = value; } }

        public MentalPenalty(float water, float food, float stamina)
        {
            Water_ = water;
            Food_ = food;
            Stamina_ = stamina;
        }
    }

    [SerializeField]
    MentalPenalty LowMentalPenalty;
    [SerializeField]
    MentalPenalty MediumMentalPenalty;
    [SerializeField]
    MentalPenalty HighMentalPenalty;
    [SerializeField]
    MentalPenalty InsaneMentalPenalty;

    TimedAction CurrentAction;
    [SerializeField]
    bool CanDualWield_;
    public bool CanDualWield { get { return CanDualWield_; } private set { CanDualWield_ = value; } }

    [Header("Дальность обзора")]
    [SerializeField, Range(0, 255)]
    byte ViewDistance_;
    public byte ViewDistance { get { return ViewDistance_; } private set { ViewDistance_ = value; } }

    byte Level;

    float MoveTime;
    Stack<LocalPos> Path = new Stack<LocalPos>();//TODO В LivingBeing?
    public byte RemainingMoves { get; private set; }
    LocalPos NextMovePoint;

    void OnEnable()
    {
        EventManager.ActionStarted += StartAction;
        EventManager.HourPassed += UpdateState;
        EventManager.ActionEnded += EndAction;

        //Inventory
        Inventory.ItemUsed += UseItem;
        Inventory.ItemEquipped += EquipArmor;
        Inventory.ItemEquipped += EquipWeapon;
        Inventory.ItemUnequipped += UnequipArmor;
        Inventory.ItemUnequipped += UnequipWeapon;
    }

    void OnDisable()
    {
        EventManager.ActionStarted -= StartAction;
        EventManager.HourPassed -= UpdateState;
        EventManager.ActionEnded -= EndAction;

        //Inventory
        Inventory.ItemUsed -= UseItem;
        Inventory.ItemEquipped -= EquipArmor;
        Inventory.ItemEquipped -= EquipWeapon;
        Inventory.ItemUnequipped -= UnequipArmor;
        Inventory.ItemUnequipped -= UnequipWeapon;
    }

    protected override void Start()
    {
        base.Start();
        GetComponent<SpriteRenderer>().sortingLayerName = "Player";

        //TODO C# 6.0 инициализаторы свойств:
        Water = MaxWater;
        Food = MaxFood;
        Stamina = MaxStamina;
        Mental = MaxMental;

        RemainingMoves = Speed;
    }

    void Update()
    {
        if (MoveTime > 0)
        {
            float tstep = MoveTime / Time.deltaTime;
            MoveTime -= Time.deltaTime;
            //TODO Возможно стоит сохранять значение из GetTransformPosFromMapPos(MapCoords,World.IsCurrentMapLocal())), так как это улучшит(?) производительность
            if (GameObject.FindWithTag("World").GetComponent<World>().IsCurrentMapLocal())
            {
                float dstep = Vector2.Distance(transform.position, WorldVisualiser.GetTransformPosFromMapPos(NextMovePoint)) / tstep;
                transform.position = Vector2.MoveTowards(transform.position, WorldVisualiser.GetTransformPosFromMapPos(NextMovePoint), dstep);
            }
            else
            {
                float dstep = Vector2.Distance(transform.position, WorldVisualiser.GetTransformPosFromMapPos(GlobalPos)) / tstep;
                transform.position = Vector2.MoveTowards(transform.position, WorldVisualiser.GetTransformPosFromMapPos(GlobalPos), dstep);
            }

            EventManager.OnPlayerObjectMoved();
        }
        else if (Path.Count != 0)
        {
            //LocalPos buf = Pos;
            NextMovePoint = Path.Pop();
            //EventManager.OnCreatureMove(buf, Pos); //TODO name?

            MoveTime = MoveAnimTime;
        }
    }

    public void MoveTo(LocalPos pos/*, bool inBattle*/)//TODO
    {
        List<LocalPos> buf = Pathfinder.MakePath((GameObject.FindWithTag("World").GetComponent<World>().CurrentMap as LocalMap).GetBlockMatrix(), Pos, pos, false);//TODO Тут?
        buf.Reverse();
        Path = new Stack<LocalPos>(buf);
        Path.Pop();

        LocalPos pBuf = Pos;
        Pos = pos;

        EventManager.OnCreatureMove(pBuf, pos);
        RemainingMoves -= (byte)Path.Count;
        if (RemainingMoves == 0)
        {
            RemainingMoves = Speed;
            EventManager.OnPlayerTurn();
            //EventManager.OnCreatureStartTurn();
        }
        else
            GameObject.FindWithTag("World").GetComponent<World>().RerenderBlueHexesOnLocal();
    }

    public void MoveTo(GlobalPos pos, float moveAnimTime)
    {
        MoveTime = moveAnimTime;
        GlobalPos = pos;
        //EventManager.OnPlayerMove(mapCoords); //TODO Временно //Временно закоммент. см. HexInteraction 21
    }

    public void Attack(LivingBeing target, float damage, float accuracy)//C#6.0 EBD
    {
        if (Random.value < accuracy)
            target.TakeDamage((byte)damage, true);
        else
            EventManager.OnAttackMiss(transform.position);
    }

    void StartAction(TimedAction action)
    {
        CurrentAction = action;
    }

    void UpdateState()
    {
        TimedAction terrainCons = GameObject.FindWithTag("World").GetComponent<Terrains>().GetTerrainProperties(GameObject.FindWithTag("World").GetComponent<World>().GetHexTerrain(Pos));
        float mentalPercent = Mental / MaxMental;
        MentalPenalty curPenalty = mentalPercent < InsaneMentalPenalty.TopPercent ? InsaneMentalPenalty : (mentalPercent < HighMentalPenalty.TopPercent ? HighMentalPenalty : (mentalPercent < MediumMentalPenalty.TopPercent ? MediumMentalPenalty : (mentalPercent < LowMentalPenalty.TopPercent ? LowMentalPenalty : new MentalPenalty(0, 0, 0))));
        Water = Water - WaterConsumption - terrainCons.WaterConsumption - curPenalty.Water - (CurrentAction != null ? CurrentAction.WaterConsumption : 0);//C#6.0
        Food = Food - FoodConsumption - terrainCons.FoodConsumption - curPenalty.Food - (CurrentAction != null ? CurrentAction.FoodConsumption : 0);//C#6.0
        Stamina = Stamina - StaminaConsumption - terrainCons.StaminaConsumption - curPenalty.Stamina - (CurrentAction != null ? CurrentAction.StaminaConsumption : 0);//C#6.0
    }

    void EndAction()
    {
        CurrentAction = null;
    }

    public void Drink(float value)//C#6.0 EBD
    {
        Debug.Assert(value >= 0);
        Water = Mathf.Clamp(Water + value, 0, MaxWater);
    }

    public void Eat(float value)//C#6.0 EBD
    {
        Debug.Assert(value >= 0);
        Food = Mathf.Clamp(Food + value, 0, MaxFood);
    }

    public void RestoreEnergy(float value)//C#6.0 EBD
    {
        Debug.Assert(value >= 0);
        Stamina = Mathf.Clamp(Stamina + value, 0, MaxStamina);
    }

    public TempWeapon Weapon;

    public TempWeapon GetWeapon()
    {
        return Weapon == null ? BaseWeapon : Weapon;
    }

    public byte MaxEnergy;
    public byte MaxDamage;

    public byte CurrentHealth;
    public byte CurrentEnergy;
    public byte CurrentArmor;
    public byte CurrentDamage;




	

    public void UseItem(Item item)
    {
        for (int i = 0; i < item.ItemAttributes.Count; i++)
        {
            if (item.ItemAttributes[i].AttributeName == "Health")
            {
                if ((CurrentHealth + item.ItemAttributes[i].AttributeValue) > MaxHealth)
                    CurrentHealth = MaxHealth;
                else
                    CurrentHealth += (byte)item.ItemAttributes[i].AttributeValue;
            }   
            if (item.ItemAttributes[i].AttributeName == "Armor")
            {
                    CurrentArmor += (byte)item.ItemAttributes[i].AttributeValue;
            }
            if (item.ItemAttributes[i].AttributeName == "Damage")
            {
                if ((CurrentDamage + item.ItemAttributes[i].AttributeValue) > MaxDamage)
                    CurrentDamage = MaxDamage;
                else
                    CurrentDamage += (byte)item.ItemAttributes[i].AttributeValue;
            }
        }
    }

    public void EquipArmor(Item item)
    {
        for (int i = 0; i < item.ItemAttributes.Count; i++)
        {
            if (item.ItemAttributes[i].AttributeName == "Health")
            {
                //MaxHealth += (byte)item.ItemAttributes[i].AttributeValue;
            }
            if (item.ItemAttributes[i].AttributeName == "Armor")
            {
                CurrentArmor += (byte)item.ItemAttributes[i].AttributeValue;
            }
        }
        CurrentArmor += (byte)item.Armor;
    }

    public void UnequipArmor(Item item)
    {
        for (int i = 0; i < item.ItemAttributes.Count; i++)
        {
            if (item.ItemAttributes[i].AttributeName == "Health")
            {
                //MaxHealth -= (byte)item.ItemAttributes[i].AttributeValue;
            }
            if (item.ItemAttributes[i].AttributeName == "Armor")
            {
                CurrentArmor -= (byte)item.ItemAttributes[i].AttributeValue;
            }
        }
        CurrentArmor -= (byte)item.Armor;
    }

    public void EquipWeapon(Item item)
    {
        for (int i = 0; i < item.ItemAttributes.Count; i++)
        {
            if (item.ItemAttributes[i].AttributeName == "Damage")
            {
                CurrentDamage += (byte)item.ItemAttributes[i].AttributeValue;
            }
        }
        CurrentDamage += (byte)item.Damage;
    }

    public void UnequipWeapon(Item item)
    {
        for (int i = 0; i < item.ItemAttributes.Count; i++)
        {
            if (item.ItemAttributes[i].AttributeName == "Damage")
            {
                MaxDamage -= (byte)item.ItemAttributes[i].AttributeValue;
            }
        }
        CurrentDamage -= (byte)item.Damage;
    }
}
