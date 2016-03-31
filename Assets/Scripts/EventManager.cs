using UnityEngine;
using System.Collections;

public static class EventManager
{
    public delegate void VoidDelegate();
	public delegate void FloatDelegate(float f);
    public delegate void VectorDelegate(Vector2 v);
	public delegate void Uint16Delegate(ushort s);
    public delegate void TwoVectorDelegate(Vector2 v1, Vector2 v2);
	public delegate void GameObjectAndByteDelegate(GameObject obj, byte b);

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
    public static event VoidDelegate TurnMade = delegate { };

    public static void OnPlayerMove(Vector2 mapCoords)
    {
        PlayerMoved(mapCoords);
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

    public static event GameObjectAndByteDelegate CreatureHit = delegate { };

	public static void OnCreatureHit(GameObject obj, byte damage)
    {
		CreatureHit(obj,damage);
    }

	public static event VectorDelegate AttackMissed = delegate { };

	public static void OnAttackMiss(Vector2 v)
	{
		AttackMissed(v);
	}

	public static event VoidDelegate HourPassed = delegate { };
	
	public static void OnHourPass()
	{
		HourPassed();
	}

	public static event Uint16Delegate ActionStarted = delegate { };
	public static event FloatDelegate ActionProgressed = delegate { };
	public static event VoidDelegate ActionEnded = delegate { };
	
	public static void OnActionStart(ushort durationInMinutes)
	{
		ActionStarted(durationInMinutes);
	}

	public static void OnActionProgress(float progress)
	{
		ActionProgressed(progress);
	}

	public static void OnActionEnd()
	{
		ActionEnded();
	}
}
