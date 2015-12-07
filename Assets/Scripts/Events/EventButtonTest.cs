using UnityEngine;
using System.Collections;

public class EventButtonTest : MonoBehaviour 
{
	[HideInInspector]
	public byte Index;
	
	public void Click()
	{
		GetComponentInParent<EventManager>().EventPanelButtonPress(Index);
	}
}
