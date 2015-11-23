using UnityEngine;
using System.Collections;

public class EventTimer : MonoBehaviour 
{
	public float TimeCountdown = 30;
		
	void Update () 
	{
		if(TimeCountdown > 0)
			TimeCountdown -= Time.deltaTime;
		else
			Debug.Log("GAME OVER");

	}
}
