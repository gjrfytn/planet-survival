using UnityEngine;

public class GameTime : MonoBehaviour
{
    public const float GameMinToRealSec = 0.033333f;
    //TODO Ждём C# 6.0, когда введут инициализацию авто-свойств
    public static uint TimeInMinutes { get; private set; } //=0;

	public static uint TimeInDays//C#6.0 EBD
    {
        get
        {
            return TimeInMinutes / 1440;
        }
        private set
        {

        }
    }

	public static uint HoursOfDay//C#6.0 EBD
    {
        get
        {
            return TimeInMinutes / 60 - TimeInDays * 24;
        }
        private set
        {

        }
    }

	public static uint MinutesOfHour//C#6.0 EBD
    {
        get
        {
            return TimeInMinutes - (TimeInDays * 24 + HoursOfDay) * 60;
        }
        private set
        {

        }
    }

    byte minBuffer = 0;
	byte hourBuffer = 0;

    void OnEnable()
    {
        EventManager.ActionStarted += RunForMinutes;
    }

    void OnDisable()
    {
        EventManager.ActionStarted -= RunForMinutes;
    }

    void RunForMinutes(ushort count)
    {
        StartCoroutine(RunTimeCoroutine(count));
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
				if(hourBuffer==24)
				{
					EventManager.OnDayPass();
					hourBuffer=0;
				}
            }
            TimeInMinutes++;
            yield return new WaitForSeconds(GameMinToRealSec);//TODO Временно
        }
        EventManager.OnActionEnd();
    }
}
