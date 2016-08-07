using UnityEngine;
using System.Collections.Generic;

using LocalPos = U16Vec2;
using GlobalPos = S32Vec2;

public class Player : LivingBeing
{
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
    public byte MaxWater { get { return MaxWater_; } }
    [SerializeField, Range(0, 255)]
    byte MaxFood_;
    public byte MaxFood { get { return MaxFood_; } }
    [SerializeField, Range(0, 255)]
    byte MaxStamina_;
    public byte MaxStamina { get { return MaxStamina_; } }
    [SerializeField, Range(0, 255)]
    byte MaxMental_;
    public byte MaxMental { get { return MaxMental_; } }
    [SerializeField, Range(0, 255)]
    byte MentalRegen_;
    public byte MentalRegen { get { return MentalRegen_; } }

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
        public float TopPercent { get { return TopPercent_; } }
        [SerializeField]
        float Water_;
        public float Water { get { return Water_; } }
        [SerializeField]
        float Food_;
        public float Food { get { return Food_; } }
        [SerializeField]
        float Stamina_;
        public float Stamina { get { return Stamina_; } }

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
    public bool CanDualWield { get { return CanDualWield_; } }

    byte Level;

    float MoveTime;
    Stack<LocalPos> Path = new Stack<LocalPos>();//TODO В LivingBeing?
    public byte RemainingMoves { get; private set; }
    LocalPos NextMovePoint;

    EquipmentSlot WeaponSlot;
    public Item Weapon
    {
        get
        {
            return WeaponSlot.GetComponentInChildren<AttachedItem>() != null ? WeaponSlot.GetComponentInChildren<AttachedItem>().Item : BaseWeapon;
        }
    }
    EquipmentSlot BodyArmorSlot;

    [SerializeField]
    Sprite NormalSprite;
    [SerializeField]
    Sprite FightingSprite;
    [SerializeField]
    DefenceAction[] DefenceActions;

    bool DefendedInTurn;

    void OnEnable()
    {
        EventManager.HourPassed += UpdateState;
        EventManager.ActionEnded += EndAction;
        EventManager.LocalMapLeft += StopMakingTurn;
        EventManager.ActionChosen += PerformAction;

        //Inventory
        InventoryEvents.OnConsume += ConsumeItem;

        CraftingEvents.OnItemCraft += PerformCraftAction;
    }

    void OnDisable()
    {
        EventManager.HourPassed -= UpdateState;
        EventManager.ActionEnded -= EndAction;
        EventManager.LocalMapLeft -= StopMakingTurn;
        EventManager.ActionChosen -= PerformAction;

        //Inventory
        InventoryEvents.OnConsume -= ConsumeItem;

        CraftingEvents.OnItemCraft -= PerformCraftAction;
    }

    protected override void Start()
    {
        base.Start();
        transform.position = WorldVisualizer.GetTransformPosFromMapPos(GlobalPos);
        GetComponent<SpriteRenderer>().sortingLayerName = "Player";

        //TODO C# 6.0 инициализаторы свойств:
        Water = MaxWater;
        Food = MaxFood;
        Stamina = MaxStamina;
        Mental = MaxMental;

        Equipment equipment = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>().Equipment;
        foreach (GameObject slot in equipment.Slots)
            if (slot.GetComponent<EquipmentSlot>().EquipmentType == ItemType.Weapon)
                WeaponSlot = slot.GetComponent<EquipmentSlot>();
            else if (slot.GetComponent<EquipmentSlot>().EquipmentType == ItemType.Chest)
                BodyArmorSlot = slot.GetComponent<EquipmentSlot>();
    }

    bool BluesRendered = false; //TODO Временно

