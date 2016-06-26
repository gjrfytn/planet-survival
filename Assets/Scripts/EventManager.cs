using UnityEngine;

public static class EventManager
{
    public delegate void VoidDelegate();
    public delegate void FloatDelegate(float f);
    public delegate void Vector2Delegate(Vector2 p);
    public delegate void GlobalPosDelegate(GlobalPos p);
    public delegate void TimedActionDelegate(TimedAction a);
    public delegate void Vector2AndActionArrayDelegate(Vector2 v, Action[] a);
    public delegate void TwoLocalPosDelegate(LocalPos p1, LocalPos p2);
    public delegate void LivBeAndByteDelegate(LivingBeing lb, byte b);
    public delegate void EntityDelegate(Entity e);
    public delegate void PopupButtonDelegate(PopupButton pb);

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

    public static event GlobalPosDelegate PlayerMovedOnGlobal = delegate { };
    public static event VoidDelegate LivingBeingEndedTurn = delegate { };

    public static void OnPlayerMoveOnGlobal(GlobalPos pos)//C#6.0 EBD
    {
        PlayerMovedOnGlobal(pos);
    }

    public static void OnLivingBeingEndTurn()//C#6.0 EBD
    {
        LivingBeingEndedTurn();
    }

    public static event TwoLocalPosDelegate CreatureMoved = delegate { }; //TODO Возможно, это событие будет ненужно потом.

    public static void OnCreatureMove(LocalPos from, LocalPos to)//C#6.0 EBD
    {
        CreatureMoved(from, to);
    }

    public static event VoidDelegate PlayerObjectMoved = delegate { };

    public static void OnPlayerObjectMove()//C#6.0 EBD
    {
        PlayerObjectMoved();
    }

    public static event LivBeAndByteDelegate CreatureHit = delegate { };
    public static event LivBeAndByteDelegate CreatureHealed = delegate { };

    public static void OnCreatureHit(LivingBeing lb, byte damage)//C#6.0 EBD
    {
        CreatureHit(lb, damage);
    }

    public static void OnCreatureHealed(LivingBeing lb, byte heal)//C#6.0 EBD
    {
        CreatureHealed(lb, heal);
    }

    public static event Vector2Delegate AttackMissed = delegate { };

    public static void OnAttackMiss(Vector2 v)//C#6.0 EBD
    {
        AttackMissed(v);
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

    public static event TimedActionDelegate ActionStarted = delegate { };
    public static event FloatDelegate MinutePassed = delegate { };
    public static event VoidDelegate ActionEnded = delegate { };

    public static void OnActionStart(TimedAction a)//C#6.0 EBD
    {
        ActionStarted(a);
    }

    public static void OnMinutePass(float progress)//C#6.0 EBD
    {
        MinutePassed(progress);
    }

    public static void OnActionEnd()//C#6.0 EBD
    {
        ActionEnded();
    }

    public static event EntityDelegate EntitySpawned = delegate { };
    public static event EntityDelegate EntityDestroyed = delegate { };
    public static event VoidDelegate CreatureDied = delegate { };

    public static void OnEntitySpawn(Entity e)//C#6.0 EBD
    {
        EntitySpawned(e);
    }

    public static void OnEntityDestroy(Entity e)//C#6.0 EBD
    {
        EntityDestroyed(e);
    }

    public static void OnCreatureDeath()//C#6.0 EBD
    {
        CreatureDied();
    }

    public static event Vector2AndActionArrayDelegate PopupButtonsCalled = delegate { };
    public static event PopupButtonDelegate PopupButtonClicked = delegate { };
    public static event VoidDelegate PopupButtonsExpelled = delegate { };

    public static void OnPopupButtonsCall(Vector2 v, Action[] a)//C#6.0 EBD
    {
        PopupButtonsCalled(v, a);
    }

    public static void OnPopupButtonClick(PopupButton pb)//C#6.0 EBD
    {
        PopupButtonClicked(pb);
    }

    public static void OnPopupButtonExpel()//C#6.0 EBD
    {
        PopupButtonsExpelled();
    }

    public static event VoidDelegate LocalMapEntered = delegate { };
    public static event VoidDelegate LocalMapLeft = delegate { };

    public static void OnLocalMapEnter()//C#6.0 EBD
    {
        LocalMapEntered();
    }

    public static void OnLocalMapLeave()//C#6.0 EBD
    {
        LocalMapLeft();
    }

    public static event VoidDelegate BluesRendered = delegate { };
    public static event VoidDelegate BluesUnrendered = delegate { };

    public static void OnBluesRender()//C#6.0 EBD
    {
        BluesRendered();
    }

    public static void OnBluesUnrender()//C#6.0 EBD
    {
        BluesUnrendered();
    }
}
