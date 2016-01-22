using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public delegate void VoidDelegate();

    public event VoidDelegate UIShowed = delegate { };
    public event VoidDelegate UIHided = delegate { };

    public void OnUIShow()
    {
        UIShowed();
    }

    public void OnUIHide()
    {
        UIHided();
    }

    public event VoidDelegate TurnMade = delegate { };

    public void OnTurn()
    {
        TurnMade();
    }

    public event VoidDelegate LocalMapLeft = delegate { };

    public void OnLocalMapLeave()
    {
        LocalMapLeft();
    }

    public delegate void TwoVectorDelegate(Vector2 from, Vector2 to);

    public event TwoVectorDelegate CreatureMoved = delegate { }; //TODO Возможно, это событие будет ненужно потом.

    public void OnCreatureMove(Vector2 from, Vector2 to)
    {
        CreatureMoved(from, to);
    }
}