    void Update()
    {
        if (MakingTurn)
        {
            if (MoveTime > 0)
            {
                float tstep = MoveTime / Time.deltaTime;
                MoveTime -= Time.deltaTime;
                //TODO Возможно стоит сохранять значение из GetTransformPosFromMapPos(MapCoords,World.IsCurrentMapLocal())), так как это улучшит(?) производительность
                if (GameObject.FindWithTag("World/World").GetComponent<World>().IsCurrentMapLocal())
                {
                    float dstep = Vector2.Distance(transform.position, WorldVisualizer.GetTransformPosFromMapPos(NextMovePoint)) / tstep;
                    transform.position = Vector2.MoveTowards(transform.position, WorldVisualizer.GetTransformPosFromMapPos(NextMovePoint), dstep);
                }
                else
                {
                    float dstep = Vector2.Distance(transform.position, WorldVisualizer.GetTransformPosFromMapPos(GlobalPos)) / tstep;
                    transform.position = Vector2.MoveTowards(transform.position, WorldVisualizer.GetTransformPosFromMapPos(GlobalPos), dstep);
                }

                EventManager.OnPlayerObjectMove();
            }
            else if (Path.Count != 0)
            {
                NextMovePoint = Path.Pop();
                MoveTime = MoveAnimTime;
            }
            else if (RemainingMoves == 0)
            {
                MakingTurn = false;
                EventManager.OnLivingBeingEndTurn();
            }
            else if (!BluesRendered)
            {
                GameObject.FindWithTag("World/World").GetComponent<World>().RerenderBlueHexesOnLocal();
                BluesRendered = true;
            }
        }
    }

    public override void TakeDamage(byte damage, bool applyArmor, bool applyDefenceAction)
    {
        //Debug.Assert(damage >= 0);

        DamageTemp = damage;
        ApplyArmorTemp = applyArmor;

        GetComponent<SpriteRenderer>().sprite = FightingSprite;
        if (applyDefenceAction && !DefendedInTurn)
        {
            EventManager.OnPopupButtonsCall(transform.position, DefenceActions, true);
            DefendedInTurn = true;
        }
        else
            ApplyDamage(damage, applyArmor);
    }

    void ApplyDamage(byte damage, bool applyArmor)
    {
        if (applyArmor && BodyArmorSlot.GetComponentInChildren<AttachedItem>() != null)
            DamageTemp = (byte)Mathf.RoundToInt(DamageTemp * (1 - BodyArmorSlot.GetComponentInChildren<AttachedItem>().Item.Armor));
        Health = (byte)(Health - DamageTemp > 0 ? Health - DamageTemp : 0);//TODO Если Health не float

        EventManager.OnCreatureHit(this, DamageTemp);
        EventManager.OnPlayerDefence();
    }

    public void MoveTo(LocalPos pos)
    {
        List<LocalPos> buf = Pathfinder.MakePath((GameObject.FindWithTag("World/World").GetComponent<World>().CurrentMap as LocalMap).GetBlockMatrix(), Pos, pos, false);//TODO Тут?
        buf.Reverse();
        Path = new Stack<LocalPos>(buf);
        Path.Pop();

        LocalPos pBuf = Pos;
        Pos = pos;

        EventManager.OnCreatureMove(pBuf, pos);
        RemainingMoves -= (byte)Path.Count;
        EventManager.OnBluesUnrender();
    }

    public void MoveTo(GlobalPos pos, float moveAnimTime)
    {
        MakingTurn = true;
        MoveTime = moveAnimTime;
        GlobalPos = pos;
        //EventManager.OnPlayerMove(mapCoords); //TODO Временно //Временно закоммент. см. HexInteraction 21
    }

    void Attack(LivingBeing target, float damage, float accuracy)//C#6.0 EBD
    {
        if (Random.value < accuracy)
            target.TakeDamage((byte)damage, true, true);
        else
            EventManager.OnAttackMiss(transform.position);
        RemainingMoves = 0;
        MakingTurn = false;
        EventManager.OnBluesUnrender();
        EventManager.OnLivingBeingEndTurn();

        GetComponent<SpriteRenderer>().sprite = FightingSprite;
    }

