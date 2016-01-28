using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour 
{
	GameObject Player;

	void OnEnable()
	{
		EventManager.PlayerObjectMoved+=FollowPlayer;
	}
	
	void OnDisable()
	{
		EventManager.PlayerObjectMoved-=FollowPlayer;
	}

	void Start () 
	{
		Player=GameObject.FindWithTag("Player");
	}
	
	void FollowPlayer()
	{
		transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y + transform.position.z * (Mathf.Tan((360 - transform.rotation.eulerAngles.x) / 57.3f)), transform.position.z);
	}
}
