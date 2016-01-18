using UnityEngine;
using System.Collections;

public class HexInteraction : MonoBehaviour
{
	private GameObject Player;
	private GameObject Camera;
	private GameObject World;
	private GameObject EventManager;

	void Start ()
	{
		Player=GameObject.FindWithTag("Player");
		Camera=GameObject.FindWithTag("MainCamera");
		World=GameObject.FindWithTag("World");
		EventManager=GameObject.FindWithTag("EventManager");
	}


	public void OnMouseDown ()
	{
		if(World.GetComponent<World>().IsMapCoordsAdjacent(Player.GetComponent<Player>().MapCoords,GetComponent<HexData>().MapCoords))
		{
			Player.GetComponent<Player>().MoveToMapCoords(GetComponent<HexData>().MapCoords);
			Player.GetComponent<Player>().MakeTurn();
			Camera.transform.position=new Vector3(transform.position.x,transform.position.y+Camera.transform.position.z*(Mathf.Tan((360-Camera.transform.rotation.eulerAngles.x)/57.3f)),Camera.transform.position.z);
			World.GetComponent<World>().OnGotoHex();
			//EventManager.GetComponent<EventManager>().MakeActionEvent();
		}
	}

	public void OnMouseEnter ()
	{
		GetComponent<SpriteRenderer>().material.color=GetComponent<SpriteRenderer>().material.color*1.5f;
	}

	public void OnMouseExit ()
	{
		GetComponent<SpriteRenderer>().material.color=GetComponent<SpriteRenderer>().material.color*0.666f;
	}
}
