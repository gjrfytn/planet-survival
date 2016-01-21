using UnityEngine;
using System.Collections;

public class Creature : Entity
{
	public enum AI_State{STATE_IDLE, STATE_MOVE, STATE_ATTACK};

	public float MoveAnimationTime;

	public float MaxHealth;
	
	public float Damage;
	public float Armor;
	//public float Experience;

	public float Health{get;private set;}

	WorldVisualiser Visualiser;

	bool Moving;
	Vector2 TargetCoords;
	float MoveTime;
	Vector2 PreviousCoords;
	AI_State State;
	GameObject Target;

	protected new void OnEnable()
	{
		base.OnEnable();
		EventManager.TurnMade+=MakeTurn;
	}

	protected new void OnDisable()
	{
		base.OnDisable();
		EventManager.TurnMade-=MakeTurn;
	}

	protected void Start()
	{
		Health=MaxHealth;
		Visualiser=GameObject.FindWithTag("World").GetComponent<WorldVisualiser>();
	}

	protected void Update ()
	{
		if (Moving) 
		{
			MoveTime -= Time.deltaTime;
			float tstep = MoveTime / Time.deltaTime;
			float dstep = Vector2.Distance (transform.position, Visualiser.GetTransformPosFromMapCoords (MapCoords)) / tstep;
			if (MoveTime > 0) 
				transform.position = Vector2.MoveTowards (transform.position, Visualiser.GetTransformPosFromMapCoords (MapCoords), dstep);
			else 
				Moving = false;
		}
	}

	public void MoveToMapCoords (Vector2 mapCoords)
	{
		State = AI_State.STATE_MOVE;
		TargetCoords = mapCoords;
	}

	public void Attack (GameObject target)
	{
		State = AI_State.STATE_ATTACK;
		Target = target;
		TargetCoords = Target.GetComponent<Creature> ().MapCoords;
	}

	public void Idle ()
	{
		Target = null;
		State = AI_State.STATE_IDLE;
	}

	void Move ()
	{
		sbyte dx = (sbyte)(TargetCoords.x - MapCoords.x);
		sbyte dy = (sbyte)(TargetCoords.y - MapCoords.y);

		if (dx != 0)
			dx = (sbyte)(dx > 0 ? 1 : -1);
		if (dy != 0)
			dy = (sbyte)(dy > 0 ? 1 : -1);
		MapCoords.x += dx;
		MapCoords.y += dy;
		MoveTime = MoveAnimationTime;
		Moving = true;
	}

	void PerformAttack ()
	{
		Target.GetComponent<Creature> ().TakeDamage (Damage);
	}

	public void TakeDamage (float damage)
	{
		Debug.Assert(damage>=0);
		Health -= Mathf.Clamp (damage - Armor, 0, damage);
		if(Health<=0)
			Destroy(gameObject);
	}

	public void TakeHeal(float heal)
	{
		Debug.Assert(heal>=0);
		Health=Mathf.Clamp(Health+heal,0,MaxHealth);
	}

	void MakeTurn ()
	{
		switch (State) 
		{
		case AI_State.STATE_IDLE:
			break;
		case AI_State.STATE_MOVE:
			if (TargetCoords == MapCoords)
				Idle ();
			else
				Move ();
			break;
		case AI_State.STATE_ATTACK:
			TargetCoords = Target.GetComponent<Creature> ().MapCoords;
			if (World.IsMapCoordsAdjacent (TargetCoords, MapCoords)) 
			{
				PerformAttack ();
			}
			else 
			{
				Move ();
			}
			break;
		}
	}
}
