using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class Player : LivingBeing
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
    [Range(0, 255)]
    public byte MaxWater = 100;
    [Range(0, 255)]
    public byte MaxFood = 100;
    [Range(0, 255)]
    public byte MaxEnergy = 100;
    [Range(0, 255)]
    public byte MaxMental = 100;
    [Range(0, 255)]
    public byte MentalRegen = 100;

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

    float Energy_;
    public float Energy
    {
        get
        {
            return Energy_;
        }
        private set
        {
            Energy_ = value;
            if (Energy_ <= 0)
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

    public float WaterConsumption;
    public float FoodConsumption;
    public float EnergyConsumption;

    [System.Serializable]
    public class MentalPenalty
    {
        public float TopPercent;
        public float Water;
        public float Food;
        public float Energy;
    }

    public MentalPenalty LowMentalPenalty;
    public MentalPenalty MediumMentalPenalty;
    public MentalPenalty HighMentalPenalty;
    public MentalPenalty InsaneMentalPenalty;

    TimedAction CurrentAction;

    public bool CanDualWield;

    [Header("Дальность обзора")]
    [Range(0, 255)]
    public byte ViewDistance;

    byte Level;

    float MoveTime;
    Stack<LocalPos> Path = new Stack<LocalPos>();//TODO В LivingBeing?

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.ActionStarted += StartAction;
        EventManager.HourPassed += UpdateState;
        EventManager.ActionEnded += EndAction;

        EventManager.LocalMapLeft -= Destroy; //TODO Временно?
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.ActionStarted -= StartAction;
        EventManager.HourPassed -= UpdateState;
        EventManager.ActionEnded -= EndAction;
    }

    protected override void Start()
    {
        base.Start();
        GetComponent<SpriteRenderer>().sortingLayerName = "Player";

        //TODO C# 6.0 инициализаторы свойств:
        Water = MaxWater;
        Food = MaxFood;
        Energy = MaxEnergy;
        Mental = MaxMental;
    }

    void Update()
    {
        if (MoveTime > 0)
        {
            float tstep = MoveTime / Time.deltaTime;
            MoveTime -= Time.deltaTime;
            //TODO Возможно стоит сохранять значение из GetTransformPosFromMapPos(MapCoords,World.IsCurrentMapLocal())), так как это улучшит(?) производительность
            if (GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World.IsCurrentMapLocal())
            {
                float dstep = Vector2.Distance(transform.position, WorldVisualiser.GetTransformPosFromMapPos(Pos)) / tstep;
                transform.position = Vector2.MoveTowards(transform.position, WorldVisualiser.GetTransformPosFromMapPos(Pos), dstep);
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
            LocalPos buf = Pos;
            Pos = Path.Pop();
            EventManager.OnCreatureMove(buf, Pos); //TODO name?

            MoveTime = MoveAnimTime;
        }
    }

    public void MoveTo(LocalPos pos, bool inBattle)
    {
        LocalPos pBuf = Pos;
        if (inBattle)
        {
            MoveTime = MoveAnimTime;
            GlobalPos = Pos = pos;
        }
        else
        {
            List<LocalPos> buf = Pathfinder.MakePath((GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World.CurrentMap as LocalMap).GetBlockMatrix(), Pos, pos);//TODO Тут?
            buf.Reverse();
            Path = new Stack<LocalPos>(buf);
            Path.Pop();
        }

        //TODO Тут всё очень плохо, надо пересматривать:
        EventManager.OnCreatureMove(pBuf, pos);
        EventManager.OnTurn();
        EventManager.OnPlayerMove(pos);
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
        List<Terrains.TerrainProperties> terrains = GameObject.FindWithTag("World").GetComponent<Terrains>().GetTerrainProperties(GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World.GetHexTerrain(Pos));
        float mentalPercent = Mental / MaxMental;
        MentalPenalty curPenalty = mentalPercent < InsaneMentalPenalty.TopPercent ? InsaneMentalPenalty : (mentalPercent < HighMentalPenalty.TopPercent ? HighMentalPenalty : (mentalPercent < MediumMentalPenalty.TopPercent ? MediumMentalPenalty : (mentalPercent < LowMentalPenalty.TopPercent ? LowMentalPenalty : new MentalPenalty() { Water = 0, Food = 0, Energy = 0 })));
        Water = Water - WaterConsumption - terrains.Sum(t => t.Travel.WaterConsumption) - curPenalty.Water - (CurrentAction != null ? CurrentAction.WaterConsumption : 0);//C#6.0
        Food = Food - FoodConsumption - terrains.Sum(t => t.Travel.FoodConsumption) - curPenalty.Food - (CurrentAction != null ? CurrentAction.FoodConsumption : 0);//C#6.0
        Energy = Energy - EnergyConsumption - terrains.Sum(t => t.Travel.EnergyConsumption) - curPenalty.Energy - (CurrentAction != null ? CurrentAction.EnergyConsumption : 0);//C#6.0
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
        Energy = Mathf.Clamp(Energy + value, 0, MaxEnergy);
    }

    public TempWeapon GetWeapon()
    {
        return BaseWeapon;
    }
}
