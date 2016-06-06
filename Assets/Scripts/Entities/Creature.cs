using UnityEngine;
using System.Collections.Generic;

public class Creature : LivingBeing
{
    public enum AI_State : byte { STATE_IDLE, STATE_MOVE, STATE_ATTACK };
    public bool AttackingPlayer { get; private set; }

    LocalMap Map;
    LocalPos TargetPos;
    AI_State State;
    LivingBeing Target;
    Stack<LocalPos> Path = new Stack<LocalPos>(); //TODO В LivingBeing?
    float MoveTime;

    LocalPos NextMovePoint;
    Stack<LocalPos> MovePath = new Stack<LocalPos>();

    byte RemainingMoves;

    Animator Animator;

    void OnEnable()
    {
        EventManager.PlayerMadeTurn += MakeTurn;
    }

    void OnDisable()
    {
        EventManager.PlayerMadeTurn -= MakeTurn;
    }

    void Awake()
    {
        Map = GameObject.FindWithTag("World").GetComponent<World>().CurrentMap as LocalMap;
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (MoveTime > 0)
        {
            float tstep = MoveTime / Time.deltaTime;
            MoveTime -= Time.deltaTime;

            float dstep = Vector2.Distance(transform.position, WorldVisualiser.GetTransformPosFromMapPos(NextMovePoint)) / tstep;
            transform.position = Vector2.MoveTowards(transform.position, WorldVisualiser.GetTransformPosFromMapPos(NextMovePoint), dstep);
        }
        else if (MovePath.Count != 0)
        {
            MoveTime = MoveAnimTime;
            NextMovePoint = MovePath.Pop();

            if (transform.position.x - WorldVisualiser.GetTransformPosFromMapPos(NextMovePoint).x > 0)
                transform.rotation = new Quaternion(0, 180, 0, 0);
            else
                transform.rotation = Quaternion.identity;
        }
        else
        {
            Animator.SetBool("Moving", false);

            if (RemainingMoves == 0)
                EventManager.OnCreatureEndTurn();
            else
                Think();
        }
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

        if (target is Player)
            AttackingPlayer = true;

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
        if (Path.Count == 0)
        {
            List<LocalPos> buf = Pathfinder.MakePath(Map.GetBlockMatrix(), Pos, TargetPos, true);
            buf.Reverse();
            Path = new Stack<LocalPos>(buf);
            Path.Pop();
        }
        else
        {
            bool pathIsObsolete = false;
            LocalPos[] arrBuf = Path.ToArray();
            foreach (LocalPos pos in arrBuf)
                if (Map.IsBlocked(pos))
                {
                    pathIsObsolete = true;
                    break;
                }
            if (pathIsObsolete)
            {
                List<LocalPos> buf = Pathfinder.MakePath(Map.GetBlockMatrix(), Pos, TargetPos, true);
                buf.Reverse();
                Path = new Stack<LocalPos>(buf);
                Path.Pop();
            }
        }

        byte movesCount = (byte)Mathf.Min(Speed, Path.Count);
        RemainingMoves = (byte)(Speed - movesCount);

        List<LocalPos> lstBuf = new List<LocalPos>(movesCount);
        for (byte i = 0; i < movesCount; ++i)
        {
            lstBuf.Add(Path.Pop());
        }
        lstBuf.Reverse();
        MovePath = new Stack<LocalPos>(lstBuf);

        LocalPos pBuf = Pos;
        Pos = lstBuf[0];
        EventManager.OnCreatureMove(pBuf, Pos); //TODO name?

        Animator.SetBool("Moving", true);
    }

    void PerformAttack(LivingBeing target, float damage, float accuracy)
    {
        if (Random.value < accuracy)
            target.TakeDamage((byte)damage, true);
        else
            EventManager.OnAttackMiss(transform.position);

        Animator.SetTrigger("Attack");
        if (transform.position.x - WorldVisualiser.GetTransformPosFromMapPos(target.Pos).x > 0)
            transform.rotation = new Quaternion(0, 180, 0, 0);
        else
            transform.rotation = Quaternion.identity;
    }

    void MakeTurn()
    {
        EventManager.OnCreatureStartTurn();
        Think();
    }

    void Think()
    {
        switch (State)
        {
            case AI_State.STATE_IDLE:
                EventManager.OnCreatureEndTurn();
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
                    PerformAttack(Target, BaseWeapon.NormalHitDamage, 0.75f);//TODO Временно normal, 0.85f
                    RemainingMoves = 0;
                    EventManager.OnCreatureEndTurn();
                }
                else
                {
                    Move();
                }
                break;
        }
    }
}
