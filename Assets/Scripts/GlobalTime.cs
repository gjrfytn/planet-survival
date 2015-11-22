using UnityEngine;
using System.Collections;

public class GlobalTime : MonoBehaviour
{	
	public int day;
	public int hour;
	public int minute;
	public float second;
	
	void Start ()
	{
		hour = Random.Range (0, 23);
		minute = Random.Range (0, 59);
	}

	void Update ()
	{
		second += Time.deltaTime;
		if (second >= 1f) 
		{
			second = 0;
			minute++;
		
			if (minute == 60) 
			{
				minute = 0;
				hour++;

				if (hour == 24) 
				{
					hour = 0;
					day++;
				}
			}
		}
	}
}
