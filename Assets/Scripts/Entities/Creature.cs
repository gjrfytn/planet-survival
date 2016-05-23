using UnityEngine;
using System.Collections.Generic;

public class Creature : LivingBeing
{
    public enum AI_State : byte { STATE_IDLE, STATE_MOVE, STATE_ATTACK };

    World World;
    LocalPos TargetPos;
    AI_State State;
    LivingBeing Target;
    Stack<LocalPos> Path;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.TurnMade += MakeTurn;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.TurnMade -= MakeTurn;
    }

    protected override void Start()
    {
        base.Start();
        World = GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World;
    }

    public void MoveTo(LocalPos pos)
    {
        State = AI_State.STATE_MOVE;
        TargetPos = pos;

        List<LocalPos> buf = Pathfinder.MakePath((World.CurrentMap as LocalMap).GetBlockMatrix(), Pos, TargetPos);//TODO Тут?
        buf.Reverse();
        Path = new Stack<LocalPos>(buf);
        Path.Pop();
    }

    public void Attack(LivingBeing target)
    {
        State = AI_State.STATE_ATTACK;
        Target = target;
        TargetPos = Target.Pos;
        World = GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World;//TODO Костыль
        List<LocalPos> buf = Pathfinder.MakePath((World.CurrentMap as LocalMap).GetBlockMatrix(), Pos, TargetPos);//TODO Тут?
        buf.Reverse();
        Path = new Stack<LocalPos>(buf);
        Path.Pop();
    }

    public void Idle()
    {
        Target = null;
        Path = null;
        State = AI_State.STATE_IDLE;
    }

    void Move()
    {
        LocalPos node = Path.Pop();
        if (!World.IsHexFree(node))
        {
            List<LocalPos> buf = Pathfinder.MakePath((World.CurrentMap as LocalMap).GetBlockMatrix(), Pos, TargetPos);//TODO Тут?
            buf.Reverse();
            Path = new Stack<LocalPos>(buf);
            Path.Pop();
            node = Path.Pop();
        }
        LocalPos vBuf = Pos;
        Pos = node;
        EventManager.OnCreatureMove(vBuf, Pos); //TODO name?

        StartCoroutine(MoveHelper.Fly(gameObject, WorldVisualiser.GetTransformPosFromMapPos(Pos), MoveAnimTime));
    }

    void PerformAttack(LivingBeing target)
    {
        if (Random.value < BaseAccuracy)
            target.TakeDamage((byte)(BaseDamage + Random.Range(-BaseDamage * DamageSpread, BaseDamage * DamageSpread)), true);
        else
            EventManager.OnAttackMiss(Pos);
    }

    void MakeTurn()
    {
        switch (State)
        {
            case AI_State.STATE_IDLE:
                break;
            case AI_State.STATE_MOVE:
                if (TargetPos == Pos)
                    Idle();
                else
                    Move();
                break;
            case AI_State.STATE_ATTACK:
                if (TargetPos != Target.Pos)//TODO Тут?
                {
                    TargetPos = Target.Pos;
                    List<LocalPos> buf = Pathfinder.MakePath((World.CurrentMap as LocalMap).GetBlockMatrix(), Pos, TargetPos);//TODO Тут?
                    buf.Reverse();
                    Path = new Stack<LocalPos>(buf);
                    Path.Pop();
                }
                if (HexNavigHelper.IsMapCoordsAdjacent(TargetPos, Pos, true))
                {
                    PerformAttack(Target);
                }
                else
                {
                    Move();
                }
                break;
        }
    }
}
