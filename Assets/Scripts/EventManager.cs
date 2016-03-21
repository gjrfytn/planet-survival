using UnityEngine;
using System.Collections;

public static class EventManager
{
    public delegate void VoidDelegate();
    public delegate void VectorDelegate(Vector2 v);
    public delegate void TwoVectorDelegate(Vector2 v1, Vector2 v2);
	public delegate void GameObjectAndFloatDelegate(GameObject obj, float f);

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

    public static event VectorDelegate PlayerMoved = delegate { };
    public static event VoidDelegate PlayerActed = delegate { };
    public static event VoidDelegate TurnMade = delegate { };

    public static void OnPlayerMove(Vector2 mapCoords)
    {
        PlayerMoved(mapCoords);
    }

    public static void OnPlayerAction()
    {
        PlayerActed();
    }

    public static void OnTurn()
    {
        TurnMade();
    }

    public static event VoidDelegate LocalMapLeft = delegate { };

    public static void OnLocalMapLeave()
    {
        LocalMapLeft();
    }

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

    public static event GameObjectAndFloatDelegate CreatureHit = delegate { };

	public static void OnCreatureHit(GameObject obj, float damage)
    {
		CreatureHit(obj,damage);
    }
}
