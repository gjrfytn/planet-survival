using UnityEngine;
using System.Collections;

public sealed class Player : Creature
{
    //[Header("Текст на панели характеристик")]
    //public Text CurrentHealthText;
    //public Text CurrentEnergyText;
    //public Text CurrentStaminaText;
    //public Text CurrentHungreedText;

    [Header("Факторы влияющие на характеристики")]
    public float HungerTime = 20;  // Переменная времени голода (через сколько секунд отнимаем еду)
    public float HungerTimeSet = 20; // Переменная времени голода (для повтора отнимания еды); надо переделать


    [Header("Основные характеристики")]
    public float MaxEnergy = 100; // Максимальное количество энергии
    public float MaxStamina = 1000; // Максимальное количество силы
    public float MaxHungreed = 500; // Максимальное количество еды

    public float CurrentEnergy = 100; // Энергия (в данный момент)
    public float CurrentStamina = 1000; // Выносливость (сколько в данный момент)
    public float CurrentHunger = 500; // Голод (в данный момент)

    [Header("Дальность обзора")]
    public byte ViewDistance;

    new void OnEnable()
    {
        base.OnEnable();
        EventManager.LocalMapLeft -= Destroy;
    }

    new void OnDisable()
    {
        base.OnDisable();
    }

    new void Start()
    {
        base.Start();
        GetComponent<SpriteRenderer>().sortingLayerName = "Player";
    }

    new void Update()
    {
        base.Update();

        if (Moving)//TODO Возможно, временно
            EventManager.OnPlayerObjectMoved();

        if (Health <= 0)
        {
            //Health=0;
            Application.LoadLevel("Menu");
        }
        if (CurrentEnergy <= 0)
        { //Если жизни меньше или равно 0
            CurrentEnergy = 0; // Задаём значение 0
        }
        if (HungerTime > 0)
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
        }
        if (CurrentStamina != 1000)
        { // Если выносливость не равна 1000
            CurrentStamina += 0.0025f; // Прибавляем выносливость (Отдых)
        }
        if (Input.GetKey(KeyCode.LeftShift))
        { // Если нажать кнопку LeftShift (Для теста)
            CurrentStamina -= 1; // Отнимаем выносливлость
        }
        if (CurrentStamina == 1000)
        { //Если выносливость равна 1000
            CurrentStamina = MaxStamina; // Задаём число значения MaxStamina
        }
        if (CurrentStamina <= 0)
        { // Если выносливость меньше или равна 0
            CurrentStamina = 0; // Задаём число 0
        }



        //Наверное надо куда-нибудь переместить :)
        //	CurrentHealthText.text = "Health: " + Health + " / " + MaxHealth;
        //CurrentEnergyText.text = "Energy: " + CurrentEnergy + " / " + MaxEnergy;
        //CurrentStaminaText.text = "Stamina: " + CurrentStamina + " / " + MaxStamina;
        //CurrentHungreedText.text = "Hungreed: " + CurrentHungreed + " / " + MaxHungreed;
    }

    public new void MoveToMapCoords(Vector2 mapCoords)
    {
        base.MoveToMapCoords(mapCoords);
        EventManager.OnTurn();
        EventManager.OnPlayerMove(mapCoords); //TODO Временно
    }
}
