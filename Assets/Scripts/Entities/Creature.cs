using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using LocalPos = U16Vec2;
using GlobalPos = S32Vec2;

public class Creature : LivingBeing
{
    [System.Flags]
    public enum AggrTarget : byte
    {
        NONE = 0x0,
        PLAYER = 0x1,
        NONPLAYER = 0xFE,
        EVERYBODY = 0xFF
    };

    enum AI_State : byte { IDLE, MOVE, ATTACK };

    public AggrTarget AggressiveTo;

    [SerializeField, Range(0, 1)]
    float BaseArmor;

    [SerializeField]
    DefenceAction DefenceAction;

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

    void Awake()
    {
        Map = GameObject.FindWithTag("World/World").GetComponent<World>().CurrentMap as LocalMap;
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (MakingTurn)
        {
            if (MoveTime > 0)
            {
                float tstep = MoveTime / Time.deltaTime;
                MoveTime -= Time.deltaTime;

                float dstep = Vector2.Distance(transform.position, WorldVisualizer.GetTransformPosFromMapPos(NextMovePoint)) / tstep;
                transform.position = Vector2.MoveTowards(transform.position, WorldVisualizer.GetTransformPosFromMapPos(NextMovePoint), dstep);
            }
            else if (MovePath.Count != 0)
            {
                MoveTime = MoveAnimTime;
                NextMovePoint = MovePath.Pop();

                if (transform.position.x - WorldVisualizer.GetTransformPosFromMapPos(NextMovePoint).x > 0)
                    transform.rotation = new Quaternion(0, 180, 0, 0);
                else
                    transform.rotation = Quaternion.identity;
            }
            else
            {
                Animator.SetBool("Moving", false);

                if (RemainingMoves == 0)
                {
                    MakingTurn = false;
                    EventManager.OnLivingBeingEndTurn();
                }
                else
                    Think();
            }
        }
    }

    public override void TakeDamage(byte damage, bool applyArmor, bool applyDefenceAction)
    {
        //Debug.Assert(damage >= 0);
        if (applyDefenceAction && DefenceAction.TryPerform(ref damage))
            Animator.SetTrigger("Defence");
        damage = (byte)Mathf.RoundToInt(damage * (1 - BaseArmor));
        Health = (byte)(Health - damage > 0 ? Health - damage : 0);
        EventManager.OnCreatureHit(this, damage);
    }

    public void MoveTo(LocalPos pos)
    {
        State = AI_State.MOVE;
        TargetPos = pos;
    }

    void Attack(LivingBeing target)
    {
        State = AI_State.ATTACK;
        Target = target;
        TargetPos = Target.Pos;

        Animator.SetBool("Agressive", true);
    }

    void Idle()
    {
        Target = null;
        Path.Clear();
        State = AI_State.IDLE;

        Animator.SetBool("Agressive", false);
    }

    void Move()
    {
        if (Path.Count == 0)
        {
            List<LocalPos> buf = Pathfinder.MakePath(Map.GetBlockMatrix(), Pos, TargetPos, true);
            if (buf == null)
            {
                Idle(); //?
                return;
            }
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
                if (buf == null)
                {
                    Idle(); //?
                    return;
                }
                buf.Reverse();
                Path = new Stack<LocalPos>(buf);
                Path.Pop();
            }
        }

        byte movesCount = (byte)Mathf.Min(RemainingMoves, Path.Count);
        RemainingMoves -= movesCount;

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

    void StartAttack(float damage, float accuracy)
    {
        RemainingMoves = 0;
        MakingTurn = false;
        if (Random.value < accuracy)
        {
            EventManager.PlayerDefended += FinishAttack;
            Target.TakeDamage((byte)damage, true, true);
        }
        else
        {
            EventManager.OnAttackMiss(transform.position);
            EventManager.OnLivingBeingEndTurn();
        }
    }

    void FinishAttack()
    {
        EventManager.PlayerDefended -= FinishAttack;
        Animator.SetTrigger("Attack");
        if (transform.position.x - WorldVisualizer.GetTransformPosFromMapPos(Target.Pos).x > 0)
            transform.rotation = new Quaternion(0, 180, 0, 0);
        else
            transform.rotation = Quaternion.identity;

        EventManager.OnLivingBeingEndTurn();
    }

    LivingBeing FindTarget()
    {
        switch (AggressiveTo)
        {
            case AggrTarget.PLAYER:
                Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
                if (HexNavigHelper.Distance(Pos, player.Pos, true) < ViewRange)
                    return player;
                else
                    return null;
            case AggrTarget.EVERYBODY:
                List<LivingBeing> buf = Map.GetAllLivingBeings();
                buf.Remove(this);
                byte closestDist = (byte)buf.Min(lb2 => HexNavigHelper.Distance(Pos, lb2.Pos, true));
                return closestDist < ViewRange ? buf.Find(lb1 => HexNavigHelper.Distance(Pos, lb1.Pos, true) == closestDist) : null;
        }
        return null;
    }

    public override void MakeTurn()
    {
        MakingTurn = true;
        RemainingMoves = Speed;
        Think();
    }

    void Think()
    {
        switch (State)
        {
            case AI_State.IDLE:
                if (AggressiveTo != AggrTarget.NONE)
                {
                    LivingBeing target = FindTarget();
                    if (target != null)
                    {
                        Fighting = true;
                        Attack(target);
                        Think();
                    }
                    else
                    {
                        GlobalPos pos;
                        do
                            pos = HexNavigHelper.GetNeighborMapCoords(Pos, (TurnedHexDirection)Random.Range(0, 6));
                        while (pos.X < 0 || pos.X >= Map.Width || pos.Y < 0 || pos.Y >= Map.Height || Map.IsBlocked((LocalPos)pos));
                        TargetPos = (LocalPos)pos;
                        Move();
                        //MakingTurn = false;
                        //EventManager.OnLivingBeingEndTurn();
                    }
                }
                else
                {
                    GlobalPos pos;
                    do
                        pos = HexNavigHelper.GetNeighborMapCoords(Pos, (TurnedHexDirection)Random.Range(0, 6));
                    while (pos.X < 0 || pos.X >= Map.Width || pos.Y < 0 || pos.Y >= Map.Height || Map.IsBlocked((LocalPos)pos));
                    TargetPos = (LocalPos)pos;
                    Move();
                    //MakingTurn = false;
                    //EventManager.OnLivingBeingEndTurn();
                }
                break;
            case AI_State.MOVE:
                if (TargetPos == Pos)
                    Idle();
                else
                    Move();
                break;
            case AI_State.ATTACK:
                if (AggressiveTo == AggrTarget.NONE)
                    Idle();
                else
                {
                    if (TargetPos != Target.Pos)
                    {
                        TargetPos = Target.Pos;
                        Path.Clear();
                    }
                    if (HexNavigHelper.IsMapCoordsAdjacent(TargetPos, Pos, true))
                    {
                        Path.Clear();
                        StartAttack(BaseWeapon.Damage, 0.75f);//TODO Временно 0.85f
                    }
                    else
                        Move();
                }
                break;
        }
    }
}
