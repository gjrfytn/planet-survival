using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour 
{
	public delegate void VoidDelegate();

	public event VoidDelegate UIShowed=delegate{};
	public event VoidDelegate UIHided=delegate{};

	public void OnUIShow()
	{
		UIShowed();
	}

	public void OnUIHide()
	{
		UIHided();
	}

	public event VoidDelegate TurnMade=delegate{};

	public void OnTurn()
	{
		TurnMade();
	}
	
	public event VoidDelegate LocalMapLeft=delegate{};

	public void OnLocalMapLeave()
	{
		LocalMapLeft();
	}
}
