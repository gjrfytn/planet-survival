using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    [SerializeField]
    Text DaysText;
    [SerializeField]
    Text TimeText;
    [SerializeField]
    float BlinkInterval;

    void OnEnable()
    {
        EventManager.DayPassed += ChangeDaysAlive;
        EventManager.MinutePassed += ChangeTimeOfDay;
    }

    void OnDisable()
    {
        EventManager.DayPassed -= ChangeDaysAlive;
        EventManager.MinutePassed -= ChangeTimeOfDay;
    }

    void Start()
    {
        StartCoroutine(BlinkColon());
    }

    void ChangeDaysAlive()
    {
        DaysText.text = GameTime.TimeInDays.ToString();
    }

    void ChangeTimeOfDay(float unused)
    {
        TimeText.text = string.Format("{0:00}{1}{2:00}", GameTime.HoursOfDay, TimeText.text[2], GameTime.MinutesOfHour);
    }

    System.Collections.IEnumerator BlinkColon()
    {
        while (true)
        {
            if (TimeText.text[2] == ':')
                TimeText.text = TimeText.text.Replace(':', ' ');
            else
                TimeText.text = TimeText.text.Replace(' ', ':');
            yield return new WaitForSeconds(BlinkInterval);
        }
    }
}
