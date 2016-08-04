using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Clocks : MonoBehaviour {

    public Text DaysText;
    void OnEnable()
    {
        EventManager.ActionStarted += DaysAlive;
    }

    public void DaysAlive(TimedAction action)
    {
        DaysText.text = GameTime.TimeInDays.ToString();
    }

}
