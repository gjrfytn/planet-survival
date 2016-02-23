using UnityEngine;
using System.Collections;

public class Creature : Entity
{
    public enum AI_State : byte { STATE_IDLE, STATE_MOVE, STATE_ATTACK };

    public float MoveAnimationTime;
    public float MaxHealth;
    public float Damage;
    public float Armor;
    //public float Experience;

    public float Health { get; private set; }

    World World;
    protected bool Moving { get; private set; }
    Vector2 TargetCoords;
    float MoveTime;
    Vector2 PreviousCoords;
    AI_State State;
    GameObject Target;

    protected new void OnEnable()
    {
        base.OnEnable();
        EventManager.TurnMade += MakeTurn;
    }

    protected new void OnDisable()
    {
        base.OnDisable();
        EventManager.TurnMade -= MakeTurn;
    }

    protected void Start()
    {
        Health = MaxHealth;
        World = GameObject.FindWithTag("World").GetComponent<World>();
    }

    protected void Update()
    {
        if (Moving)
        {
            if (MoveTime > 0)
            {
                float tstep = MoveTime / Time.deltaTime;
                MoveTime -= Time.deltaTime;
                float dstep = Vector2.Distance(transform.position, WorldVisualiser.GetTransformPosFromMapCoords(MapCoords)) / tstep;
                transform.position = Vector2.MoveTowards(transform.position, WorldVisualiser.GetTransformPosFromMapCoords(MapCoords), dstep);
            }
            else
                Moving = false;
        }
    }

    public void MoveToMapCoords(Vector2 mapCoords)
    {
        State = AI_State.STATE_MOVE;
        TargetCoords = mapCoords;
    }

    public void Attack(GameObject target)
    {
        State = AI_State.STATE_ATTACK;
        Target = target;
        TargetCoords = Target.GetComponent<Creature>().MapCoords;
    }

    public void Idle()
    {
        Target = null;
        State = AI_State.STATE_IDLE;
    }

    void Move()
    {
        sbyte dx = (sbyte)(TargetCoords.x - MapCoords.x);
        sbyte dy = (sbyte)(TargetCoords.y - MapCoords.y);

        if (dx != 0)
            dx = (sbyte)(dx > 0 ? 1 : -1);
        if (dy != 0)
            dy = (sbyte)(dy > 0 ? 1 : -1);

        if (!World.IsHexFree(new Vector2(MapCoords.x + dx, MapCoords.y + dy)))
        {
            if (!World.IsHexFree(new Vector2(MapCoords.x, MapCoords.y + dy)))
            {
                if (!World.IsHexFree(new Vector2(MapCoords.x + dx, MapCoords.y)))
                {
                    Debug.Log("Pathfind error.");
                    return;
                }
                else
                    dy = 0;
            }
            else
                dx = 0;
        }
        Vector2 buf = MapCoords;
        MapCoords.x += dx;
        MapCoords.y += dy;
        EventManager.OnCreatureMove(buf, MapCoords);

        MoveTime = MoveAnimationTime;
        Moving = true;
    }

    void PerformAttack()
    {
        Target.GetComponent<Creature>().TakeDamage(Damage);
    }

    public void TakeDamage(float damage)
    {
        Debug.Assert(damage >= 0);
        EventManager.OnCreatureHit(transform.position, damage);
        Health -= Mathf.Clamp(damage - Armor, 0, damage);
        if (Health <= 0)
            Destroy(gameObject);
    }

    public void TakeHeal(float heal)
    {
        Debug.Assert(heal >= 0);
        Health = Mathf.Clamp(Health + heal, 0, MaxHealth);
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
                TargetCoords = Target.GetComponent<Creature>().MapCoords;
                if (World.IsMapCoordsAdjacent(TargetCoords, MapCoords))
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
