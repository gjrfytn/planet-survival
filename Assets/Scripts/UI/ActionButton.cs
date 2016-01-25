using UnityEngine;
using System.Collections;

public class ActionButton : MonoBehaviour 
{
	public GameObject Entity;

	public void Click()
	{
		Entity.GetComponent<PopupButtons>().ButtonClick(gameObject);
		Entity.GetComponent<Creature>().TakeDamage(10);///Временно
	}
}
