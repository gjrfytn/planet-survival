using UnityEngine;
using System.Collections;

public class PlayerData : MonoBehaviour 
{
	[HideInInspector]
	public Vector2 MapCoords=new Vector2(5,5);
	public byte ViewDistance;
}
