﻿using UnityEngine;
using System.Collections;

public class HexClick : MonoBehaviour
{
	private GameObject Player;
	private GameObject Camera;
	private GameObject World_;
	private GameObject EventManager;

	void Start ()
	{
		Player=GameObject.FindWithTag("Player");
		Camera=GameObject.FindWithTag("MainCamera");
		World_=GameObject.FindWithTag("World");
		EventManager=GameObject.FindWithTag("EventManager");
	}


	public void OnMouseDown ()
	{
		float dist=Vector2.Distance(Player.GetComponent<PlayerData>().MapCoords,GetComponent<HexData>().MapCoords);
		if(dist<1.5f&&dist!=0)
		{
			Player.transform.position=new Vector3(transform.position.x,transform.position.y,-0.1f);
			Player.GetComponent<PlayerData>().MapCoords=GetComponent<HexData>().MapCoords;
			Camera.transform.position=new Vector3(transform.position.x,transform.position.y,Camera.transform.position.z);
			World_.GetComponent<WorldVisualiser>().RenderVisibleHexes(Player.GetComponent<PlayerData>().MapCoords,Player.GetComponent<PlayerData>().ViewDistance,World_.GetComponent<World>().CurrentMap);
			EventManager.GetComponent<EventManager>().MakeActionEvent();
		}
	}
}