    void UpdateState()
    {
        TimedAction terrainCons = GameObject.FindWithTag("World/World").GetComponent<Terrains>().GetTerrainProperties(GameObject.FindWithTag("World/World").GetComponent<World>().GetHexTerrain(GlobalPos));
        float mentalPercent = Mental / MaxMental;
        MentalPenalty curPenalty = mentalPercent < InsaneMentalPenalty.TopPercent ? InsaneMentalPenalty : (mentalPercent < HighMentalPenalty.TopPercent ? HighMentalPenalty : (mentalPercent < MediumMentalPenalty.TopPercent ? MediumMentalPenalty : (mentalPercent < LowMentalPenalty.TopPercent ? LowMentalPenalty : new MentalPenalty(0, 0, 0))));
        Water = Water - WaterConsumption - terrainCons.WaterConsumption - curPenalty.Water - (CurrentAction != null ? CurrentAction.WaterConsumption : 0);//C#6.0
        Food = Food - FoodConsumption - terrainCons.FoodConsumption - curPenalty.Food - (CurrentAction != null ? CurrentAction.FoodConsumption : 0);//C#6.0
        Stamina = Stamina - StaminaConsumption - terrainCons.StaminaConsumption - curPenalty.Stamina - (CurrentAction != null ? CurrentAction.StaminaConsumption : 0);//C#6.0
    }

    void EndAction()
    {
        if (CurrentAction.ProducedItems != null)
        {
            Inventory inventory = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>().Inventory;
            foreach (TimedAction.ProducedItem item in CurrentAction.ProducedItems)
                inventory.AddItem(item.ItemID, item.Quantity);
        }
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

    public override void MakeTurn()
    {
        MakingTurn = true;
        RemainingMoves = Speed;
        DefendedInTurn = false;

        BluesRendered = false;
    }

    void StopMakingTurn()
    {
        MakingTurn = false;
        RemainingMoves = 0;
    }

    byte DamageTemp;
    bool ApplyArmorTemp;

    void PerformAction(Action action)
    {
        if (action is TimedAction)
        {
            TimedAction ta = action as TimedAction;
            CurrentAction = ta;
            EventManager.OnActionStart(ta);
        }
        else if (action is DefenceAction)
        {
            (action as DefenceAction).TryPerform(ref DamageTemp);
            ApplyDamage(DamageTemp, ApplyArmorTemp);
        }
        else if (action is AttackAction)
        {
            AttackAction aa = action as AttackAction;
            Attack(aa.Target, Weapon.Damage * aa.DamageMultiplier, aa.Chance);
        }
    }

    void PerformCraftAction(Blueprint blueprint)
    {
        TimedAction timedAction = new TimedAction();
        timedAction.Duration = blueprint.CraftTime;
        timedAction.StaminaConsumption = 0.5f;
        timedAction.Sprite = blueprint.Item.Icon;
        CurrentAction = timedAction;
        EventManager.OnActionStart(timedAction);
    }


    //==============================
    public void ConsumeItem(AttachedItem attachedItem)
    {
        for (int i = 0; i < attachedItem.Item.ItemAttributes.Count; i++)
        {
            if (attachedItem.Item.ItemAttributes[i].AttributeName == "Health")
                TakeHeal((byte)attachedItem.Item.ItemAttributes[i].AttributeValue);

            /*if (item.ItemAttributes[i].AttributeName == "Armor")
            {
                CurrentArmor += (byte)item.ItemAttributes[i].AttributeValue;
            }
            if (item.ItemAttributes[i].AttributeName == "Damage")
            {
                if ((CurrentDamage + item.ItemAttributes[i].AttributeValue) > MaxDamage)
                    CurrentDamage = MaxDamage;
                else
                    CurrentDamage += (byte)item.ItemAttributes[i].AttributeValue;
            }*/
        }
    }
}