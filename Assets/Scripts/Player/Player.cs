using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class Player : LivingBeing
{
    public GlobalPos GlobalPos;

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

    public bool CanDualWield;

    [Header("Дальность обзора")]
    [Range(0, 255)]
    public byte ViewDistance;

    byte Level;

    bool Moving;
    float MoveTime;
    Stack<LocalPos> Path = new Stack<LocalPos>();

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.HourPassed += UpdateState;
        EventManager.LocalMapLeft -= Destroy; //TODO Временно?
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.HourPassed -= UpdateState;
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
        if (Moving)
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
            else if (Path.Count == 0)
                Moving = false;
            else
            {
                LocalPos node = Path.Pop();
                LocalPos vBuf = (LocalPos)Pos;
                Pos = node;
                EventManager.OnCreatureMove(vBuf, (LocalPos)Pos); //TODO name?

                MoveTime = MoveAnimTime;
            }
        }
    }

    public void MoveTo(LocalPos pos, bool inBattle)
    {
        if (inBattle)
        {
            MoveTime = MoveAnimTime;
            GlobalPos = Pos = pos;
            Moving = true;
            EventManager.OnTurn();
        }
        else
        {
            List<LocalPos> buf = Pathfinder.MakePath((GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World.CurrentMap as LocalMap).GetBlockMatrix(), (LocalPos)Pos, pos);//TODO Тут?
            buf.Reverse();
            Path = new Stack<LocalPos>(buf);
            Path.Pop();
            MoveTime = MoveAnimTime;
            Pos = Path.Pop();
            Moving = true;
        }

        EventManager.OnPlayerMove(pos); //TODO Временно
    }

    public void MoveTo(GlobalPos pos, float moveAnimTime)
    {
        MoveTime = moveAnimTime;
        GlobalPos = pos;
        Moving = true;
        //EventManager.OnPlayerMove(mapCoords); //TODO Временно //Временно закоммент. см. HexInteraction 21
    }

    public void Attack(LivingBeing target/*,weapon*/)//C#6.0 EBD
    {
        if (Random.value < BaseAccuracy)
            target.TakeDamage((byte)(BaseDamage + Random.Range(-BaseDamage * DamageSpread, BaseDamage * DamageSpread)), true);
        else
            EventManager.OnAttackMiss((LocalPos)Pos);
    }

    void UpdateState()
    {
        List<Terrains.TerrainProperties> terrains = GameObject.FindWithTag("World").GetComponent<Terrains>().GetTerrainProperties(GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World.GetHexTerrain(Pos));
        float mentalPercent = Mental / MaxMental;
        MentalPenalty curPenalty = mentalPercent < InsaneMentalPenalty.TopPercent ? InsaneMentalPenalty : (mentalPercent < HighMentalPenalty.TopPercent ? HighMentalPenalty : (mentalPercent < MediumMentalPenalty.TopPercent ? MediumMentalPenalty : (mentalPercent < LowMentalPenalty.TopPercent ? LowMentalPenalty : new MentalPenalty() { Water = 0, Food = 0, Energy = 0 })));
        Water = Water - WaterConsumption - terrains.Sum(t => t.WaterConsumption) - curPenalty.Water;
        Food = Food - FoodConsumption - terrains.Sum(t => t.FoodConsumption) - curPenalty.Food;
        Energy = Energy - EnergyConsumption - terrains.Sum(t => t.EnergyConsumption) - curPenalty.Energy;
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
}
