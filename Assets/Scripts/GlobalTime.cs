using UnityEngine;
using System.Collections;

public class GlobalTime : MonoBehaviour {
	
	public int day;
	public int hour;
	public int minute;
	public float second;

	// Use this for initialization
	void Start () {
		hour = Random.Range(0, 23);
		minute = Random.Range (0, 59);
	}
	
	// Update is called once per frame
	void Update () {
		second += Time.deltaTime;
		if (second >= 1f) {
			second = 0;
			minute += 1;
		}
		if (minute == 60){
			minute = 0;
			hour += 1;
		}
		if (hour == 24){
			hour = 0;
			day += 1;
		}
	}
}
