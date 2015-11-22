using UnityEngine;
using System.Collections;

public class EventTimer : MonoBehaviour {
	
	public float TimeCountdown = 30f;
	// Use this for initialization
	void Start () {
	
	}
	
	void Update () {
		if(TimeCountdown > 0){
			TimeCountdown -= Time.deltaTime;
		}else if(TimeCountdown <= 0){
			Debug.Log("GAME OVER");
		}
	}
}
