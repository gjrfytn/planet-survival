using UnityEngine;

public static class EventManager
{
    public delegate void VoidDelegate();
    public delegate void FloatDelegate(float f);
    public delegate void LocalPosDelegate(LocalPos p);
    public delegate void GlobalPosDelegate(GlobalPos p);
    public delegate void Uint16Delegate(ushort s);
    public delegate void TwoLocalPosDelegate(LocalPos p1, LocalPos p2);
    public delegate void LivBeAndByteDelegate(LivingBeing lb, byte b);
    public delegate void EntityDelegate(Entity e);

    public static event VoidDelegate UIShowed = delegate { };
    public static event VoidDelegate UIHided = delegate { };

    public static void OnUIShow()//C#6.0 EBD
    {
        UIShowed();
    }

    public static void OnUIHide()//C#6.0 EBD
    {
        UIHided();
    }

    public static event GlobalPosDelegate PlayerMoved = delegate { };
    public static event VoidDelegate TurnMade = delegate { };

    public static void OnPlayerMove(GlobalPos pos)//C#6.0 EBD
    {
        PlayerMoved(pos);
    }

    public static void OnTurn()//C#6.0 EBD
    {
        TurnMade();
    }

    public static event VoidDelegate LocalMapLeft = delegate { };

    public static void OnLocalMapLeave()//C#6.0 EBD
    {
        LocalMapLeft();
    }

    public static event TwoLocalPosDelegate CreatureMoved = delegate { }; //TODO Возможно, это событие будет ненужно потом.

    public static void OnCreatureMove(LocalPos from, LocalPos to)//C#6.0 EBD
    {
        CreatureMoved(from, to);
    }

    public static event VoidDelegate PlayerObjectMoved = delegate { };

    public static void OnPlayerObjectMoved()//C#6.0 EBD
    {
        PlayerObjectMoved();
    }

    public static event LivBeAndByteDelegate CreatureHit = delegate { };

    public static void OnCreatureHit(LivingBeing lb, byte damage)//C#6.0 EBD
    {
        CreatureHit(lb, damage);
    }

    public static event LocalPosDelegate AttackMissed = delegate { };

    public static void OnAttackMiss(LocalPos p)//C#6.0 EBD
    {
        AttackMissed(p);
    }

    public static event VoidDelegate HourPassed = delegate { };
    public static event VoidDelegate DayPassed = delegate { };

    public static void OnHourPass()//C#6.0 EBD
    {
        HourPassed();
    }

    public static void OnDayPass()//C#6.0 EBD
    {
        DayPassed();
    }

    public static event Uint16Delegate ActionStarted = delegate { };
    public static event FloatDelegate MinutePassed = delegate { };
    public static event VoidDelegate ActionEnded = delegate { };

    public static void OnActionStart(ushort durationInMinutes)//C#6.0 EBD
    {
        ActionStarted(durationInMinutes);
    }

    public static void OnMinutePass(float progress)//C#6.0 EBD
    {
        MinutePassed(progress);
    }

    public static void OnActionEnd()//C#6.0 EBD
    {
        ActionEnded();
    }

    public static event EntityDelegate EntityDestroyed = delegate { };

    public static void OnEntityDestroy(Entity e)
    {
        EntityDestroyed(e);
    }
}
