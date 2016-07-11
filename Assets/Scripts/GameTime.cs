using UnityEngine;

public class GameTime : MonoBehaviour
{
    public const float GameMinToRealSec = 0.033333f;
    //TODO Ждём C# 6.0, когда введут инициализацию авто-свойств
    public static uint TimeInMinutes { get; private set; } //=0;

    public static uint TimeInDays { get { return TimeInMinutes / 1440; } }

    public static uint HoursOfDay { get { return TimeInMinutes / 60 - TimeInDays * 24; } }

    public static uint MinutesOfHour { get { return TimeInMinutes - (TimeInDays * 24 + HoursOfDay) * 60; } }

    byte minBuffer = 0;
    byte hourBuffer = 0;

    void OnEnable()
    {
        EventManager.ActionStarted += Run;
    }

    void OnDisable()
    {
        EventManager.ActionStarted -= Run;
    }

    void Run(TimedAction action)
    {
        StartCoroutine(RunTimeCoroutine(action.Duration));
    }

    System.Collections.IEnumerator RunTimeCoroutine(ushort count)
    {
        ushort t = 0;
        while (t != count)
        {
            EventManager.OnMinutePass((float)t / count);
            t++;
            minBuffer++;
            if (minBuffer == 60)
            {
                EventManager.OnHourPass();
                minBuffer = 0;
                hourBuffer++;
                if (hourBuffer == 24)
                {
                    EventManager.OnDayPass();
                    hourBuffer = 0;
                }
            }
            TimeInMinutes++;
            yield return new WaitForSeconds(GameMinToRealSec);//TODO Временно
        }
        EventManager.OnActionEnd();
    }
}
