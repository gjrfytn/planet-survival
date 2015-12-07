using UnityEngine;
using System.Collections;

public class MapSwitchTest : MonoBehaviour 
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
	
	public void OnClick()
	{
		//TODO !!! Возможно состояние гонки, если переключение карты будет осуществляться позже, чем вызов этого метода.
		World_.GetComponent<WorldVisualiser>().DestroyAllHexes(); //TODO Это лучше делать до генерации карты, чтобы не было видно подвисания (или нужно отображение загрузки).
		Player.transform.position=new Vector3(Player.GetComponent<PlayerData>().MapCoords.x*0.48f,Player.GetComponent<PlayerData>().MapCoords.y*0.64f+((Player.GetComponent<PlayerData>().MapCoords.x%2)!=0?1:0)*0.32f,-0.1f);
		Camera.transform.position=new Vector3(Player.transform.position.x,Player.transform.position.y,Camera.transform.position.z);
		World_.GetComponent<WorldVisualiser>().RenderVisibleHexes(Player.GetComponent<PlayerData>().MapCoords,Player.GetComponent<PlayerData>().ViewDistance,World_.GetComponent<World>().CurrentMap);
	}
}
