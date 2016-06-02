using UnityEngine;
using System.Collections.Generic;

public class Creature : LivingBeing
{
    public enum AI_State : byte { STATE_IDLE, STATE_MOVE, STATE_ATTACK };

    LocalMap Map;
    LocalPos TargetPos;
    AI_State State;
    LivingBeing Target;
    Stack<LocalPos> Path = new Stack<LocalPos>(); //TODO В LivingBeing?
    float MoveTime;

    Animator Animator;

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

    void Awake()
    {
        Map = GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World.CurrentMap as LocalMap;

        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (MoveTime > 0)
        {
            float tstep = MoveTime / Time.deltaTime;
            MoveTime -= Time.deltaTime;

            float dstep = Vector2.Distance(transform.position, WorldVisualiser.GetTransformPosFromMapPos(Pos)) / tstep;
            transform.position = Vector2.MoveTowards(transform.position, WorldVisualiser.GetTransformPosFromMapPos(Pos), dstep);
        }
        else
            Animator.SetBool("Moving", false);
    }

    public void MoveTo(LocalPos pos)
    {
        State = AI_State.STATE_MOVE;
        TargetPos = pos;
    }

    public void Attack(LivingBeing target)
    {
        State = AI_State.STATE_ATTACK;
        Target = target;
        TargetPos = Target.Pos;

        Animator.SetBool("Agressive", true);
    }

    public void Idle()
    {
        Target = null;
        Path.Clear();
        State = AI_State.STATE_IDLE;

        Animator.SetBool("Agressive", false);
    }

    void Move()
    {
        LocalPos node = new LocalPos();
        bool noPath = false;
        if (Path.Count == 0)
            noPath = true;
        else
            node = Path.Pop();
        if (noPath || Map.IsBlocked(node))
        {
            List<LocalPos> buf = Pathfinder.MakePath(Map.GetBlockMatrix(), Pos, TargetPos);
            buf.Reverse();
            Path = new Stack<LocalPos>(buf);
            Path.Pop();
            node = Path.Pop();
        }
        LocalPos pBuf = Pos;
        Pos = node;
        EventManager.OnCreatureMove(pBuf, Pos); //TODO name?

        MoveTime = MoveAnimTime;

        Animator.SetBool("Moving", true);
        if (WorldVisualiser.GetTransformPosFromMapPos(pBuf).x - WorldVisualiser.GetTransformPosFromMapPos(Pos).x > 0)
            gameObject.transform.rotation = new Quaternion(0, 180, 0, 0);
        else
            gameObject.transform.rotation = Quaternion.identity;
    }

    void PerformAttack(LivingBeing target)
    {
        if (Random.value < BaseAccuracy)
            target.TakeDamage((byte)(BaseDamage + Random.Range(-BaseDamage * DamageSpread, BaseDamage * DamageSpread)), true);
        else
            EventManager.OnAttackMiss(Pos);

        Animator.SetTrigger("Attack");
        if (WorldVisualiser.GetTransformPosFromMapPos(Pos).x - WorldVisualiser.GetTransformPosFromMapPos(target.Pos).x > 0)
            gameObject.transform.rotation = new Quaternion(0, 180, 0, 0);
        else
            gameObject.transform.rotation = Quaternion.identity;
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
                if (TargetPos != Target.Pos)
                {
                    TargetPos = Target.Pos;
                    Path.Clear();
                }
                if (HexNavigHelper.IsMapCoordsAdjacent(TargetPos, Pos, true))
                {
                    Path.Clear();
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
