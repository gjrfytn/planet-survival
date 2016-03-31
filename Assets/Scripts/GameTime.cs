using UnityEngine;
using System.Collections;

public class GameTime : MonoBehaviour
{	
	public const float GameMinToRealSec=0.08333f;
	public uint TimeInMinutes{get;private set;}

	byte Buffer=0;

	void OnEnable()
	{
		EventManager.ActionStarted += RunForMinutes;
	}
	
	void OnDisable()
	{
		EventManager.ActionStarted -= RunForMinutes;
	}

	void RunForMinutes(ushort count)
	{
		StartCoroutine(RunTimeCoroutine(count));
	}

	IEnumerator RunTimeCoroutine(ushort count)
	{
		ushort t=0;
		while(t!=count)
		{
			EventManager.OnActionProgress((float)t/count);
			t++;
			Buffer++;
			if(Buffer==60)
			{
				EventManager.OnHourPass();
				Buffer=0;
			}
			TimeInMinutes++;
			yield return new WaitForSeconds(GameMinToRealSec);//TODO Временно
		}
		EventManager.OnActionEnd();
	}
}
