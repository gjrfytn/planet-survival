using UnityEngine;
using System.Collections;

public sealed class Player : Creature
{
    //[Header("Текст на панели характеристик")]
    //public Text CurrentHealthText;
    //public Text CurrentEnergyText;
    //public Text CurrentStaminaText;
    //public Text CurrentHungreedText;

    // [Header("Факторы влияющие на характеристики")]
    // public float HungerTime = 20;  // Переменная времени голода (через сколько секунд отнимаем еду)
    //  public float HungerTimeSet = 20; // Переменная времени голода (для повтора отнимания еды); надо переделать

    [Header("Основные характеристики")]
    [Range(0, 255)]
    public byte MaxWater = 100;
    [Range(0, 255)]
    public byte MaxFood = 100;
    [Range(0, 255)]
    public byte MaxEnergy = 100; // Максимальное количество энергии
    [Range(0, 255)]
    public byte MaxMental = 100;
    [Range(0, 255)]
    public byte MentalRegen = 100;
    //  public float MaxHunger = 500; // Максимальное количество еды

    [Range(0, 255)]
    public byte Water = 100;
    [Range(0, 255)]
    public byte Food = 100;
    [Range(0, 255)]
    public byte Energy = 100; // Энергия (в данный момент)
    [Range(0, 255)]
    public byte Mental = 100;
    //public float CurrentHunger = 500; // Голод (в данный момент)

    public bool CanDualWield;

    [Header("Дальность обзора")]
    [Range(0, 255)]
    public byte ViewDistance;

    float DefaultMoveAnimTime;
    byte Level;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.LocalMapLeft -= Destroy;
    }

    protected override void Start()
    {
        base.Start();
        DefaultMoveAnimTime = MoveAnimTime;
        GetComponent<SpriteRenderer>().sortingLayerName = "Player";
    }

    protected override void Update()
    {
        base.Update();

        if (Moving)//TODO Возможно, временно
            EventManager.OnPlayerObjectMoved();

        if (Health == 0)
        {
            //Health=0;
            // Application.LoadLevel("Menu");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
        /*if (HungerTime > 0)
        {  // Если HungerTime больше нуля
            HungerTime -= Time.deltaTime; //Отсчитываем в минус
        }
        if (HungerTime < 0)
        { //Если HungerTime меньше нуля
            CurrentHunger -= 1; // Отбавляем еду
            HungerTime = HungerTimeSet; //Повторяем всё выше написанное
        }
        if (CurrentHunger == -1)
        { //Если еда равна -1
            CurrentHunger = 0; // Задаём значение 0
        }
        if (CurrentHunger <= 0)
        { //Если еда меньше или равна 0
            CurrentEnergy -= 0.0025f; // Отнимаем здоровье
        }*/



        //Наверное надо куда-нибудь переместить :)
        //	CurrentHealthText.text = "Health: " + Health + " / " + MaxHealth;
        //CurrentEnergyText.text = "Energy: " + CurrentEnergy + " / " + MaxEnergy;
        //CurrentStaminaText.text = "Stamina: " + CurrentStamina + " / " + MaxStamina;
        //CurrentHungreedText.text = "Hungreed: " + CurrentHungreed + " / " + MaxHungreed;
    }

    public override void MoveToMapCoords(Vector2 mapCoords)
    {
        MoveAnimTime = DefaultMoveAnimTime;
        base.MoveToMapCoords(mapCoords);
        EventManager.OnTurn();
        EventManager.OnPlayerMove(mapCoords); //TODO Временно
    }

    public void MoveToMapCoords(Vector2 mapCoords, float moveAnimTime)
    {
        MoveAnimTime = moveAnimTime;
        base.MoveToMapCoords(mapCoords);
        EventManager.OnTurn();
        //EventManager.OnPlayerMove(mapCoords); //TODO Временно //Временно закоммент. см. HexInteraction 21
    }
}
