using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Creature : Entity
{
    public enum AI_State : byte { STATE_IDLE, STATE_MOVE, STATE_ATTACK };

    public float MoveAnimTime;
    [Range(0, 255)]
    public byte MaxHealth;
    [Range(0, 255)]
    public byte BaseDamage;
    public const float DamageSpread = 0.1f;
    [Range(0, 1)]
    public float BaseAccuracy;
    [Range(0, 1)]
    public float BaseArmor;
    public ushort Experience;

    public byte Health { get; private set; }

    protected bool Moving { get; private set; }
    World World;
    Vector2 TargetCoords;
    float MoveTime;
    Vector2 PreviousCoords;
    AI_State State;
    GameObject Target;
    Stack<Vector2> Path;

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

    protected virtual void Start()
    {
        Health = MaxHealth;
        World = GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World;
    }

    protected virtual void Update()
    {
        if (Moving)
        {
            if (MoveTime > 0)
            {
                float tstep = MoveTime / Time.deltaTime;
                MoveTime -= Time.deltaTime;
                //TODO Возможно стоит сохранять значение из GetTransformPosFromMapCoords(MapCoords,World.IsCurrentMapLocal())), так как это улучшит(?) производительность
                float dstep = Vector2.Distance(transform.position, WorldVisualiser.GetTransformPosFromMapCoords(MapCoords, World.IsCurrentMapLocal())) / tstep; //TODO IsCurrentMapLocal - временно?
                transform.position = Vector2.MoveTowards(transform.position, WorldVisualiser.GetTransformPosFromMapCoords(MapCoords, World.IsCurrentMapLocal()), dstep);
            }
            else
                Moving = false;
        }
    }

    public virtual void MoveToMapCoords(Vector2 mapCoords)
    {
        State = AI_State.STATE_MOVE;
        TargetCoords = mapCoords;
        if (World.IsCurrentMapLocal())//TODO Временно?
        {
            List<Vector2> buf = Pathfinder.MakePath((World.CurrentMap as LocalMap).BlockMatrix, MapCoords, TargetCoords);//TODO Тут?
            buf.Reverse();
            Path = new Stack<Vector2>(buf);
            Path.Pop();
        }
    }

    public void Attack(GameObject target)
    {
        State = AI_State.STATE_ATTACK;
        Target = target;
        TargetCoords = Target.GetComponent<Creature>().MapCoords; //TODO Нужно?
        World = GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World;//TODO Костыль
        List<Vector2> buf = Pathfinder.MakePath((World.CurrentMap as LocalMap).BlockMatrix, MapCoords, TargetCoords);//TODO Тут?
        buf.Reverse();
        Path = new Stack<Vector2>(buf);
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
        if (World.IsCurrentMapLocal())//TODO Временно?
        {
            Vector2 node = Path.Pop();
            if (!World.IsHexFree(node))
            {
                List<Vector2> buf = Pathfinder.MakePath((World.CurrentMap as LocalMap).BlockMatrix, MapCoords, TargetCoords);//TODO Тут?
                buf.Reverse();
                Path = new Stack<Vector2>(buf);
                Path.Pop();
                node = Path.Pop();
            }
            EventManager.OnCreatureMove(MapCoords, node);
            MapCoords = node;
        }
        else
        {
            sbyte dx = (sbyte)(TargetCoords.x - MapCoords.x);
            sbyte dy = (sbyte)(TargetCoords.y - MapCoords.y);

            if (dx != 0)
                dx = (sbyte)(dx > 0 ? 1 : -1);
            if (dy != 0)
                dy = (sbyte)(dy > 0 ? 1 : -1);

            Vector2 buf = MapCoords;
            MapCoords.x += dx;
            MapCoords.y += dy;
            EventManager.OnCreatureMove(buf, MapCoords);
        }
        MoveTime = MoveAnimTime;
        Moving = true;
    }

    void PerformAttack()
    {
        if (Random.value < BaseAccuracy)
            Target.GetComponent<Creature>().TakeDamage((byte)(BaseDamage + Random.Range(-BaseDamage * DamageSpread, BaseDamage * DamageSpread)));
        else
            EventManager.OnAttackMiss(transform.position);
    }

    public void TakeDamage(byte damage)
    {
        Debug.Assert(damage >= 0);
        EventManager.OnCreatureHit(gameObject, damage);
        Health = (byte)((Health - damage * (1 - BaseArmor) > 0) ? Health - damage * (1 - BaseArmor) : 0);
        if (Health == 0)
            Destroy(gameObject);
    }

    public void TakeHeal(byte heal)
    {
        Debug.Assert(heal >= 0);
        Health = (byte)Mathf.Clamp(Health + heal, 0, MaxHealth);
    }

    void MakeTurn()
    {
        switch (State)
        {
            case AI_State.STATE_IDLE:
                break;
            case AI_State.STATE_MOVE:
                if (TargetCoords == MapCoords)
                    Idle();
                else
                    Move();
                break;
            case AI_State.STATE_ATTACK:
                if (TargetCoords != Target.GetComponent<Creature>().MapCoords)//TODO Тут?
                {
                    TargetCoords = Target.GetComponent<Creature>().MapCoords;
                    List<Vector2> buf = Pathfinder.MakePath((World.CurrentMap as LocalMap).BlockMatrix, MapCoords, TargetCoords);//TODO Тут?
                    buf.Reverse();
                    Path = new Stack<Vector2>(buf);
                    Path.Pop();
                }
                if (HexNavigHelper.IsMapCoordsAdjacent(TargetCoords, MapCoords, true))
                {
                    PerformAttack();
                }
                else
                {
                    Move();
                }
                break;
        }
    }
}
