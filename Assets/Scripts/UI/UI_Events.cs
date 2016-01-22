using UnityEngine;
using System.Collections;

public class UI_Events : MonoBehaviour
{
	EventManager EventManager;

	void Awake ()
	{
		EventManager = GameObject.Find ("EventManager").GetComponent<EventManager> ();
	}

	void OnEnable ()
	{
		EventManager.OnUIShow ();
	}

	void OnDisable ()
	{
		EventManager.OnUIHide ();
	}
}
