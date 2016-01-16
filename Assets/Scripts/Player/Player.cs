using UnityEngine;
using System.Collections;

public class Player : Creature 
{
	public byte ViewDistance;

	void Start()
	{
		MapCoords=new Vector2(5,5);
		GetComponent<SpriteRenderer>().sortingLayerName="Player";
	}
}
