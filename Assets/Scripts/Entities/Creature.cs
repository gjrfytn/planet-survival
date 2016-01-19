using UnityEngine;
using System.Collections;

public class Creature : Entity
{
	public enum AI_State{STATE_IDLE, STATE_MOVE, STATE_ATTACK};

	public GameObject World; //TODO Возможно, надо будет перенести выше по иерархии наследования.

	public float MoveAnimationTime;

	public float MaxHealth;
	public float Health;
	
	public float Damage;
	public float Armor;
	//public float Experience;
	
	bool Moving;
	Vector2 TargetCoords;
	float MoveTime;
	Vector2 PreviousCoords;
	AI_State State;
	GameObject Target;

	protected void Update ()
	{
		if (Moving) 
		{
			MoveTime -= Time.deltaTime;
			float tstep = MoveTime / Time.deltaTime;
			float dstep = Vector2.Distance (transform.position, World.GetComponent<WorldVisualiser> ().GetTransformPosFromMapCoords (MapCoords)) / tstep;
			if (MoveTime > 0) 
				transform.position = Vector2.MoveTowards (transform.position, World.GetComponent<WorldVisualiser> ().GetTransformPosFromMapCoords (MapCoords), dstep);
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
		Health -= Mathf.Clamp (damage - Armor, 0, damage);
	}

	public void MakeTurn ()
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
			if (World.GetComponent<World> ().IsMapCoordsAdjacent (TargetCoords, MapCoords)) 
			{
				PerformAttack ();
			}
			else 
			{
				TargetCoords = Target.GetComponent<Creature> ().MapCoords;
				Move ();
			}
			break;
		}
	}
}
