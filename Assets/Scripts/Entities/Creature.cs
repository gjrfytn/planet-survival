using UnityEngine;
using System.Collections;

public class Creature : Entity 
{
	public GameObject World; //TODO Возможно, надо будет перенести выше по иерархии наследования.

	public float MoveAnimationTime;

	public float Health;	

	bool Moving;
	Vector2 TargetCoords;
	float MoveTime;
	Vector2 PreviousCoords;


	void Update()
	{
		if(Moving)
		{
		MoveTime-=Time.deltaTime;
			float tstep=MoveTime/Time.deltaTime;
			float dstep=Vector2.Distance(transform.position,World.GetComponent<WorldVisualiser>().GetTransformPosFromMapCoords(MapCoords))/tstep;
		//GetComponent<SpriteRenderer>().sprite=MoveSprite;
			if(MoveTime>0)//(Vector2)transform.position!=World.GetComponent<WorldVisualiser>().GetTransformPosFromMapCoords(MapCoords))
			{
				transform.position= Vector2.MoveTowards(transform.position, World.GetComponent<WorldVisualiser>().GetTransformPosFromMapCoords(MapCoords),dstep);
			}
			else
			{
			Moving=false;
		//GetComponent<SpriteRenderer>().sprite=IdleSprite;
			}
		}
	}

	public void MoveToMapCoords(Vector2 mapCoords)
	{
		TargetCoords=mapCoords;
	}

	void Move()
	{
		sbyte dx=(sbyte)(TargetCoords.x-MapCoords.x);
		sbyte dy=(sbyte)(TargetCoords.y-MapCoords.y);

		if(dx!=0)
			dx=(sbyte)(dx>0?1:-1);
		if(dy!=0)
			dy=(sbyte)(dy>0?1:-1);
		MapCoords.x +=dx;
		MapCoords.y+=dy;
		MoveTime=MoveAnimationTime;
		Moving=true;
	}
	
	public void MakeTurn()
	{
		Move();
	}
}
