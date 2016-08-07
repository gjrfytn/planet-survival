using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Clocks : MonoBehaviour {

    public Text DaysText;
    public Text TimeText;
    void OnEnable()
    {
        EventManager.DayPassed += DaysAlive;
        EventManager.MinutePassed += ChangeTimeOfDay;
    }

    void OnDisable()
    {
        EventManager.DayPassed -= DaysAlive;
        EventManager.MinutePassed -= ChangeTimeOfDay;
    }

    void DaysAlive()
    {
        DaysText.text = GameTime.TimeInDays.ToString();
    }

    void ChangeTimeOfDay(float unused)
    {
        TimeText.text = GameTime.HoursOfDay + " : " + GameTime.MinutesOfHour;
    }

}
