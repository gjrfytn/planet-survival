using UnityEngine;
using System.Collections;

public class HexClick : MonoBehaviour
{
	private GameObject Player;
	private GameObject Camera;
	private GameObject World_;

	void Start ()
	{
		Player=GameObject.FindWithTag("Player");
		Camera=GameObject.FindWithTag("MainCamera");
		World_=GameObject.FindWithTag("World");
	}


	public void OnMouseDown ()
	{
		//TODO Сюда нужно написать то, что будет происходить при клике на хекс (переход игрока на этот хекс, генерация карты и т.п.)
		float dist=Vector2.Distance(Player.GetComponent<PlayerData>().MapCoords,GetComponent<HexData>().MapCoords);
		if(dist<1.5f&&dist!=0)
		{
			Player.transform.position=new Vector3(transform.position.x,transform.position.y,-0.1f);
			Player.GetComponent<PlayerData>().MapCoords=GetComponent<HexData>().MapCoords;
			Camera.transform.position=new Vector3(transform.position.x,transform.position.y-5,Camera.transform.position.z);
			World_.GetComponent<WorldVisualiser>().RenderVisibleHexes(Player.GetComponent<PlayerData>().MapCoords,Player.GetComponent<PlayerData>().ViewDistance,World_.GetComponent<World>().CurrentMap);
	 	}
	}
}
