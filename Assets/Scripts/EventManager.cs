﻿using UnityEngine;
using System.Collections;

public static class EventManager
{
    public delegate void VoidDelegate();

    public static event VoidDelegate UIShowed = delegate { };
    public static event VoidDelegate UIHided = delegate { };

    public static void OnUIShow()
    {
        UIShowed();
    }

    public static void OnUIHide()
    {
        UIHided();
    }

    public static event VoidDelegate TurnMade = delegate { };

    public static void OnTurn()
    {
        TurnMade();
    }

    public static event VoidDelegate LocalMapLeft = delegate { };

    public static void OnLocalMapLeave()
    {
        LocalMapLeft();
    }

    public delegate void TwoVectorDelegate(Vector2 from, Vector2 to);

    public static event TwoVectorDelegate CreatureMoved = delegate { }; //TODO Возможно, это событие будет ненужно потом.

    public static void OnCreatureMove(Vector2 from, Vector2 to)
    {
        CreatureMoved(from, to);
    }

    public static event VoidDelegate PlayerObjectMoved = delegate { };

    public static void OnPlayerObjectMoved()
    {
        PlayerObjectMoved();
    }
}